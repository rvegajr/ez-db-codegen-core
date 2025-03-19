using System;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Enums;

namespace EzDbCodeGen.Cli;

/// <summary>
/// Example Usages:
///         -t "Templates/SchemaRenderAsFiles.hbs" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa"
///         -t "Templates/SchemaRenderAsFilesNoOutput.hbs" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa" -p "C:\Temp\EzDbCodeGen"
///         -t "TemplatePAth" -sc "Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa" -cf "configPath"
/// </summary>

public class Program
{
    public static int Main(string[] args)
    {
        try
        {
            // Special handling for version and help arguments
            if (args.Contains("--version") || args.Contains("-v"))
            {
                var version = typeof(Program).Assembly.GetName().Version;
                Console.WriteLine($"EzDbCodeGen Tool Version {version}");
                return (int)ReturnCode.Ok;
            }
            
            if (args.Contains("--help") || args.Contains("-h") || args.Contains("-?"))
            {
                var app = new CommandLineApplication<CommandMain>();
                app.Conventions.UseDefaultConventions();
                app.ShowHelp();
                return (int)ReturnCode.Ok;
            }
            
            // Normal command processing
            var cmdApp = new CommandLineApplication<CommandMain>();
            cmdApp.Conventions.UseDefaultConventions();
            return cmdApp.Execute(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return (int)ReturnCode.Error;
        }
    }

    static void LoadSettings()
    {
        var schemaCoreVersion = typeof(EzDbSchema.Core.Objects.Entity).Assembly.GetName().Version;
        var schemaMssqlVersion = typeof(EzDbSchema.MsSql.Database).Assembly.GetName().Version;
        var codeGenCliVersion = typeof(EzDbCodeGen.Cli.Program).Assembly.GetName().Version;
        var codeGenCoreVersion = typeof(EzDbCodeGen.Core.CodeGenBase).Assembly.GetName().Version;
        
        AppSettings.Instance.SchemaCoreVersion = schemaCoreVersion?.ToString() ?? "0.0.0.0";
        AppSettings.Instance.SchemaMssqlVersion = schemaMssqlVersion?.ToString() ?? "0.0.0.0";
        AppSettings.Instance.CodeGenCliVersion = codeGenCliVersion?.ToString() ?? "0.0.0.0";
        AppSettings.Instance.CodeGenCoreVersion = codeGenCoreVersion?.ToString() ?? "0.0.0.0";
        AppSettings.Instance.Version = AppSettings.Instance.CodeGenCliVersion;
    }
}
