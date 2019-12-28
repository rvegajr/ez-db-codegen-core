using System;
using System.Reflection;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Internal;
using McMaster.Extensions.CommandLineUtils;

namespace EzDbCodeGen.Cli
{
    /// <summary>
    /// Example Usages:
    ///         -t "Templates/SchemaRenderAsFiles.hbs" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa"
    ///         -t "Templates/SchemaRenderAsFilesNoOutput.hbs" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa" -p "C:\Temp\EzDbCodeGen"
    ///         -t "C:\Dev\PXD\cem-rest-api\CppeDb.WebApi\EzDbCodeGen\Templates\Ef6ModelsTemplate.hbs" -sc "Server=localhost;Database=CPPE;user id=**USER**;password=***REMOVED***" -cf "C:\Dev\PXD\cem-rest-api\CppeDb.WebApi\EzDbCodeGen\CppeDb.WebApi.config.json"
    /// </summary>

    class Program
    {
        private static CommandLineApplication App = new CommandLineApplication();
        static int Main(string[] args)
        {
            try
            {
                LoadSettings();
                CommandMain.Enable(App);
                App.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = (int)ReturnCode.Error;
                Environment.Exit(Environment.ExitCode);
                return Environment.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
                Environment.ExitCode = (int)ReturnCode.Error;
                Environment.Exit(Environment.ExitCode);
                return Environment.ExitCode;
            }
            Environment.ExitCode = (int)ReturnCode.Ok;
            Environment.Exit(Environment.ExitCode);
            return Environment.ExitCode;
        }

        static void LoadSettings()
        {
            AppSettings.Instance.Version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
