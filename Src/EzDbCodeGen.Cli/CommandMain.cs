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
        static void StatusChangeEventHandler(object sender, StatusChangeEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
        public static void Enable(CommandLineApplication app) {
            app.Name = "ezdb.codegen.cli";
            app.Description = "EzDbCodeGen - Code Generation Utility";
            app.ExtendedHelpText = "This application will allow you to trigger code generation based on a template file or a list of template files."
                + Environment.NewLine + "";

            app.HelpOption("-?|-h|--help");

            var verboseOption = app.Option("-v|--verbose",
                "Will output more detailed message about what is happening during application processing.  This parm is optional and will override the value in appsettings.json.   ",
                CommandOptionType.NoValue);

            var templateFileNameOrDirectoryOption = app.Option("-t|--template <value>",
                "The template file name or path that you wish to render.  If you choose aa path,  ",
                CommandOptionType.SingleValue);

            var pathNameOption = app.Option("-p|--ouputpath <value>",
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

            app.OnExecute(() =>
            {
                var pfx = "Unknown: ";
                if (verboseOption.HasValue()) AppSettings.Instance.VerboseMessages = verboseOption.HasValue();
                try
                {
                    var schemaName = "MySchema";
                    if (schemaNameOption.HasValue()) schemaName = schemaNameOption.Value();
                    if (sourceConnectionStringOption.HasValue()) AppSettings.Instance.ConnectionString = sourceConnectionStringOption.Value();
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
                    if (Path.GetPathRoot(TemplateFileNameOrPath).Length==0)
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
                        ProjectPath = ((projectFileToModifyOption.HasValue()) ? projectFileToModifyOption.Value() : "")
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
                    Environment.ExitCode = (int)returnCode.Result;
                    Environment.Exit(Environment.ExitCode);
                    return Environment.ExitCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not render template. " + ex.Message);
                    Console.WriteLine("Stack Trace:");
                    Console.WriteLine(ex.StackTrace);
                    Environment.ExitCode = (int)ReturnCode.Error;
                    Environment.Exit(Environment.ExitCode);
                    return Environment.ExitCode;
                }

            });

        }
    }
}