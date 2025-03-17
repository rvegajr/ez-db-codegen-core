using System;
using System.Reflection;
using System.Threading.Tasks;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Internal;
using McMaster.Extensions.CommandLineUtils;

namespace EzDbCodeGen.Cli;

/// <summary>
/// Example Usages:
///         -t "Templates/SchemaRenderAsFiles.hbs" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa"
///         -t "Templates/SchemaRenderAsFilesNoOutput.hbs" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa" -p "C:\Temp\EzDbCodeGen"
///         -t "TemplatePAth" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa" -cf "configPath"
/// </summary>

    class Program
    {
        private static CommandLineApplication App = new CommandLineApplication();
        static async Task<int> Main(string[] args)
        {
            try
            {
                LoadSettings();
                await CommandMain.Enable(App);
                return App.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
                return (int)ReturnCode.Error;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
                return (int)ReturnCode.Error;
            }
        }

    static void LoadSettings()
    {
        AppSettings.Instance.SchemaCoreVersion = typeof(EzDbSchema.Core.Objects.Entity).Assembly.GetName().Version.ToString();
        AppSettings.Instance.SchemaMssqlVersion = typeof(EzDbSchema.MsSql.Database).Assembly.GetName().Version.ToString();
        AppSettings.Instance.CodeGenCliVersion = typeof(EzDbCodeGen.Cli.Program).Assembly.GetName().Version.ToString();
        AppSettings.Instance.CodeGenCoreVersion = typeof(EzDbCodeGen.Core.CodeGenBase).Assembly.GetName().Version.ToString();
        AppSettings.Instance.Version = AppSettings.Instance.CodeGenCliVersion;
    }
}
