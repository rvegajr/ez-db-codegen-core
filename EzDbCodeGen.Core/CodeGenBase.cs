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

namespace EzDbCodeGen.Core
{
	public abstract class CodeGenBase
	{
		public static string OP_FILE = "<FILE>";
		public static string OP_ENTITY_KEY = "<ENTITY_KEY>";
		public static string OP_FILE_END = "</FILE>";
		public static string OP_ENTITY_KEY_END = "</ENTITY_KEY>";

		public virtual string TemplatePath { get; set; } = "";
		public virtual string OutputPath { get; set; } = "";
		public virtual string ConnectionString { get; set; } = "";
		public TemplatePathOption TemplatePathOption { get; set; } = TemplatePathOption.Auto;
		public virtual bool VerboseMessages { get; set; } = true;
		public virtual string SchemaName { get; set; } = "MyEzSchema";
		public IDatabase Schema { get; set; }
		public static Config.CodeGenConfiguration RzDbConfig = new Config.CodeGenConfiguration();
		public string[] AllowedKeys(IDatabase model)
		{
			return model.Keys.Where(k => !k.EndsWith("_Archive", StringComparison.OrdinalIgnoreCase)).ToArray();
		}

		public CodeGenBase(string connectionString, string templatePath, string outputPath)
		{
			this.TemplatePath = templatePath;
			this.OutputPath = outputPath;
			this.ConnectionString = connectionString;
		}

		public ReturnCode ProcessTemplate()
		{
			try
			{
				this.Schema = new EzDbSchema.MsSql.Database().Render(SchemaName, ConnectionString);
				return ProcessTemplate(new TemplateInputDirectObject(this.Schema), null);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Failed on ProcessTemplate. {0}", ex.Message), ex);
			}
		}

		public ReturnCode ProcessTemplate(ITemplateInput templateInput)
		{
			try
			{
				return ProcessTemplate(templateInput, null);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Failed on ProcessTemplate. {0}", ex.Message), ex);
			}
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
		/// Processes the template using passed Template Inputs.  These inputs can be from a variety of sources including direct schema (useful for caching scenarios), filename and connection strings.
		/// </summary>
		/// <returns><c>true</c>, if template was processed, <c>false</c> otherwise.</returns>
		/// <param name="originalTemplateInputSource">Original template input source.  Pass the input to here if you want to generate using only 1 schema</param>
		/// <param name="compareToTemplateInputSource">Optional - Compare to template input source.  This will process only the differences. </param>
		public ReturnCode ProcessTemplate(ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource)
		{
			var CurrentTask = "Entering ProcessTemplate";
			var returnCode = ReturnCode.OkNoAddDels;
			try
			{
				CurrentTask = "Peforming Validations";
				if (originalTemplateInputSource == null) throw new Exception(@"There must be an Template Source passed through originalTemplateInputSource!");
				CurrentTask = "Loading Source Schema";
				IDatabase schema = originalTemplateInputSource.LoadSchema();
				if (schema == null) throw new Exception(@"originalTemplateInputSource is not a valid template");

				CurrentTask = "Reading Assembly Name";
				string assemblyBasePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "").Replace("file:", "") + Path.DirectorySeparatorChar.ToString();
				string FullTemplatePath = assemblyBasePath + TemplatePath;
				CurrentTask = "Template path is " + FullTemplatePath;
				string outputDirectory = Path.GetDirectoryName(OutputPath) + Path.DirectorySeparatorChar;
				if (!File.Exists(FullTemplatePath)) throw new FileNotFoundException("Template File " + FullTemplatePath + " is not found");
				CurrentTask = string.Format("Checking to see if outpath of {0} exists?", outputDirectory);

				if (!Directory.Exists(outputDirectory))
				{
					CurrentTask = string.Format("It doesn't... sp lets try to create it");
					Directory.CreateDirectory(outputDirectory);
				}

				string result = "";
				try
				{
                    
					CurrentTask = string.Format("Trying to find Config file");
					string RzDbConfigPath = AppSettings.Instance.ConfigurationFileName;
					if (File.Exists(RzDbConfigPath))
					{
						CurrentTask = string.Format("Config file found! lets read it");
						RzDbConfig = Config.CodeGenConfiguration.FromFile(RzDbConfigPath);
						foreach (var item in RzDbConfig.PluralizerCrossReference)
							Pluralizer.Instance.AddWord(item.SingleWord, item.PluralWord);
					}

					CurrentTask = string.Format("Reading Template from '{0}'", FullTemplatePath);
					var templateAsString = File.ReadAllText(FullTemplatePath);
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
					.Replace("$OUTPUT_PATH$", outputDirectory).TrimStart();

				CurrentTask = string.Format("Template Preocess Compeleted");
				/* If the entity key specifier doesn't exist */
				var hasEntityKeySpecifier = result.Contains(CodeGenBase.OP_ENTITY_KEY);
				CurrentTask = string.Format("Does the file contain FILE operator?");
				if (result.Contains(CodeGenBase.OP_FILE)) /* File seperation specifier - this will split by the files specified by  */
				{
					CurrentTask = string.Format("Parsing files");
					/* First, lets get all the files currently in the path */
					var FileActions = new Dictionary<string, TemplateFileAction>();
					string[] FilesinOutputDirectory = Directory.GetFiles(outputDirectory);
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
						if ((newOutputFileName.Length > 0) && (newOutputFileName.StartsWith(outputDirectory, StringComparison.Ordinal)))
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
					throw new ApplicationException("The Template Engine Produced No results for path [" + FullTemplatePath + "]");
				}
				CurrentTask = string.Format("All done!");
				return returnCode;
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
