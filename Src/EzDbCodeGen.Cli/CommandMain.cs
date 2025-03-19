using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extentions;
using System.Text.Json;
using EzDbSchema.Core.Extentions.Json;
using EzDbSchema.Core.Extentions.Strings;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Internal;
using EzDbSchema.MsSql;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Cli.Extensions;

namespace EzDbCodeGen.Cli;

[Command(Name = "ezdbcg", Description = "EzDbCodeGen CLI Tool")]
public class CommandMain
{
    private const string Prefix = "EzDbCodeGen: ";
    
    [Option("-a|--app-name", "Application name", CommandOptionType.SingleValue)]
    public string AppName { get; set; } = "MyApp";

    [Option("-s|--schema-name", "Schema name", CommandOptionType.SingleValue)]
    public string SchemaName { get; set; } = "MySchema";

    [Option("-t|--template", "Template file or directory path", CommandOptionType.SingleValue)]
    public string TemplateFileNameOrPath { get; set; } = string.Empty;

    [Option("-c|--connection-string", "Database connection string", CommandOptionType.SingleValue)]
    public string ConnectionString { get; set; } = string.Empty;

    [Option("-v|--verbose", "Enable verbose output", CommandOptionType.NoValue)]
    public bool Verbose { get; set; }

    [Option("-o|--output", "Output directory path", CommandOptionType.SingleValue)]
    public string OutputPath { get; set; } = string.Empty;

    [Option("--save-settings", "Save current settings for future use", CommandOptionType.NoValue)]
    public bool SaveSettings { get; set; }
    
    [Option("--init-config", "Initialize a new configuration file", CommandOptionType.SingleValue)]
    public string InitConfigPath { get; set; } = string.Empty;

    private Settings Settings { get; set; } = new();
    private string SampleFilesPath { get; set; } = string.Empty;

    private void StatusChangeEventHandler(object? sender, StatusChangeEventArgs e)
    {
        Console.WriteLine(e.Message);
    }

    protected virtual string ReadLine()
    {
        return Console.ReadLine();
    }

    private void InteractiveConnectionString()
    {
        var iLoopCount = 0;
        while (string.IsNullOrEmpty(ConnectionString))
        {
            if (iLoopCount > 0)
            {
                Console.WriteLine($"{Prefix}Invalid connection string. Please try again.");
            }
            Console.Write($"{Prefix}Please enter a connection string: ");
            ConnectionString = ReadLine()?.SettingResolution() ?? "";
            iLoopCount++;
        }
    }

    private void InteractiveTemplatePath()
    {
        var iLoopCount = 0;
        while (!((File.Exists(TemplateFileNameOrPath)) || (Directory.Exists(TemplateFileNameOrPath))))
        {
            if (iLoopCount > 0)
            {
                Console.WriteLine($"{Prefix}Invalid template file or path. Please try again.");
            }
            Console.Write($"{Prefix}Please enter a template file or path: ");
            TemplateFileNameOrPath = ReadLine() ?? "";
            iLoopCount++;
        }
    }

    private async Task DownloadSampleFiles(string path, string appName)
    {
        var workPath = Path.GetTempPath() + @"EzDbCodeGen\";
        if (Directory.Exists(workPath)) Directory.Delete(workPath, true);
        Console.WriteLine($"{Prefix}Sample Files to be downloaded from https://github.com/rvegajr/ez-db-codegen-core to {workPath}");
        await WebFileHelper.CurlGitRepoZip(workPath);

        if (Directory.Exists(workPath))
        {
            var rootPath = workPath + @"ez-db-codegen-core-master\Src\EzDbCodeGen.Cli\";
            WebFileHelper.CopyTo(@"Templates\SchemaRender.hbs", rootPath, path);
            WebFileHelper.CopyTo(@"Templates\SchemaRenderAsFiles.hbs", rootPath, path);
            WebFileHelper.CopyTo(@"Templates\SchemaRenderAsFilesNoOutput.hbs", rootPath, path);
            WebFileHelper.CopyTo(@"ezdbcodegen.config.json", rootPath, path);
            WebFileHelper.CopyTo(@"ezdbcodegen.ps1", rootPath, path);

            WebFileHelper.CopyTo(@"ezdbcodegen.config.json", rootPath, path, appName + ".config.json")
                .ReplaceAll("MyEntities", appName + "Entities");
            WebFileHelper.CopyTo(@"ezdbcodegen.ps1", rootPath, path, appName + ".codegen.ps1")
                .ReplaceAll("ezdbcodegen", appName)
                .ReplaceAll("Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa", ConnectionString);
        }
    }

    protected virtual void SaveConfigToFile(string configPath, string content)
    {
        File.WriteAllText(configPath, content);
    }

    protected virtual bool FileExists(string path)
    {
        return File.Exists(path);
    }

    protected virtual string ReadFileContent(string path)
    {
        return File.ReadAllText(path);
    }

    protected virtual void WriteOutputFile(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
        File.WriteAllText(path, content);
    }

    private void SaveCurrentSettings()
    {
        Settings.AppName = AppName;
        Settings.SchemaName = SchemaName;
        Settings.ConnectionString = ConnectionString;
        Settings.TemplateFileNameOrPath = TemplateFileNameOrPath;
        Settings.AutoRun = true;
        Settings.Verbose = Verbose;
        Settings.Config = AppSettings.Instance.ConfigurationFileName;

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var configPath = Path.Combine(Environment.CurrentDirectory, "").PathEnds() + "ezdbcodegen.config.json";
        SaveConfigToFile(configPath, JsonSerializer.Serialize(Settings, jsonOptions));
    }

    protected virtual EzDbSchema.Core.Objects.Database CreateDatabase()
    {
        return new EzDbSchema.Core.Objects.Database();
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    protected virtual int OnExecute()
    {
        try
        {
            var version = Assembly.GetAssembly(typeof(CodeGenerator))?.GetName().Version;
            Console.WriteLine($"EzDbCodeGen Tool Version {version}");

            // If only the version was requested, return success
            if (Environment.GetCommandLineArgs().Contains("--version") || 
                Environment.GetCommandLineArgs().Contains("-v"))
            {
                return (int)ReturnCode.Ok;
            }
            
            // Check if we need to initialize a new configuration file
            if (!string.IsNullOrEmpty(InitConfigPath))
            {
                return InitializeConfigurationFile(InitConfigPath);
            }

            var configPath = Path.Combine(Environment.CurrentDirectory, "").PathEnds() + "ezdbcodegen.config.json";
            if (FileExists(configPath))
            {
                var settingsJsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                Settings = JsonSerializer.Deserialize<Settings>(ReadFileContent(configPath), settingsJsonOptions) ?? new Settings();

                if (Settings.AutoRun)
                {
                    Console.WriteLine("Using saved settings from 'ezdbcodegen.config.json'");
                    ConnectionString = Settings.ConnectionString ?? "";
                    TemplateFileNameOrPath = Settings.TemplateFileNameOrPath ?? "";
                    AppName = Settings.AppName;
                    SchemaName = Settings.SchemaName;
                    Verbose = Settings.Verbose;
                }
            }

            AppSettings.Instance.VerboseMessages = Verbose;
            AppSettings.Instance.ConnectionString = ConnectionString;

            if (string.IsNullOrEmpty(ConnectionString))
            {
                InteractiveConnectionString();
            }

            if (string.IsNullOrEmpty(TemplateFileNameOrPath))
            {
                InteractiveTemplatePath();
            }

            if (SaveSettings)
            {
                SaveCurrentSettings();
            }

            // Execute the code generation
            var generator = new CodeGenerator();
            generator.OnStatusChangeEventArgs += StatusChangeEventHandler;
            
            // Override the default file writer if needed
            if (generator.FileWriter == null)
            {
                generator.FileWriter = (path, content) => WriteOutputFile(path, content);
            }
            
            var returnCodes = generator.ProcessTemplate(TemplateFileNameOrPath, new TemplateInputDatabaseConnecton(ConnectionString) 
            {
                SchemaName = SchemaName,
                Schema = CreateDatabase(),
                VerboseMessages = Verbose
            }, OutputPath ?? Environment.CurrentDirectory);

            return (int)returnCodes.Result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return (int)ReturnCode.Error;
        }
    }

    /// <summary>
    /// Initializes a new configuration file at the specified path
    /// </summary>
    /// <param name="configPath">Path where the configuration file should be created</param>
    /// <returns>Return code indicating success or failure</returns>
    protected virtual int InitializeConfigurationFile(string configPath)
    {
        try
        {
            // Create a default configuration
            var config = new Configuration();
            
            // Set some default values
            config.SetConfigValue("Namespace", AppName);
            config.SetConfigValue("OutputPath", OutputPath);
            config.SetConfigValue("TemplatesPath", Path.Combine(Environment.CurrentDirectory, "Templates"));
            config.SetConfigValue("Database", new Dictionary<string, object>
            {
                { "Schema", SchemaName },
                { "ConnectionString", ConnectionString }
            });
            
            // Add default template settings
            config.Templates = new List<string>();
            config.TemplateFileNameFilter = new List<string>();
            
            // Set source file name
            config.SourceFileName = configPath;
            
            // Save the configuration to the specified path
            config.SaveToFile(configPath);
            
            Console.WriteLine($"Configuration file created at: {configPath}");
            Console.WriteLine("You can now edit this file to customize your code generation settings.");
            
            return (int)ReturnCode.Ok;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating configuration file: {ex.Message}");
            return (int)ReturnCode.Error;
        }
    }
}