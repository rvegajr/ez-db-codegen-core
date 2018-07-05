using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Text;
using EzDbCodeGen.Internal;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core;

namespace Plex.CodeGen.Cli
{
    public static class CommandTemplateRender
    {
        public static string CommandName = "Template Render";
        public static string CommandOperator = "templaterender";
        /*
         * Example Usage - templaterender -t webapi -p "/Users/rvegajr/Downloads/Schema/Output/" -sc "Server=NSWIN10VM.local;Database=CPPE;user id=**USER**;password=***REMOVED***"
         * Example Usage - templaterender -t ef6context -p "/Users/rvegajr/Downloads/Schema/Output/templateContextRender.txt" -sf "/Users/rvegajr/Downloads/Schema/SchemaImage.db.json"
         * Example Usage - templaterender -t ef6models -p "/Users/rvegajr/Downloads/Schema/Output/templateContextRender.txt" -sf "/Users/rvegajr/Downloads/Schema/SchemaImage.db.json"
         * Example Usage - templaterender -t unitysvcreg -p "/Users/rvegajr/Downloads/Schema/Output/templateContextRender.txt" -sf "/Users/rvegajr/Downloads/Schema/SchemaImage.db.json"
         * Example Usage - templaterender -t nginterface -p "C:/dev/output/Schema/Output/" -sc "Server=DESKTOP-VODONH3\MSSQLSERVER01;Database=CPPE_DEV;user id=erlocal;password=warehouse"
         * Example Usage - templaterender -t ngmodelbase -p "C:/dev/output/Schema/base/" -sc "Server=DESKTOP-VODONH3\MSSQLSERVER01;Database=CPPE_DEV;user id=erlocal;password=warehouse"

        * */
        public static void Enable(CommandLineApplication app) {
            app.Command(CommandOperator, (command) =>
            {
                command.ExtendedHelpText = "Use the '" + CommandOperator + "' command to render a sepecific template.";
                command.Description = "Render a template";
                command.HelpOption("-?|-h|--help");

                var verboseOption = command.Option("-verbose|--verbose",
                    "Will output more detailed message about what is happening during application processing.  This parm is optional and will override the value in appsettings.json.   ",
                    CommandOptionType.NoValue);

                var templateFileName = command.Option("-t|--template <value>",
                    "The template file name that you wish to render",
                    CommandOptionType.SingleValue);

                var configFile = command.Option("-cf|--configfile",
                    "the configuration file this template render will use.  the default will be in the same path as this assembly of this applicaiton.  This parm is optional and will override the value in appsettings.json.   ",
                    CommandOptionType.SingleValue);

                var pathName = command.Option("-p|--path <value>",
                    "The template that you wish to render",
                    CommandOptionType.SingleValue);

                var sourceConnectionString = command.Option("-sc|--connection-string <optionvalue>",
                    "Connection String pass via the commandline.  This parm is optional and this value will override the value in appsettings.json. ",
                    CommandOptionType.SingleValue);

                var sourceSchemaFileName = command.Option("-sf|--schema-file <optionvalue>",
                    "Specify a schema json dump to perform the code generation (as opposed to a connection string).  This parm is optional and parm is present, it will override the appsettings and the -sc command line parm",
                CommandOptionType.SingleValue);

                var compareToConnectionString = command.Option("-tc|--compare-connection-string <optionvalue>",
                    "Connection String to compare to.  This parm is optional.  If it is present,  This schema will be compared to either the -sc or -sf and the only the changes will be updated.",
                    CommandOptionType.SingleValue);

                var compareToSchemaFileName = command.Option("-tf|--compare-schema-file <optionvalue>",
                "Specify a compare schema json dump to perform the (as opposed to a compare connection string).  This parm is optional and parm is present, it will override the -ts command line parameter and will be used to compare,  affecting only those entities that have changed.",
                CommandOptionType.SingleValue);


                command.OnExecute(() =>
                {
                    var pfx = "Unknown: ";
                    if (verboseOption.HasValue()) AppSettings.Instance.VerboseMessages = verboseOption.HasValue();
                    try
                    {
                        if (sourceConnectionString.HasValue()) AppSettings.Instance.ConnectionString = sourceConnectionString.Value();
                        if (configFile.HasValue()) AppSettings.Instance.ConfigurationFileName = configFile.Value();

                        var Errors = new StringBuilder();
                        if ((!templateFileName.HasValue()) || (templateFileName.Value().Length == 0)) Errors.AppendLine("TemplateName is missing or empty. ");
                        if ((!pathName.HasValue()) || (pathName.Value().Length == 0)) Errors.AppendLine("PathName is missing or empty. ");
                        if ((!sourceSchemaFileName.HasValue()) && (AppSettings.Instance.ConnectionString.Length == 0))
                            Errors.AppendLine("ConnectionString and schemaFileName are both missing or empty. ");
                        if ((sourceSchemaFileName.HasValue()) && (!File.Exists(sourceSchemaFileName.Value()))) 
                            Errors.AppendLine(string.Format("Schema file '{0}' was does not exists! ", sourceSchemaFileName.Value()));
                        if (Errors.Length>0) {
                            throw new Exception(Errors.ToString());
                        }
                        var TemplateFileName = templateFileName.Value();
                        pfx = Path.GetFileNameWithoutExtension(TemplateFileName) + ": ";
                        if (!File.Exists(TemplateFileName)) throw new Exception(string.Format("Template File {0} was not found!", TemplateFileName));

                        Console.WriteLine(pfx + "Performing " + CommandName + " " + pfx + "....");
                        if (AppSettings.Instance.VerboseMessages)
                        {
                            Console.WriteLine(pfx + "Template Name: " + TemplateFileName);
                            Console.WriteLine(pfx + "Path Name: " + pathName.Value());
                            if (!sourceSchemaFileName.HasValue()) Console.WriteLine(pfx + "Source Connection String: " + AppSettings.Instance.ConnectionString);
                            if (sourceSchemaFileName.HasValue()) Console.WriteLine(pfx + "Source Schema File Name: " + sourceSchemaFileName.Value());

                            if (compareToSchemaFileName.HasValue()) Console.WriteLine(pfx + "Compare To Schema File Name: " + compareToSchemaFileName.Value());
                            if ((!compareToSchemaFileName.HasValue()) && (compareToConnectionString.HasValue())) Console.WriteLine(pfx + "Compare To Connection String: " + compareToConnectionString.Value());
                        }
                        var CodeGen = new CodeGenerator(); 

                        var returnCode = ReturnCode.Ok;
                        ITemplateInput Source = null;
                        if (sourceSchemaFileName.HasValue())
                            Source = new TemplateInputFileSource(sourceSchemaFileName.Value());
                        else
                            Source =  new TemplateInputDatabaseConnecton(AppSettings.Instance.ConnectionString);

                        ITemplateInput CompareTo = null;
                        if (compareToSchemaFileName.HasValue())
                            CompareTo = new TemplateInputFileSource(compareToSchemaFileName.Value());
                        else if (compareToConnectionString.HasValue())
                            CompareTo = new TemplateInputDatabaseConnecton(compareToConnectionString.Value());

                        if (CompareTo==null) {
                            Source.VerboseMessages = AppSettings.Instance.VerboseMessages;
                            returnCode = CodeGen.ProcessTemplate(TemplateFileName, Source, pathName.Value());
                        } else {
                            Source.VerboseMessages = AppSettings.Instance.VerboseMessages;
                            CompareTo.VerboseMessages = AppSettings.Instance.VerboseMessages;
                            returnCode = CodeGen.ProcessTemplate(TemplateFileName, Source, CompareTo, pathName.Value());
                        }
                        Console.WriteLine(pfx + "" + CommandName + " of " + templateFileName.Value() + " Completed!");
                        Environment.ExitCode = (int)returnCode;
                        Environment.Exit(Environment.ExitCode);
                        return Environment.ExitCode;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(pfx + "Could not " + CommandName + ". " + ex.Message);
                        Console.WriteLine(pfx + "Stack Trace:");
                        Console.WriteLine(ex.StackTrace);
                        Environment.ExitCode = (int)ReturnCode.Error;
                        Environment.Exit(Environment.ExitCode);
                        return Environment.ExitCode;
                    }

                });
            });
        }
    }
}