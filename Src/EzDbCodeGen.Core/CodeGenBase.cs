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
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Classes;

namespace EzDbCodeGen.Core
{
    public abstract class CodeGenBase
    {
        public static string OP_FILE = "<FILE>";
        public static string OP_ENTITY_KEY = "<ENTITY_KEY>";
        public static string OP_OUTPUT_PATH = "<OUTPUT_PATH>";
        public static string OP_PROJECT_PATH = "<PROJECT_PATH>";
        public static string OP_FILE_END = "</FILE>";
        public static string OP_ENTITY_KEY_END = "</ENTITY_KEY>";
        public static string OP_OUTPUT_PATH_END = "</OUTPUT_PATH>";
        public static string OP_PROJECT_PATH_END = "</PROJECT_PATH>";
        public static string VAR_THIS_PATH = "$THIS_PATH$";
        public static string VAR_TEMP_PATH = "$TEMP$";

        public virtual string TemplatePath { get; set; } = "";
        public virtual string OutputPath { get; set; } = "";
        public virtual string ProjectPath { get; set; } = "";
        public virtual string ConnectionString { get; set; } = "";
        public virtual string ConfigurationFileName { get; set; } = "";
        public virtual string TemplateFileNameFilter { get; set; } = "";  //"FileName*,SampleFile*"

        public TemplatePathOption TemplatePathOption { get; set; } = TemplatePathOption.Auto;
        public virtual bool VerboseMessages { get; set; } = true;
        public virtual string SchemaName { get; set; } = "MyEzSchema";
        public IDatabase Schema { get; set; }
        public static Config.Configuration RzDbConfig = Internal.AppSettings.Instance.Configuration;
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
            message = (string.IsNullOrEmpty(_currentTemplateName) ? "" : (_currentTemplateName + ": ")) + message;

            if ((this.VerboseMessages) && (OnStatusChangeEventArgs != null))
                OnStatusChangeEventArgs(this, new StatusChangeEventArgs(message));
            else
                Console.WriteLine(message);
        }

        private void StatusMessage(string message)
        {
            StatusMessage(message, false);
        }

        private void ErrorMessage(string errorMessage)
        {
            errorMessage = (string.IsNullOrEmpty(_currentTemplateName) ? "ERROR: " : (_currentTemplateName + " ERROR: ")) + errorMessage;

            if ((this.VerboseMessages) && (OnStatusChangeEventArgs != null))
                OnStatusChangeEventArgs(this, new StatusChangeEventArgs(errorMessage));
            else
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
        /// <param name="originalTemplateInputSource">The template input class,  could be an object of type IDatabase or if type schema</param>
        /// <param name="compareToTemplateInputSource">The template input class to compare to,  will only change the difference</param>
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

        /// <summary>
        /// Processes the template using passed Template Inputs and the handlebars template name.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings. There needs to be a  &lt;FILE&gt;FILENAMEHERE&lt;/FILE&gt; specifier, then this should be a file name,  
        /// </summary>
        /// <returns><c>true</c>, if template was processed, <c>false</c> otherwise.</returns>
        /// <param name="TemplateFileNameOrPath">The file name of a handlebars template or a path that contains handlebars templates. If no path is specified,  the app will prepend the assembly path in front of the text and search there</param>
        /// <param name="originalTemplateInputSource">The template input class,  could be an object of type IDatabase or if type schema</param>
        /// <param name="compareToTemplateInputSource">The template input class to compare to,  will only change the difference</param>
        /// <returns>A return code </returns>
        public ReturnCodes ProcessTemplate(string TemplateFileNameOrPath, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource)
        {
            return ProcessTemplate(TemplateFileNameOrPath, originalTemplateInputSource, compareToTemplateInputSource, "");
        }

        /// <summary>
        /// Processes the template using passed Template Inputs and the handlebars template name.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings. There needs to be a  &lt;FILE&gt;FILENAMEHERE&lt;/FILE&gt; specifier, then this should be a file name,  
        /// </summary>
        /// <returns><c>true</c>, if template was processed, <c>false</c> otherwise.</returns>
        /// <param name="TemplateFileNameOrPath">The file name of a handlebars template or a path that contains handlebars templates. If no path is specified,  the app will prepend the assembly path in front of the text and search there</param>
        /// <param name="originalTemplateInputSource">The template input class,  could be an object of type IDatabase or if type schema</param>
        /// <param name="compareToTemplateInputSource">The template input class to compare to,  will only change the difference</param>
        /// <returns>A return code </returns>
        public ReturnCodes ProcessTemplate(string TemplateFileNameOrPath, ITemplateInput originalTemplateInputSource)
        {
            return ProcessTemplate(TemplateFileNameOrPath, originalTemplateInputSource, null, "");
        }

        protected ReturnCodes ProcessTemplate(DirectoryName pathName, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource, string outputPath)
        {
            var filesEndingInHbs = Directory.EnumerateFiles(pathName).Where(f => f.EndsWith("hbs", StringComparison.InvariantCulture)).ToList();
            var returnCodeList = new ReturnCodes();
            foreach (var _templateFullFileName in filesEndingInHbs)
            {
                string templateFullFileName = (FileName)_templateFullFileName;
                if ((this.TemplateFileNameFilter.Length>0) && (Path.GetFileNameWithoutExtension(templateFullFileName).IsIn(this.TemplateFileNameFilter)))
                {
                    CurrentTask = string.Format("Template {0} will be ignored because it matches a pattern in TemplateFileNameFilter", Path.GetFileNameWithoutExtension(templateFullFileName));
                } else
                {
                    returnCodeList.Merge(ProcessTemplate((FileName)_templateFullFileName, originalTemplateInputSource, compareToTemplateInputSource, outputPath));
                }
            }
            return returnCodeList;
        }
        public event EventHandler<StatusChangeEventArgs> OnStatusChangeEventArgs;
        private string _currentTemplateName = "";
        private string _currentTask = "";
        public string CurrentTask
        {
            get
            {
                return _currentTask;
            }
            set
            {
                _currentTask = (string.IsNullOrEmpty(_currentTemplateName) ? "" : (_currentTemplateName + ": ")) + value;
                if ((this.VerboseMessages) && (OnStatusChangeEventArgs != null)) OnStatusChangeEventArgs(this, new StatusChangeEventArgs(_currentTask));
            }
        }
        /// <summary>
        /// Contains the list of files that were affected by the last template execution
        /// </summary>
        /// <value>
        /// The file actions indexed by file name and a File Action
        /// </value>
        public Dictionary<string, TemplateFileAction> FileActions { get; } = new Dictionary<string, TemplateFileAction>();

        /// <summary>
        /// Processes the template using passed Template Inputs and the handlebars template name.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings.
        /// </summary>
        /// <returns><c>true</c>, if template was processed, <c>false</c> otherwise.</returns>
        /// <param name="templateFileName">File name of an existing handlebars template file name</param>
        /// <param name="originalTemplateInputSource">Original template input source.  Pass the input to here if you want to generate using only 1 schema</param>
        /// <param name="compareToTemplateInputSource">Optional - Compare to template input source.  This will process only the differences. </param>
        /// <param name="outputPath">The output path.  If there is no &lt;FILE&gt;FILENAMEHERE&lt;/FILE&gt; specifier, then this should be a file name,  if there is a file specifier,  then it will write to the file resolved between the FILE tags</param>
        protected ReturnCodes ProcessTemplate(FileName _templateFileName, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource, string outputPath)
		{
            FileActions.Clear();
            Configuration EzDbConfig = null;
            this.OutputPath = outputPath;
            string templateFileName = _templateFileName;
            _currentTemplateName = Path.GetFileNameWithoutExtension(templateFileName);

            if (!File.Exists(templateFileName)) templateFileName = ("{ASSEMBLY_PATH}" + templateFileName).ResolvePathVars();
            CurrentTask = "Entering ProcessTemplate";
			var returnCode = ReturnCode.OkNoAddDels;
			try
			{
                CurrentTask = string.Format("Trying to find Config file at {0}", ConfigurationFileName);
                if (File.Exists(ConfigurationFileName))
                {
                    CurrentTask = string.Format("Config file found! lets read it");
                    EzDbConfig = Configuration.FromFile(ConfigurationFileName);
                    foreach (var item in EzDbConfig.PluralizerCrossReference)
                        Pluralizer.Instance.AddWord(item.SingleWord, item.PluralWord);
                    if (!string.IsNullOrEmpty(EzDbConfig.Database.SchemaName))
                    {
                        CurrentTask = string.Format("Schema name has been changed from {0} to {1} by configuration file.", this.SchemaName, EzDbConfig.Database.SchemaName);
                        this.SchemaName = EzDbConfig.Database.SchemaName;
                    }
                }
                else
                {
                    ErrorMessage(string.Format("WARNING!  Configuration file was not found at {0}", ConfigurationFileName));
                }

                CurrentTask = "Performing Validations";
				if (originalTemplateInputSource == null) throw new Exception(@"There must be an Template Source passed through originalTemplateInputSource!");
				CurrentTask = "Loading Source Schema";
				IDatabase schema = originalTemplateInputSource.LoadSchema(EzDbConfig);
                schema.Name = this.SchemaName;

                if (schema == null) throw new Exception(@"originalTemplateInputSource is not a valid template");

				CurrentTask = "Template path is " + templateFileName;

				string result = "";
                try
                {

                    CurrentTask = string.Format("Reading Template from '{0}'", templateFileName);
                    var templateAsString = File.ReadAllText(templateFileName);
                    var forceDeleteReloadOfDirectory = false;
                    //Does template have a project path override? If so, extract it and stash it
                    if (templateAsString.Contains(CodeGenBase.OP_PROJECT_PATH))
                    {
                        this.ProjectPath = templateAsString.Pluck(CodeGenBase.OP_PROJECT_PATH, CodeGenBase.OP_PROJECT_PATH_END, out templateAsString).Trim();
                        if (this.ProjectPath.StartsWith("@"))  //An @ at the beginning forces the app to treat this as a path and delete and attempt to recreate it 
                        {
                            this.ProjectPath = this.ProjectPath.Substring(1);
                            CurrentTask = string.Format("'@' was found at the beginning of ProjectPath but doesn't do anything here and will be ignored");
                        }
                        if (this.ProjectPath.Contains(CodeGenBase.VAR_THIS_PATH)) this.ProjectPath = Path.GetFullPath(this.ProjectPath.Replace(CodeGenBase.VAR_THIS_PATH, Path.GetDirectoryName(templateFileName).PathEnds()));
                        if (this.ProjectPath.Contains(CodeGenBase.VAR_TEMP_PATH)) this.ProjectPath = Path.GetFullPath(this.ProjectPath.Replace(CodeGenBase.VAR_TEMP_PATH, Path.GetDirectoryName(templateFileName).PathEnds()));
                        CurrentTask = string.Format("Project Path modifier found in template, resolved to: {0}", this.ProjectPath);
                        templateAsString = templateAsString.Replace(CodeGenBase.OP_PROJECT_PATH, "").Replace(CodeGenBase.OP_PROJECT_PATH_END, "").Trim();
                    }

                    //Does template have and Output path override? if so, override the local outputDirectory and strip it 
                    if (templateAsString.Contains(CodeGenBase.OP_OUTPUT_PATH))
                    {
                        this.OutputPath = templateAsString.Pluck(CodeGenBase.OP_OUTPUT_PATH, CodeGenBase.OP_OUTPUT_PATH_END, out templateAsString).Trim();

                        if (this.OutputPath.StartsWith("@"))  //An @ at the beginning forces the app to treat this as a path and delete and attempt to recreate it 
                        {
                            this.OutputPath = this.OutputPath.Substring(1);
                            CurrentTask = string.Format("'@' was found at the beginning... Will forcefully delete: {0}", this.OutputPath);
                            forceDeleteReloadOfDirectory = true;
                        }
                        if (this.OutputPath.Contains(CodeGenBase.VAR_THIS_PATH)) this.OutputPath = Path.GetFullPath(this.OutputPath.Replace(CodeGenBase.VAR_THIS_PATH, Path.GetDirectoryName(templateFileName).PathEnds()));
                        if (this.OutputPath.Contains(CodeGenBase.VAR_TEMP_PATH)) this.OutputPath = Path.GetFullPath(this.OutputPath.Replace(CodeGenBase.VAR_TEMP_PATH, Path.GetDirectoryName(templateFileName).PathEnds()));
                        CurrentTask = string.Format("Output Path modifier found in template, resolved to: {0}", this.OutputPath);

                        //If we asked for a force of a reload and if we don't contain a file operator, then the output path must be a single file result.
                        //  Forcing this to be created will prevent us from writing this file out, so we will ignore that force directory operator in this case
                        if (forceDeleteReloadOfDirectory)
                        {
                            if (templateAsString.Contains(CodeGenBase.OP_FILE))
                            {
                                if (Directory.Exists(this.OutputPath)) Directory.Delete(this.OutputPath, true);
                                if (!Directory.Exists(this.OutputPath)) Directory.CreateDirectory(this.OutputPath);
                            }
                            else
                            { 
                                var OutputDirectoryContainer = Path.GetDirectoryName(this.OutputPath).PathEnds();
                                if (File.Exists(this.OutputPath)) File.Delete(this.OutputPath);  
                                if (!Directory.Exists(OutputDirectoryContainer)) Directory.CreateDirectory(OutputDirectoryContainer);
                            }
                        } 
                        templateAsString = templateAsString.Replace(CodeGenBase.OP_OUTPUT_PATH, "").Replace(CodeGenBase.OP_OUTPUT_PATH_END, "").Trim();
                    }

                    if (!File.Exists(templateFileName)) throw new FileNotFoundException("Template File " + templateFileName + " is not found");
                    CurrentTask = string.Format("Checking to see if outpath of {0} exists?", this.OutputPath);
                    if (string.IsNullOrEmpty(this.OutputPath))
                    {
                        throw new Exception(string.Format("Output Path was not passed through ProcessTemplate nor did <OUTPUT_PATH /> exist in the hbs template {1}.  It must exist in one or the other.", this.OutputPath, templateFileName));
                    }
                    CurrentTask = string.Format("Registering Handlbar helpers");
					HandlebarsUtility.RegisterHelpers();
					HandlebarsCsUtility.RegisterHelpers();
                    HandlebarsCsV0Utility.RegisterHelpers();
                    HandlebarsTsUtility.RegisterHelpers();
					CurrentTask = string.Format("Compiling Handlbar Template");
					var template = Handlebars.Compile(templateAsString);
					CurrentTask = string.Format("Rendering Handlbar Template");
					result = template(schema);
				}
				catch (Exception exTemplateError)
				{
					returnCode = ReturnCode.Error;
					ErrorMessage(string.Format("{0}: Error while {1}. {2}", Path.GetFileNameWithoutExtension(templateFileName), CurrentTask, exTemplateError.Message));
					throw;
					//throw exRazerEngine;
				}
				finally
				{
				}

                result = result.Replace("<t>", "")
					.Replace("<t/>", "")
					.Replace("<t />", "")
					.Replace("</t>", "")
					.Replace("$OUTPUT_PATH$", this.OutputPath).TrimStart();

				CurrentTask = string.Format("Template Rendering Completed.. handling output now");
				/* If the entity key specifier doesn't exist */
				var hasEntityKeySpecifier = result.Contains(CodeGenBase.OP_ENTITY_KEY);
				CurrentTask = string.Format("Does the file contain FILE operator?");
				if (result.Contains(CodeGenBase.OP_FILE)) /* File seperation specifier - this will split by the files specified by  */
				{
                    if (!Directory.Exists(this.OutputPath))  //This does contain a FILE specifier,  so we need to make this a directoy and try to create it if it doesn't exist
                    {
                        this.OutputPath = Path.GetDirectoryName(this.OutputPath).PathEnds();
                        CurrentTask = string.Format("It doesn't... so lets try to create it");
                        Directory.CreateDirectory(this.OutputPath);
                    }


                    CurrentTask = string.Format("Parsing files");
                    /* First, lets get all the files currently in the path */
                    FileActions.Clear();
					string[] FilesinOutputDirectory = Directory.GetFiles(this.OutputPath);
					foreach (var fileName in FilesinOutputDirectory) FileActions.Add(fileName, TemplateFileAction.Unknown);

					var FileListAndContents = new EntityFileDictionary();
					string[] parseFiles = result.Split(new[] { CodeGenBase.OP_FILE }, StringSplitOptions.RemoveEmptyEntries);

					var EntityKey = "";
					foreach (string fileText in parseFiles)
					{
						var filePart = CodeGenBase.OP_FILE + fileText;  //need to add the delimiter to make pluck work as expected
                        string newOutputFileName = filePart.Pluck(CodeGenBase.OP_FILE, CodeGenBase.OP_FILE_END, out string FileContents);
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
							schemaToCompareTo = compareToTemplateInputSource.LoadSchema(EzDbConfig);
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
                StatusMessage(string.Format("Template was rendered to path {0}", this.OutputPath));

                CurrentTask = string.Format("Checking Project File Modification Option: {0}", (this.ProjectPath.Length>0));

                if (this.ProjectPath.Length > 0)
                {
                    CurrentTask = string.Format("Looks like Project Mod was set to true, Does path exist? ");
                    if (!File.Exists(this.ProjectPath))
                    {
                        StatusMessage(string.Format("ProjectPath was set to {0} but this file doesn't exist,  I will ignore ProjectPath option", this.ProjectPath));

                    }
                    else
                    {
                        StatusMessage(string.Format("ProjectPath was set to {0},  so we will alter this project with the files affected if necessary", this.ProjectPath));
                        var FileActionsOffset = new Dictionary<string, TemplateFileAction>();
                        CurrentTask = string.Format("Figuring out offset of files added compared to Project location");
                        foreach (var fileWithFileAction in FileActions)
                        {
                            var fileOffset = (new Uri(this.ProjectPath))
                                .MakeRelativeUri(new Uri(fileWithFileAction.Key))
                                .ToString()
                                .Replace('/', Path.DirectorySeparatorChar);
                            CurrentTask = string.Format("Project File Check: {0}", fileOffset);
                            FileActionsOffset.Add(fileOffset, fileWithFileAction.Value);
                        }

                        CurrentTask = string.Format("Now we modify the project file {0}", this.ProjectPath);
                        var ret = (new ProjectHelpers()).ModifyClassPath(this.ProjectPath, FileActionsOffset);
                        if (ret)
                            StatusMessage(string.Format("There were changes to {0},  project will probably have to be reloaded", this.ProjectPath));
                        else
                            StatusMessage(string.Format("There were no changes to {0}", this.ProjectPath));
                    }
                }
				CurrentTask = string.Format("All done!");
				return new ReturnCodes(templateFileName, returnCode);
			}
			catch (Exception ex)
			{
				returnCode = ReturnCode.Error;
                ErrorMessage(string.Format("{0}: Error while {1}. {2}", Path.GetFileNameWithoutExtension(templateFileName), CurrentTask, ex.Message));
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
    public class StatusChangeEventArgs : EventArgs
    {
        public StatusChangeEventArgs(string message) { Message = message; }
        public string Message { get; set; }
    }
}
