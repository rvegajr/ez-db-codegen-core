using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using EzDbCodeGen.Internal;
using System.Text;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Classes;
using Newtonsoft.Json;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Cli
{
    public static class CommandMain
    {
        static void StatusChangeEventHandler(object sender, StatusChangeEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        internal static Settings Settings { get; set; } = new Settings();
        public static string SampleFilesPath { get; set; }
        public static string AppName { get; set; }

        public static string SchemaName { get; set; }

        public static string TemplateFileNameOrPath { get; set; }
        public static string pfx { get; set; } = "EzDbCodeGen: ";


        static void InteractiveConnectionString()
        {
            var iLoopCount = 0;
            bool questionLoop = true;
            Console.ResetColor();
            var defaultConnectionString = "Server=localhost;Database=YourDatabaseName;Trusted_Connection=True;";
            IConnectionParameters connparm = new EzDbSchema.MsSql.ConnectionParameters();
            connparm.ConnectionString = (string.IsNullOrEmpty(Settings.ConnectionString) ? defaultConnectionString : Settings.ConnectionString);

            var ConnectionStringLocal = "";
            if (!connparm.Database.Equals("YourDatabaseName"))
            {
                var ConnectionStringPromptMessage = $"Use this connection string '{connparm.ConnectionString}' [Y] (Type [N] to build a new one)";
                if (Prompt.GetYesNo(ConnectionStringPromptMessage, true, promptColor: ConsoleColor.Gray))
                {
                    ConnectionStringLocal = Prompt.GetString("What connection string would you like to use (just press enter to build it)?", promptColor: ConsoleColor.Green);
                }
            } else
            {
                ConnectionStringLocal = Prompt.GetString("Enter in the connection string you would like to use (or leave blank to build it)?", defaultValue: "", promptColor: ConsoleColor.Green);
            }

            if ((ConnectionStringLocal ?? "") == "")
            {
                while (questionLoop)
                {
                    iLoopCount++;
                    connparm.Server = Prompt.GetString("What is the database server?", defaultValue: connparm.Server, promptColor: ConsoleColor.Green);
                    connparm.Database = Prompt.GetString("What is the database table?", defaultValue: connparm.Database, promptColor: ConsoleColor.Green);
                    connparm.UserName = Prompt.GetString("What is the username to access the database?", defaultValue: (connparm.Trusted ?  "TRUSTED" : connparm.UserName), promptColor: ConsoleColor.Green);
                    connparm.Trusted = connparm.UserName.Equals("TRUSTED");
                    var password = "";
                    if (!connparm.Trusted)
                    {
                        password = Prompt.GetString("What is the password to access the database?", defaultValue: connparm.Password, promptColor: ConsoleColor.Green);
                        connparm.Password = password;
                    } 
                    if (Prompt.GetYesNo("Does this connection string look right: " + connparm.ConnectionString, true, promptColor: ConsoleColor.Green))
                    {
                        Settings.ConnectionString = connparm.ConnectionString;
                        AppSettings.Instance.ConnectionString = Settings.ConnectionString;
                        try
                        {
                            Console.WriteLine("Testing connection to the database");
                            if (connparm.IsValid())
                            {
                                Settings.ConnectionString = connparm.ConnectionString;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Connection OK!");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Connection Failed :( {ex.Message}");
                            if (!Prompt.GetYesNo("Would you like to try build the connection string again?", true, promptColor: ConsoleColor.Red))
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            }
                        }
                    }
                }
            } else
            {
                Settings.ConnectionString = connparm.ConnectionString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateFileOrPath"></param>
        /// <returns>If the tmeplate was changed,  then this function will rrturn true</returns>
        static bool InteractiveTemplatePath(string templateFileOrPath)
        {
            List<string> templateFileList = new List<string>();
            if (File.Exists(templateFileOrPath) && Path.GetExtension(templateFileOrPath).ToLower().Equals("hbs"))
            {
                templateFileList.Add(templateFileOrPath);
            } else if (Directory.Exists(templateFileOrPath))
            {
                templateFileList = Directory.GetFiles(templateFileOrPath, "*.hbs").ToList();
            }
            if (templateFileList.Count==0)
            {
                if (Prompt.GetYesNo($"No templates founds in '{templateFileOrPath}', shall we initilize this directory with sample templates from the git repo?", true, promptColor: ConsoleColor.Cyan))
                {
                    return SampleFileDownload(SampleFilesPath, AppName);
                }
            }
            return false;
        }

        public static bool SampleFileDownload(string sampleFilesPath, string appName)
        {
            Console.WriteLine($"{pfx}: Downloading sample files for '{appName}' into '{sampleFilesPath}'...");
            //sampleFilesPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, sampleFilesPath));
            if (!sampleFilesPath.EndsWith(Path.DirectorySeparatorChar)) sampleFilesPath += Path.DirectorySeparatorChar;

            var workPath = Path.GetTempPath() + @"EzDbCodeGen\";
            var InTestMode = false; //Uncomment so the work path will be deleted so we can test downlading the sample path
            if (!InTestMode)
            {
                if (Directory.Exists(workPath)) Directory.Delete(workPath, true);
                Console.WriteLine(pfx + "Sample Files to be downloaded from https://github.com/rvegajr/ez-db-codegen-core to " + workPath);
                WebFileHelper.CurlGitRepoZip(workPath);
            }
            if (Directory.Exists(workPath))
            {
                var rootPath = workPath + @"ez-db-codegen-core-master\Src\EzDbCodeGen.Cli\";
                WebFileHelper.CopyTo(@"Templates\SchemaRender.hbs", rootPath, sampleFilesPath);
                WebFileHelper.CopyTo(@"Templates\SchemaRenderAsFiles.hbs", rootPath, sampleFilesPath);
                WebFileHelper.CopyTo(@"Templates\SchemaRenderAsFilesNoOutput.hbs", rootPath, sampleFilesPath);
                WebFileHelper.CopyTo(@"ezdbcodegen.config.json", rootPath, sampleFilesPath);
                WebFileHelper.CopyTo(@"ezdbcodegen.ps1", rootPath, sampleFilesPath);

                WebFileHelper.CopyTo(@"ezdbcodegen.config.json", rootPath, sampleFilesPath, appName + ".config.json")
                    .ReplaceAll("MyEntities", appName + "Entities");
                WebFileHelper.CopyTo(@"ezdbcodegen.ps1", rootPath, sampleFilesPath, appName + ".codegen.ps1")
                    .ReplaceAll("ezdbcodegen", appName)
                    .ReplaceAll("Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa", AppSettings.Instance.ConnectionString)
                    ; 
                WebFileHelper.CopyTo(@"readme.txt", rootPath, sampleFilesPath)
                    .ReplaceAll("SuperApp", appName)
                    .ReplaceAll("%PSPATH%", sampleFilesPath)
                ;
                Console.WriteLine($" ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Code Generator is ready to run, open powershell and execute:");
                Console.WriteLine($" ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"./{Path.GetFullPath(Path.Combine(sampleFilesPath, appName + ".codegen.ps1"))}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" ");
                //changed the Setings Template path to the samples templates
                Settings.TemplateFileNameOrPath = Path.Combine(sampleFilesPath, "Templates").PathEnds();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sampel call is
        ///    return Exit("Message here");
        /// </summary>
        /// <param name="ExitMessage"></param>
        /// <param name="ReturnCode"></param>
        /// <returns></returns>
        public static int Exit(string ExitMessage = "Application Completed Seccuessfully", int ReturnCode = 0)
        {
            if (ReturnCode == 0) Console.ForegroundColor = ConsoleColor.Green;
            if ((ReturnCode > 0) && (ReturnCode < 10)) Console.ForegroundColor = ConsoleColor.Yellow;
            if (ReturnCode > 10) Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(ExitMessage);
            Environment.ExitCode = ReturnCode;
            Environment.Exit(Environment.ExitCode);
            return Environment.ExitCode;
        }
        
        public static void Enable(CommandLineApplication app) {
            app.Name = "ezdb.codegen.cli";
            app.Description = "EzDbCodeGen - Code Generation Utility";
            app.ExtendedHelpText = @"This application will allow you to trigger code generation based on a template file or a list of template files.

To use this application with sample templates (assuming you installed the application locally), navigate to the path you wish to install the templates :
1. dotnet ezdbcg -i ""."" -a ""MyAppName""
2. dotnet ezdbcg

Notes: step 1 will download the sample templates to this path, step 2 will start an interactive session and save the last values you entered for when executed in this path again

";

            app.HelpOption("-?|-h|--help");

            var sampleFilesOption = app.Option("-i|--init-files <path>",
                "This option will download the template files and required powerscript to the [path] directory (send '.' for current path), renaming assets using the value sent through -a/--app-name ",
                CommandOptionType.SingleValue);

            var appNameOption = app.Option("-a|--app-name <appame>",
                "This option will be used to customize the name and files on --init-files,  this default value will be MyApp",
                CommandOptionType.SingleValue);

            var verboseOption = app.Option("-v|--verbose",
                "Will output more detailed message about what is happening during application processing.  This parm is optional and will override the value in appsettings.json.   ",
                CommandOptionType.NoValue);

            var versionOption = app.Option("-ver|--version",
                "Will output the current version of the utility.",
                CommandOptionType.NoValue);

            var templateFileNameOrDirectoryOption = app.Option("-t|--template <value>",
                "The template file name or path that you wish to render.  If you choose aa path,  ",
                CommandOptionType.SingleValue);

            var pathNameOption = app.Option("-p|--outpath <value>",
                "The template that you wish to render.  This is required uniless you use the <OUTPUT_PATH> specifier in the template file.",
                CommandOptionType.SingleValue);

            var sourceConnectionStringOption = app.Option("-sc|--connection-string <optionvalue>",
                "Connection String pass via the appline.  This parm is optional and this value will override the value in appsettings.json. ",
                CommandOptionType.SingleValue);

            var sourceSchemaFileNameOption = app.Option("-sf|--schema-file <optionvalue>",
                "Specify a schema json dump to perform the code generation (as opposed to a connection string).  This parm is optional and parm is present, it will override the appsettings and the -sc app line parm",
            CommandOptionType.SingleValue);

            var configFileOption = app.Option("-cf|--configfile",
                "The configuration file this template render will use.  This is optional, the default search path will be in the same path as this assembly of this applicaiton. ",
                CommandOptionType.SingleValue);

            var compareToConnectionStringOption = app.Option("-tc|--compare-connection-string <optionvalue>",
                "Connection String to compare to.  This parm is optional.  If it is present,  This schema will be compared to either the -sc or -sf and the only the changes will be updated.",
                CommandOptionType.SingleValue);

            var compareToSchemaFileNameOption = app.Option("-tf|--compare-schema-file <optionvalue>",
            "OPTIONAL: Specify a compare schema json dump to perform the (as opposed to a compare connection string).  This parm is optional and parm is present, it will override the -ts command line parameter and will be used to compare,  affecting only those entities that have changed.",
            CommandOptionType.SingleValue);

            var schemaNameOption = app.Option("-sn|--schemaName",
                "the Name of the schema,  a decent standard could be <DatabaseName>Entites.",
                CommandOptionType.SingleValue);

            var projectFileToModifyOption = app.Option("-pf|--project-file <optionvalue>",
                "Option to pass a project file that will be altered with the ouputpath that the template files will be written to.  This only pertains to older version of a visual studio project file.",
                CommandOptionType.SingleValue);

            var templateFilterFileMasks = app.Option("-f|--filter <optionvalue>",
                "Option to ignore template file names (seperated by comma, wildcards are acceptable) for those runs where a path is sent through parm -t (or --template).",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var version_ = Assembly.GetAssembly(typeof(CodeGenerator)).GetName().Version;
                if (versionOption.HasValue())
                {
                    return Exit(version_.ToString());
                }

                if (verboseOption.HasValue()) AppSettings.Instance.VerboseMessages = verboseOption.HasValue();
                try
                {
                    AppName = "MyApp";
                    SchemaName = "MySchema";
                    var PathConfigFileName = Path.Combine(Environment.CurrentDirectory, "").PathEnds() + ".ezdbcodegen.config";
                    if (File.Exists(PathConfigFileName))
                    {
                        Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PathConfigFileName));
                    }
                    if (Settings.AutoRun)
                    {
                        Console.WriteLine("A previous run in this path asked to reuse the latest settings,  reading from '.ezdbcodegen.config' and using these settings for this run");
                        AppSettings.Instance.ConnectionString = Settings.ConnectionString;
                        TemplateFileNameOrPath = Settings.TemplateFileNameOrPath;
                        AppName = Settings.AppName;
                        SchemaName = Settings.SchemaName;
                    }
                    Console.WriteLine($"EzDbCodeGen Tool Version {version_.ToString()}");
                    SampleFilesPath = (sampleFilesOption.HasValue() 
                        ? sampleFilesOption.Value().ResolvePathVars() 
                        : Path.Combine(Environment.CurrentDirectory, "Sample").PathEnds()
                       );
                    if (appNameOption.HasValue()) AppName=appNameOption.Value();
                    if (sampleFilesOption.HasValue()) SampleFileDownload(SampleFilesPath, AppName);

                    if (schemaNameOption.HasValue()) SchemaName = schemaNameOption.Value();
                    if (sourceConnectionStringOption.HasValue())
                    {
                        AppSettings.Instance.ConnectionString = sourceConnectionStringOption.Value().SettingResolution();
                    }

                    //Overriding withe the config file option will always superscede all configuration settings
                    if (configFileOption.HasValue())
                    {
                        AppSettings.Instance.ConfigurationFileName = configFileOption.Value();
                    } else
                    {
                        var configFileFound = Path.Combine(Environment.CurrentDirectory, "ezdbcodegen.config.json");
                        string[] ConfigurationFileNameSearchList = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "").PathEnds(), "*.config.json");
                        if (ConfigurationFileNameSearchList.Contains(configFileFound, StringComparer.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine($"Found Config file at {configFileFound}");
                            AppSettings.Instance.ConfigurationFileName = configFileFound;
                        }
                        else if (ConfigurationFileNameSearchList.Count() > 0)
                        {
                            configFileFound = ConfigurationFileNameSearchList.FirstOrDefault();
                            Console.WriteLine($"Found Config file at {configFileFound}");
                            AppSettings.Instance.ConfigurationFileName = configFileFound;
                        }
                    }


                    var Errors = new StringBuilder();
                    var OutputPath = string.Empty;
                    if ((pathNameOption.HasValue()) && (pathNameOption.Value().Length > 0)) OutputPath = pathNameOption.Value();
                    if (OutputPath == string.Empty) OutputPath = Environment.CurrentDirectory.PathEnds();
                    if ((!sourceSchemaFileNameOption.HasValue()) && (AppSettings.Instance.ConnectionString.Length == 0))
                    {
                        InteractiveConnectionString();
                    }
                    if ((!sourceSchemaFileNameOption.HasValue()) && (AppSettings.Instance.ConnectionString.Length == 0))
                        Errors.AppendLine("ConnectionString and schemaFileName are both missing or empty. ");
                    if ((sourceSchemaFileNameOption.HasValue()) && (!File.Exists(sourceSchemaFileNameOption.Value())))
                        Errors.AppendLine($"Schema file '{sourceSchemaFileNameOption.Value()}' was does not exists! ");

                    if ((templateFileNameOrDirectoryOption.HasValue()) && (templateFileNameOrDirectoryOption.Value().Length > 0))
                        TemplateFileNameOrPath = templateFileNameOrDirectoryOption.Value();
                    //If we didn't get the template name through the command ling
                    if ((TemplateFileNameOrPath == null) || (Path.GetPathRoot(TemplateFileNameOrPath).Length==0))
                    {
                        //TemplateFileNameOrPath = ("{ASSEMBLY_PATH}" + TemplateFileNameOrPath).ResolvePathVars();
                        //Default to path where dot net tool us executed 
                        TemplateFileNameOrPath = Path.Combine(SampleFilesPath, "Templates").PathEnds();
                    }
                    if (!(
                        (File.Exists(TemplateFileNameOrPath)) ||
                        (Directory.Exists(TemplateFileNameOrPath)
                       ))) TemplateFileNameOrPath = Environment.CurrentDirectory.PathEnds();

                    if (InteractiveTemplatePath(TemplateFileNameOrPath)) TemplateFileNameOrPath = Settings.TemplateFileNameOrPath;

                    if (!Settings.AutoRun)
                    {
                        Settings.AutoRun = Prompt.GetYesNo("Would you like to use this configuration everytime you run in this path?", true);
                    }
                    File.WriteAllText(PathConfigFileName, JsonConvert.SerializeObject(Settings));


                    if (!((File.Exists(TemplateFileNameOrPath)) || (Directory.Exists(TemplateFileNameOrPath))))
                    {
                        //If we find not tmeplate files or path,  then lets see if we should show the help dump
                        if (Environment.GetCommandLineArgs().Count() == 1)
                        {
                            app.ExtendedHelpText = app.ExtendedHelpText
                                + Environment.NewLine + "BaseDirectory=" + AppContext.BaseDirectory
                                + Environment.NewLine + "CurrentDirectory=" + Environment.CurrentDirectory
                                + Environment.NewLine + "GetCurrentDirectory=" + System.IO.Directory.GetCurrentDirectory()
                                + Environment.NewLine + "GetCommandLineArgs=" + System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
                            ;
                            app.ShowHelp();
                            return Exit("");
                        } 
                        else
                        {
                            Errors.AppendLine($"Template not found in path {TemplateFileNameOrPath}");
                        }
                    }

                    if (Errors.Length > 0)
                    {
                        if (sampleFilesOption.HasValue())
                        {
                            return Exit("Sample files where generated... exiting");
                        }
                        throw new Exception(Errors.ToString());
                    }

                    Console.ResetColor();
                    // get the file attributes for file or directory
                    FileAttributes attr = File.GetAttributes(TemplateFileNameOrPath);

                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        pfx = "<List of templates>: ";
                        //Since we know this is a directory,  force it to end with a system path seperator
                        TemplateFileNameOrPath = TemplateFileNameOrPath.PathEnds();
                    }
                    else
                    {
                        pfx = Path.GetFileNameWithoutExtension(TemplateFileNameOrPath) + ": ";
                    }

                    Console.WriteLine("Performing Rendering of template " + pfx + "....");
                    if (AppSettings.Instance.VerboseMessages)
                    {
                        Console.WriteLine(pfx + "Template Path: " + TemplateFileNameOrPath);
                        Console.WriteLine(pfx + "Path Name: " + pathNameOption.Value());
                        Console.WriteLine(pfx + "Config File Name: " + AppSettings.Instance.ConfigurationFileName);
                        if (!sourceSchemaFileNameOption.HasValue()) Console.WriteLine(pfx + "Source Connection String: " + AppSettings.Instance.ConnectionString);
                        if (sourceSchemaFileNameOption.HasValue()) Console.WriteLine(pfx + "Source Schema File Name: " + sourceSchemaFileNameOption.Value());

                        if (compareToSchemaFileNameOption.HasValue()) Console.WriteLine(pfx + "Compare To Schema File Name: " + compareToSchemaFileNameOption.Value());
                        if ((!compareToSchemaFileNameOption.HasValue()) && (compareToConnectionStringOption.HasValue())) Console.WriteLine(pfx + "Compare To Connection String: " + compareToConnectionStringOption.Value());
                    }
                    var CodeGen = new CodeGenerator
                    {
                        SchemaName = Settings.SchemaName,
                        VerboseMessages = AppSettings.Instance.VerboseMessages,
                        ConfigurationFileName = AppSettings.Instance.ConfigurationFileName,
                        ProjectPath = ((projectFileToModifyOption.HasValue()) ? projectFileToModifyOption.Value() : ""),
                        TemplateFileNameFilter = ((templateFilterFileMasks.HasValue()) ? templateFilterFileMasks.Value() : "")
                    };

                    var version = Assembly.GetAssembly(typeof(CodeGenerator)).GetName().Version;
                    Console.WriteLine(pfx + "Using CodeGenerator Version " + version);

                    CodeGen.OnStatusChangeEventArgs += StatusChangeEventHandler;

                    var returnCode = new ReturnCodes();
                    ITemplateDataInput Source = null;
                    if (sourceSchemaFileNameOption.HasValue())
                        Source = new TemplateInputFileSource(sourceSchemaFileNameOption.Value());
                    else
                        Source = new TemplateInputDatabaseConnecton(AppSettings.Instance.ConnectionString);

                    ITemplateDataInput CompareTo = null;
                    if (compareToSchemaFileNameOption.HasValue())
                        CompareTo = new TemplateInputFileSource(compareToSchemaFileNameOption.Value());
                    else if (compareToConnectionStringOption.HasValue())
                        CompareTo = new TemplateInputDatabaseConnecton(compareToConnectionStringOption.Value());

                    if (CompareTo == null)
                    {
                        Source.VerboseMessages = AppSettings.Instance.VerboseMessages;
                        returnCode = CodeGen.ProcessTemplate(TemplateFileNameOrPath, Source, OutputPath);
                    }
                    else
                    {
                        Source.VerboseMessages = AppSettings.Instance.VerboseMessages;
                        CompareTo.VerboseMessages = AppSettings.Instance.VerboseMessages;
                        returnCode = CodeGen.ProcessTemplate(TemplateFileNameOrPath, Source, CompareTo, OutputPath);
                    }

                    return Exit("Render of template " + templateFileNameOrDirectoryOption.Value() + " Completed!");
                }
                catch (Exception ex)
                {
                     Console.WriteLine("Could not render template. " + ex.Message);
                    Console.WriteLine("Stack Trace:");
                    return Exit(ex.StackTrace, 100);
                }

            });

        }
    }
}