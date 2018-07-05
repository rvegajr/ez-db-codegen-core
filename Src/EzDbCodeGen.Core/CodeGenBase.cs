using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using HandlebarsDotNet;
using Newtonsoft.Json;
using EzDbCodeGen.Core.Enums;
using EzDbSchema.Core.Interfaces;
using EzDbCodeGen.Core.Compare;
using EzDbCodeGen.Internal;
using EzDbSchema.Core;
using EzDbCodeGen.Core.Extentions.Strings;

namespace EzDbCodeGen.Core
{
    public abstract class CodeGenBase
    {
        public static string OP_FILE = "<FILE>";
        public static string OP_ENTITY_KEY = "<ENTITY_KEY>";
        public static string OP_OUTPUT_PATH = "<OUTPUT_PATH>";
        public static string OP_FILE_END = "</FILE>";
        public static string OP_ENTITY_KEY_END = "</ENTITY_KEY>";
        public static string OP_OUTPUT_PATH_END = "</OUTPUT_PATH>";
        public static string VAR_THIS_PATH = "$THIS_PATH$";

        public virtual string TemplatePath { get; set; } = "";
        public virtual string OutputPath { get; set; } = "";
        public virtual string ConnectionString { get; set; } = "";
        public TemplatePathOption TemplatePathOption { get; set; } = TemplatePathOption.Auto;
        public virtual bool VerboseMessages { get; set; } = true;
        public virtual string SchemaName { get; set; } = "MyEzSchema";
        public IDatabase Schema { get; set; }
        public static Config.Configuration RzDbConfig = Config.Configuration.Instance;
        public string[] AllowedKeys(IDatabase model)
        {
            return model.Keys.Where(k => !k.EndsWith("_Archive", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        public CodeGenBase()
        {

        }

        public CodeGenBase(string connectionString, string templatePath, string outputPath)
        {
            this.TemplatePath = templatePath;
            this.OutputPath = outputPath;
            this.ConnectionString = connectionString;
        }

        private void StatusMessage(string message, bool Force)
        {
            if (this.VerboseMessages || Force) Console.WriteLine(message);
        }

        private void StatusMessage(string message)
        {
            StatusMessage(message, false);
        }

        private void ErrorMessage(string errorMessage)
        {
            Console.WriteLine(errorMessage);
        }

        /// <summary>
        /// Processes the template using passed Template Inputs and the handlebars template name.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings.
        /// </summary>
        /// <param name="TemplateFileNameOrPath">The file name of a handlebars template or a path that contains handlebars templates. If no path is specified,  the app will prepend the assembly path in front of the text and search there</param>
        /// <param name="templateInput">The template input class,  could be an object of type IDatabase or if type schema</param>
        /// <param name="OutputPath">The output path.  If there is no &lt;FILE&gt;FILENAMEHERE&lt;/FILE&gt; specifier, then this should be a file name,  
        /// if there is a file specifier,  then it will write to the file resolved between the FILE tags.  Note that you can specify and OUTPUT_PATH xml tag
        /// in order to specify and output target (which will override the the path passed through this paramter)</param>
        /// <returns>A return code </returns>
        /// <exception cref="Exception"></exception>
        public ReturnCodes ProcessTemplate(string TemplateFileNameOrPath, ITemplateInput templateInput, string outputPath)
        {
            try
            {
                return ProcessTemplate(TemplateFileNameOrPath, templateInput, null, outputPath);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed on ProcessTemplate. {0}", ex.Message), ex);
            }
        }

        /// <summary>
        /// Processes the template using passed Template Inputs and the handlebars template name.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings.
        /// </summary>
        /// <returns><c>true</c>, if template was processed, <c>false</c> otherwise.</returns>
        /// <param name="TemplateFileNameOrPath">The file name of a handlebars template or a path that contains handlebars templates. If no path is specified,  the app will prepend the assembly path in front of the text and search there</param>
        /// <param name="templateInput">The template input class,  could be an object of type IDatabase or if type schema</param>
        /// <param name="OutputPath">The output path.  If there is no &lt;FILE&gt;FILENAMEHERE&lt;/FILE&gt; specifier, then this should be a file name,  
        /// if there is a file specifier,  then it will write to the file resolved between the FILE tags.  Note that you can specify and OUTPUT_PATH xml tag
        /// in order to specify and output target (which will override the the path passed through this paramter)</param>
        /// <returns>A return code </returns>
        public ReturnCodes ProcessTemplate(string TemplateFileNameOrPath, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource, string outputPath)
        {
            if (!(
                (File.Exists(TemplateFileNameOrPath)) || 
                (Directory.Exists(TemplateFileNameOrPath)
               ))) TemplateFileNameOrPath = ("{ASSEMBLY_PATH}" + TemplateFileNameOrPath).ResolvePathVars();
            if (!((File.Exists(TemplateFileNameOrPath)) || (Directory.Exists(TemplateFileNameOrPath))))
                throw new Exception(string.Format("Template not found in path {0}", TemplateFileNameOrPath));

            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(TemplateFileNameOrPath);

            if (attr.HasFlag(FileAttributes.Directory))
                return ProcessTemplate((DirectoryName)TemplateFileNameOrPath, originalTemplateInputSource, compareToTemplateInputSource, outputPath);
            else
                return ProcessTemplate((FileName)TemplateFileNameOrPath, originalTemplateInputSource, compareToTemplateInputSource, outputPath);
        }

        protected ReturnCodes ProcessTemplate(DirectoryName pathName, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource, string outputPath)
        {
            var filesEndingInHbs = Directory.EnumerateFiles(pathName).Where(f => f.EndsWith("hbs", StringComparison.InvariantCulture));
            var returnCodeList = new ReturnCodes();
            foreach (var templateFullFileName in filesEndingInHbs)
            {
                returnCodeList.Merge(ProcessTemplate((FileName)templateFullFileName, originalTemplateInputSource, compareToTemplateInputSource, outputPath));
            }
            return returnCodeList;
        }

        /// <summary>
        /// Processes the template using passed Template Inputs and the handlebars template name.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings.
        /// </summary>
        /// <returns><c>true</c>, if template was processed, <c>false</c> otherwise.</returns>
        /// <param name="templateFileName">File name of an existing handlebards template file name</param>
        /// <param name="originalTemplateInputSource">Original template input source.  Pass the input to here if you want to generate using only 1 schema</param>
        /// <param name="compareToTemplateInputSource">Optional - Compare to template input source.  This will process only the differences. </param>
        /// <param name="outputPath">The output path.  If there is no &lt;FILE&gt;FILENAMEHERE&lt;/FILE&gt; specifier, then this should be a file name,  if there is a file specifier,  then it will write to the file resolved between the FILE tags</param>
        protected ReturnCodes ProcessTemplate(FileName templateFileName, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource, string outputPath)
		{
            this.OutputPath = outputPath;
            if (!File.Exists(templateFileName)) templateFileName = ("{ASSEMBLY_PATH}" + templateFileName).ResolvePathVars();
            var CurrentTask = "Entering ProcessTemplate";
			var returnCode = ReturnCode.OkNoAddDels;
			try
			{
				CurrentTask = "Peforming Validations";
				if (originalTemplateInputSource == null) throw new Exception(@"There must be an Template Source passed through originalTemplateInputSource!");
				CurrentTask = "Loading Source Schema";
				IDatabase schema = originalTemplateInputSource.LoadSchema();
				if (schema == null) throw new Exception(@"originalTemplateInputSource is not a valid template");

				CurrentTask = "Template path is " + templateFileName;

				string result = "";
				try
				{
                    
					CurrentTask = string.Format("Trying to find Config file");
					string RzDbConfigPath = Internal.AppSettings.Instance.ConfigurationFileName;
					if (File.Exists(RzDbConfigPath))
					{
						CurrentTask = string.Format("Config file found! lets read it");
						RzDbConfig = Config.Configuration.FromFile(RzDbConfigPath);
						foreach (var item in RzDbConfig.PluralizerCrossReference)
							Pluralizer.Instance.AddWord(item.SingleWord, item.PluralWord);
					}

					CurrentTask = string.Format("Reading Template from '{0}'", templateFileName);
					var templateAsString = File.ReadAllText(templateFileName);
                    //Does template have and Output path override? if so, override the local outputDirectory and strip it 
                    if (templateAsString.Contains(CodeGenBase.OP_OUTPUT_PATH))
                    {
                        this.OutputPath = templateAsString.Pluck(CodeGenBase.OP_OUTPUT_PATH, CodeGenBase.OP_OUTPUT_PATH_END, out templateAsString).Trim();

                        if (this.OutputPath.StartsWith("@"))  //An @ at the beginning forces the app to treat this as a path and delete and attempt to recreate it 
                        {
                            this.OutputPath = this.OutputPath.Substring(1);
                            if (Directory.Exists(this.OutputPath)) Directory.Delete(this.OutputPath, true);
                            if (!Directory.Exists(this.OutputPath)) Directory.CreateDirectory(this.OutputPath);
                        }
                        if (this.OutputPath.Contains(CodeGenBase.VAR_THIS_PATH)) this.OutputPath = Path.GetFullPath(this.OutputPath.Replace(CodeGenBase.VAR_THIS_PATH, Path.GetDirectoryName(templateFileName)));
                        templateAsString = templateAsString.Replace(CodeGenBase.OP_OUTPUT_PATH, "").Replace(CodeGenBase.OP_OUTPUT_PATH_END, "").Trim();
                    }

                    if (!File.Exists(templateFileName)) throw new FileNotFoundException("Template File " + templateFileName + " is not found");
                    CurrentTask = string.Format("Checking to see if outpath of {0} exists?", this.OutputPath);

                    CurrentTask = string.Format("Registering Handlbar helpers");
					HandlebarsUtility.RegisterHelpers();
					HandlebarsCsUtility.RegisterHelpers();
					HandlebarsTsUtility.RegisterHelpers();
					CurrentTask = string.Format("Compiling Handlbar Template");
					var template = Handlebars.Compile(templateAsString);
					CurrentTask = string.Format("Rendering Handlbar Template");
					result = template(schema);
				}
				catch (Exception exTemplateError)
				{
					returnCode = ReturnCode.Error;
					ErrorMessage("Error while " + CurrentTask + ". " + exTemplateError.Message);
					throw;
					//throw exRazerEngine;
				}
				finally
				{
					CurrentTask = string.Format("Template Preocess Completed");
				}

                result = result.Replace("<t>", "")
					.Replace("<t/>", "")
					.Replace("<t />", "")
					.Replace("</t>", "")
					.Replace("$OUTPUT_PATH$", this.OutputPath).TrimStart();

				CurrentTask = string.Format("Template Preocess Compeleted");
				/* If the entity key specifier doesn't exist */
				var hasEntityKeySpecifier = result.Contains(CodeGenBase.OP_ENTITY_KEY);
				CurrentTask = string.Format("Does the file contain FILE operator?");
				if (result.Contains(CodeGenBase.OP_FILE)) /* File seperation specifier - this will split by the files specified by  */
				{
                    if (!Directory.Exists(this.OutputPath))  //This does contain a FILE specifier,  so we need to make this a directoy and try to create it if it doesn't exist
                    {
                        this.OutputPath = Path.GetDirectoryName(this.OutputPath) + Path.DirectorySeparatorChar;
                        CurrentTask = string.Format("It doesn't... so lets try to create it");
                        Directory.CreateDirectory(this.OutputPath);
                    }


                    CurrentTask = string.Format("Parsing files");
					/* First, lets get all the files currently in the path */
					var FileActions = new Dictionary<string, TemplateFileAction>();
					string[] FilesinOutputDirectory = Directory.GetFiles(this.OutputPath);
					foreach (var fileName in FilesinOutputDirectory) FileActions.Add(fileName, TemplateFileAction.Unknown);

					var FileListAndContents = new EntityFileDictionary();
					string[] parseFiles = result.Split(new[] { CodeGenBase.OP_FILE }, StringSplitOptions.RemoveEmptyEntries);

					var EntityKey = "";
					foreach (string fileText in parseFiles)
					{
						var filePart = CodeGenBase.OP_FILE + fileText;  //need to add the delimiter to make pluck work as expected
						var FileContents = "";
						string newOutputFileName = filePart.Pluck(CodeGenBase.OP_FILE, CodeGenBase.OP_FILE_END, out FileContents);
						FileContents = FileContents.Replace(CodeGenBase.OP_FILE, "").Replace(CodeGenBase.OP_FILE_END, "").Trim();
						if ((newOutputFileName.Length > 0) && (newOutputFileName.StartsWith(this.OutputPath, StringComparison.Ordinal)))
						{
							EntityKey = "XXX" + Guid.NewGuid().ToString();  /* guaruntee this to be unique */
																			//var FileContents = filePart.Substring(CodeGenBase.OP_FILE_END.Length + 1);
							if (FileContents.Contains(CodeGenBase.OP_ENTITY_KEY))
							{
								EntityKey = FileContents.Pluck(CodeGenBase.OP_ENTITY_KEY, CodeGenBase.OP_ENTITY_KEY_END, out FileContents);
								FileContents = FileContents.Replace(CodeGenBase.OP_ENTITY_KEY, "").Replace(CodeGenBase.OP_ENTITY_KEY_END, "").Trim();
							}
							FileListAndContents.Add(newOutputFileName, EntityKey, FileContents);
						}
					}

					CurrentTask = string.Format("Handling the output file");
					var EffectivePathOption = this.TemplatePathOption;
					IDatabase schemaToCompareTo = null;
					if (EffectivePathOption == TemplatePathOption.Auto)
					{
						EffectivePathOption = TemplatePathOption.Clear;
						if ((compareToTemplateInputSource != null) && (hasEntityKeySpecifier))
						{
							schemaToCompareTo = compareToTemplateInputSource.LoadSchema();
							if (schemaToCompareTo == null) throw new Exception(@"schemaToCompareTo is not a valid template");
							EffectivePathOption = TemplatePathOption.SyncDiff;
						}
					}
					if (EffectivePathOption.Equals(TemplatePathOption.Clear))
					{
						StatusMessage("Path Option is set to 'Clear'");
						foreach (var fileName in FileActions.Keys.ToList()) FileActions[fileName] = TemplateFileAction.Delete;
					}
					else if (EffectivePathOption.Equals(TemplatePathOption.SyncDiff))
					{
						StatusMessage("Path Option is set to 'SyncDiff'");
						var SchemaDiffs = schema.CompareTo(schemaToCompareTo);
						StatusMessage(string.Format("There where {0} differences between the schemas", SchemaDiffs.Count));
						if (SchemaDiffs.Count > 0)
						{
							foreach (var schemaDiff in SchemaDiffs)
							{
								var entityName = schemaDiff.EntityName;
								FileName fileName = "";
								if (schemaDiff.FileAction == TemplateFileAction.Add)
								{
									if (FileActions.ContainsKey(fileName))
									{
										/* this should not happen;  but if it does */
										FileActions[fileName] = TemplateFileAction.Update;
									}
									else
										FileActions.Add(fileName, TemplateFileAction.Add);
								}
								else if (schemaDiff.FileAction == TemplateFileAction.Update)
								{
									if (FileActions.ContainsKey(fileName))
										FileActions[fileName] = TemplateFileAction.Update;
									else
									{
										FileActions.Add(fileName, TemplateFileAction.Add);
									}
								}
								else if (schemaDiff.FileAction == TemplateFileAction.Delete)
								{
									if (FileActions.ContainsKey(fileName))
										FileActions[fileName] = TemplateFileAction.Delete;
									else
									{
										FileActions.Add(fileName, TemplateFileAction.Delete);
									}
								}
							}
						}
					}
					else if (EffectivePathOption.Equals(TemplatePathOption.Update))
					{
						StatusMessage("Path Option is set to 'Update'");
					}

					CurrentTask = string.Format("Queueing file actions");
					//Lets now make sure all of those files that should be rendered are there
					foreach (string fileName in FileListAndContents.ClonePrimaryKeys())
					{
						if ((FileActions.ContainsKey(fileName)))
						{
							//if the file exists and it is slated to be deleted and if it was going to be a generated,  then mark it as an update
							if (FileActions[fileName] == TemplateFileAction.Delete)
								FileActions[fileName] = TemplateFileAction.Update;
							else
								FileActions[fileName] = TemplateFileAction.None;
						}
						else
						{
							FileActions.Add(fileName, TemplateFileAction.Add);
						}
					}

					CurrentTask = string.Format("Performing file actions");
					var Deletes = 0; var Updates = 0; var Adds = 0;
					/* Process File Actions based on which Template File action*/
					foreach (string fileName in FileActions.Keys.ToList())
					{
                        CurrentTask = string.Format("Performing file actions on {0} (Action={1})", fileName, FileActions[fileName]);
                        if ((FileActions[fileName] == TemplateFileAction.Delete) ||
							(FileActions[fileName] == TemplateFileAction.Unknown))
						{
							if (File.Exists(fileName))
							{
								File.Delete(fileName);
								Deletes++;
							}
						}
						else if (FileActions[fileName] == TemplateFileAction.Update)
						{
							//We do not need to update if the file contents are the same
							if (!FileListAndContents[(FileName)fileName].IsEqualToFileContents(fileName))
							{
								Updates++;
								//if (File.Exists(fileName)) File.Delete(fileName);
								File.WriteAllText(fileName, FileListAndContents[(FileName)fileName]);
							}
						}
						else if (FileActions[fileName] == TemplateFileAction.Add)
						{
							Adds++;
							File.WriteAllText(fileName, FileListAndContents[(FileName)fileName]);
						}
					}
					StatusMessage(string.Format("File Action Counts: Adds={0}, Updates={1}, Deletes={2}", Adds, Updates, Deletes), true);
					if ((Adds > 0) || (Deletes > 0)) returnCode = ReturnCode.OkAddDels;
				}
				else if (!string.IsNullOrEmpty(result))
				{
					if (File.Exists(OutputPath)) File.Delete(OutputPath);
					File.WriteAllText(OutputPath, result);
				}
				else
				{
					throw new ApplicationException("The Template Engine Produced No results for path [" + templateFileName + "]");
				}
				CurrentTask = string.Format("All done!");
				return new ReturnCodes(templateFileName, returnCode);
			}
			catch (Exception ex)
			{
				returnCode = ReturnCode.Error;
				ErrorMessage("Error while " + CurrentTask + "." + ex.Message);
				throw;
			}
		}

		private static void HandlePath(string Path, TemplatePathOption option, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource)
		{
			try
			{

			}
			catch (Exception)
			{
				throw;
			}
		}
		public static bool ShowWarnings { get; set; } = true;
	}
}
