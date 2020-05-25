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

namespace EzDbCodeGen.Cli
{
    public static class CommandMain
    {
        public static void Enable(CommandLineApplication app) {
            app.Name = "ezdb.codegen.cli";
            app.Description = "EzDbCodeGen - Code Generation Utility";
            app.ExtendedHelpText = "This application will allow you to trigger code generation based on a template file or a list of template files."
                + Environment.NewLine + "";

            app.HelpOption("-?|-h|--help");

            verboseOption = app.Option("-v|--verbose",
                "Will output more detailed message about what is happening during application processing.  This parm is optional and will override the value in appsettings.json.   ",
                CommandOptionType.NoValue);

            templateFileNameOrDirectoryOption = app.Option("-t|--template <value>",
                "The template file name or path that you wish to render.  If you choose aa path,  ",
                CommandOptionType.SingleValue);

            pathNameOption = app.Option("-p|--outputpath <value>",
                "The template that you wish to render.  This is required uniless you use the <OUTPUT_PATH> specifier in the template file.",
                CommandOptionType.SingleValue);

            sourceConnectionStringOption = app.Option("-sc|--connection-string <optionvalue>",
                "Connection String pass via the appline.  This parm is optional and this value will override the value in appsettings.json. ",
                CommandOptionType.SingleValue);

            sourceSchemaFileNameOption = app.Option("-sf|--schema-file <optionvalue>",
                "Specify a schema json dump to perform the code generation (as opposed to a connection string).  This parm is optional and parm is present, it will override the appsettings and the -sc app line parm",
            CommandOptionType.SingleValue);

            configFileOption = app.Option("-cf|--configfile",
                "The configuration file this template render will use.  This is optional, the default search path will be in the same path as this assembly of this applicaiton. ",
                CommandOptionType.SingleValue);

            compareToConnectionStringOption = app.Option("-tc|--compare-connection-string <optionvalue>",
                "Connection String to compare to.  This parm is optional.  If it is present,  This schema will be compared to either the -sc or -sf and the only the changes will be updated.",
                CommandOptionType.SingleValue);

            compareToSchemaFileNameOption = app.Option("-tf|--compare-schema-file <optionvalue>",
            "OPTIONAL: Specify a compare schema json dump to perform the (as opposed to a compare connection string).  This parm is optional and parm is present, it will override the -ts command line parameter and will be used to compare,  affecting only those entities that have changed.",
            CommandOptionType.SingleValue);

            schemaNameOption = app.Option("-sn|--schemaName",
                "the Name of the schema,  a decent standard could be <DatabaseName>Entites.",
                CommandOptionType.SingleValue);

            projectFileToModifyOption = app.Option("-pf|--project-file <optionvalue>",
                "Option to pass a project file that will be altered with the ouputpath that the template files will be written to.  This only pertains to older version of a visual studio project file.",
                CommandOptionType.SingleValue);

            templateFilterFileMasks = app.Option("-f|--filter <optionvalue>",
                "Option to ignore template file names (seperated by comma, wildcards are acceptable) for those runs where a path is sent through parm -t (or --template).",
                CommandOptionType.SingleValue);

            massRenameFromStringOption = app.Option("-rfr|--mass-rename-from <optionvalue>",
                "Calling this will override all other actions and rename all strings in '-p' from the string passed through -rfr this string value -rto.  You can also pass a string that has the replace parms in the following format 'FromSTR1=ToStr1,FromSTR2=ToStr2'.  Be warned that this is a permanent change!",
                CommandOptionType.SingleValue);

            massRenameToStringOption = app.Option("-rto|--mass-rename-to <optionvalue>",
                "Calling this will override all other actions and rename all strings in '-p' from the string passed through -rfr to the string passed through -rto.  Be warned that this is a permanent change!",
                CommandOptionType.SingleValue);
            // -p 'C:\Dev\Noctusoft\ez-api-urf-core\Src' -rfr 'EzApi=CPPE'
            // OLD: -t "<PATH>" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa" -cf "<CONFIG PATH>"
            app.OnExecute(() =>
            {
                var errors = new List<string>();
                if (verboseOption.HasValue()) AppSettings.Instance.VerboseMessages = verboseOption.HasValue();
                try
                {
                    var returnCode = new ReturnCodes();
                    if (massRenameFromStringOption.HasValue())
                    {
                        if (!massRenameFromStringOption.HasValue()) errors.Add(string.Format("Mass Rename Part String From parm of '-rfr' missing and is required. "));
                        if (!massRenameFromStringOption.Value().Contains("=")) {
                            if (!massRenameToStringOption.HasValue()) errors.Add(string.Format("Mass Rename Part String To parm of '-rto' missing and is required. "));
                        }
                        var path = pathNameOption.Value().Unquote().PathEnds() ?? "";
                        if (!Directory.Exists(path)) errors.Add(string.Format("Path {0} does not exist. ", path));
                        returnCode = MassRenameTask(massRenameFromStringOption.Value().Unquote() ?? "", massRenameToStringOption?.Value()?.Unquote() ?? "", path);
                    }
                    else
                    {
                        returnCode = ProcessTemplateTask();
                    }
                    if (errors.Count>0) throw new Exception(errors.ToString());
                    Environment.ExitCode = (int)returnCode.Result;
                    Environment.Exit(Environment.ExitCode);
                    return Environment.ExitCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error executing Code Gen Cli. " + ex.Message);
                    Console.WriteLine("Stack Trace:");
                    Console.WriteLine(ex.StackTrace);
                    Environment.ExitCode = (int)ReturnCode.Error;
                    Environment.Exit(Environment.ExitCode);
                    return Environment.ExitCode;
                }

            });

        }

        static void StatusChangeEventHandler(object sender, StatusChangeEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
        private static int DirectoryCount { get; set; } = 0;
        private static int FileCount { get; set; } = 0;

        public static ReturnCodes MassRenameTask(string FromString, string ToString, string Path)
        {
            Console.WriteLine(string.Format("Performing Mass Rename (file name and file contents) in path {0} from {1} to {2}....", Path, FromString, ToString));
            string tempPath = System.IO.Path.GetTempPath() + "EzDbCodeGen" + System.IO.Path.DirectorySeparatorChar;
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
            if (AppSettings.Instance.VerboseMessages) Console.WriteLine(string.Format("Creating Temporary Path at '{0}'", tempPath));
            System.IO.Directory.CreateDirectory(tempPath);
            Copy(Path, tempPath, FromString);
            Console.WriteLine(string.Format("Performing Mass Rename (file name and file contents) completed - Directory: {0}, Files: {1}", DirectoryCount, FileCount));
            var returnCode = new ReturnCodes("MassRename", ReturnCode.Ok);
            return returnCode;
        }

        public static ReturnCodes ProcessTemplateTask()
        {
            var pfx = "Unknown: ";
            var schemaName = "MySchema";
            if (schemaNameOption.HasValue()) schemaName = schemaNameOption.Value();
            if (sourceConnectionStringOption.HasValue())
            {
                AppSettings.Instance.ConnectionString = sourceConnectionStringOption.Value().SettingResolution();
            }

            if (configFileOption.HasValue()) AppSettings.Instance.ConfigurationFileName = configFileOption.Value();

            var Errors = new StringBuilder();
            var OutputPath = string.Empty;
            if ((!templateFileNameOrDirectoryOption.HasValue()) || (templateFileNameOrDirectoryOption.Value().Length == 0)) Errors.AppendLine("TemplateName is missing or empty. ");
            if ((pathNameOption.HasValue()) && (pathNameOption.Value().Length > 0)) OutputPath = pathNameOption.Value();
            if ((!sourceSchemaFileNameOption.HasValue()) && (AppSettings.Instance.ConnectionString.Length == 0))
                Errors.AppendLine("ConnectionString and schemaFileName are both missing or empty. ");
            if ((sourceSchemaFileNameOption.HasValue()) && (!File.Exists(sourceSchemaFileNameOption.Value())))
                Errors.AppendLine(string.Format("Schema file '{0}' was does not exists! ", sourceSchemaFileNameOption.Value()));
            if (Errors.Length > 0)
            {
                throw new Exception(Errors.ToString());
            }

            var TemplateFileNameOrPath = templateFileNameOrDirectoryOption.Value();
            if (Path.GetPathRoot(TemplateFileNameOrPath).Length == 0)
            {
                TemplateFileNameOrPath = ("{ASSEMBLY_PATH}" + TemplateFileNameOrPath).ResolvePathVars();
            }
            if (!(
                (File.Exists(TemplateFileNameOrPath)) ||
                (Directory.Exists(TemplateFileNameOrPath)
               ))) TemplateFileNameOrPath = ("{ASSEMBLY_PATH}" + TemplateFileNameOrPath).ResolvePathVars();
            if (!((File.Exists(TemplateFileNameOrPath)) || (Directory.Exists(TemplateFileNameOrPath))))
                throw new Exception(string.Format("Template not found in path {0}", TemplateFileNameOrPath));

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
                SchemaName = schemaName,
                VerboseMessages = AppSettings.Instance.VerboseMessages,
                ConfigurationFileName = AppSettings.Instance.ConfigurationFileName,
                ProjectPath = ((projectFileToModifyOption.HasValue()) ? projectFileToModifyOption.Value() : ""),
                TemplateFileNameFilter = ((templateFilterFileMasks.HasValue()) ? templateFilterFileMasks.Value() : "")
            };

            var version = Assembly.GetAssembly(typeof(CodeGenerator)).GetName().Version;
            Console.WriteLine(pfx + "Using CodeGenerator Version " + version);

            CodeGen.OnStatusChangeEventArgs += StatusChangeEventHandler;

            var returnCode = new ReturnCodes();
            ITemplateInput Source = null;
            if (sourceSchemaFileNameOption.HasValue())
                Source = new TemplateInputFileSource(sourceSchemaFileNameOption.Value());
            else
                Source = new TemplateInputDatabaseConnecton(AppSettings.Instance.ConnectionString);

            ITemplateInput CompareTo = null;
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
            Console.WriteLine("Render of template " + templateFileNameOrDirectoryOption.Value() + " Completed!");

            return returnCode;
        }

        private static CommandOption verboseOption { get; set; }
        private static CommandOption templateFileNameOrDirectoryOption { get; set; }
        private static CommandOption pathNameOption { get; set; }
        private static CommandOption sourceConnectionStringOption { get; set; }
        private static CommandOption sourceSchemaFileNameOption { get; set; }
        private static CommandOption configFileOption { get; set; }
        private static CommandOption compareToConnectionStringOption { get; set; }
        private static CommandOption compareToSchemaFileNameOption { get; set; }
        private static CommandOption schemaNameOption { get; set; }
        private static CommandOption projectFileToModifyOption { get; set; }
        private static CommandOption templateFilterFileMasks { get; set; }
        private static CommandOption massRenameFromStringOption { get; set; }
        private static CommandOption massRenameToStringOption { get; set; }

        public static void Copy(string sourceDirectory, string targetDirectory, List<Tuple<string, string>> stringReplacements)
        {
            DirectoryCount = 0;
            FileCount = 0;

            if (AppSettings.Instance.VerboseMessages) Console.WriteLine(string.Format("Copying Path '{0}' to '{1}' while changing file name and content using '{2}'", sourceDirectory, targetDirectory, stringReplacements.ToString()));
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget, stringReplacements, targetDirectory);
        }

        private static void Copy(string sourceDirectory, string targetDirectory, string stringReplacementString)
        {
            var stringReplacements = new List<Tuple<string, string>>();
            var arr = stringReplacementString.Split(',');
            foreach (var pair in arr)
            {
                var kv = pair.Split("=", StringSplitOptions.RemoveEmptyEntries);
                stringReplacements.Add(new Tuple<string, string>(kv[0], kv[1]));
            }
            Copy(sourceDirectory, targetDirectory, stringReplacements);
        }
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target, List<Tuple<string, string>> stringReplacements, string rootDirectory = "")
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                FileCount++;
                var fileName = fi.Name;
                foreach(var replacePair in stringReplacements) fileName = fileName.Replace(replacePair.Item1, replacePair.Item2);
                if (AppSettings.Instance.VerboseMessages) Console.WriteLine(@"Copying {0}\{1}", target.FullName, fileName);
                string str = File.ReadAllText(Path.Combine(source.FullName, fi.Name));
                var changes = 0;
                foreach (var replacePair in stringReplacements)
                {
                    if (str.Contains(replacePair.Item1))
                    {
                        changes++;
                        str = str.Replace(replacePair.Item1, replacePair.Item2);
                    }
                }
                File.WriteAllText(Path.Combine(target.FullName, fileName), str);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryCount++;
                var directoryName = diSourceSubDir.Name;
                foreach (var replacePair in stringReplacements) directoryName = directoryName.Replace(replacePair.Item1, replacePair.Item2);
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(directoryName);
                if (AppSettings.Instance.VerboseMessages) Console.WriteLine(@"Creating {0}", directoryName);
                CopyAll(diSourceSubDir, nextTargetSubDir, stringReplacements, rootDirectory);
            }
        }
    }
}