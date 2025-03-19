using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Extentions;
using EzDbSchema.Core.Extentions.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Text.Json.Nodes.JsonNode>;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Extentions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Internal
{
    /// <summary>
    /// Represents the application settings.
    /// </summary>
    internal class AppSettings 
    {
        /// <summary>
        /// Gets or sets the configuration file name.
        /// </summary>
        public string ConfigurationFileName { get; set; } = "";

        private Configuration? codeGenConfiguration;
        /// <summary>
        /// Gets or sets the code generation configuration.
        /// </summary>
        public Configuration Configuration
        {
            get
            {
                if (codeGenConfiguration == null)
                {
                    // Try to load from file if it exists, otherwise create a default configuration
                    if (!string.IsNullOrEmpty(ConfigurationFileName) && File.Exists(ConfigurationFileName))
                    {
                        codeGenConfiguration = EzDbCodeGen.Core.Config.Configuration.FromFile(ConfigurationFileName);
                    }
                    else
                    {
                        codeGenConfiguration = new Configuration();
                        codeGenConfiguration.SourceFileName = ConfigurationFileName;
                    }
                }
                
                // Update source filename if it changed
                if (!string.IsNullOrEmpty(ConfigurationFileName) && 
                    codeGenConfiguration.SourceFileName != this.ConfigurationFileName)
                {
                    if (File.Exists(ConfigurationFileName))
                    {
                        codeGenConfiguration = EzDbCodeGen.Core.Config.Configuration.FromFile(ConfigurationFileName);
                    }
                    else
                    {
                        codeGenConfiguration.SourceFileName = ConfigurationFileName;
                    }
                }
                
                return codeGenConfiguration;
            }
            set 
            {
                codeGenConfiguration = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display verbose messages.
        /// </summary>
        public bool VerboseMessages { get; set; } = false;
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; } = "";
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; } = "";
        /// <summary>
        /// Gets or sets the schema core version.
        /// </summary>
        public string SchemaCoreVersion { get; set; } = "";
        /// <summary>
        /// Gets or sets the schema MSSQL version.
        /// </summary>
        public string SchemaMssqlVersion { get; set; } = "";
        /// <summary>
        /// Gets or sets the code gen core version.
        /// </summary>
        public string CodeGenCoreVersion { get; set; } = "";
        /// <summary>
        /// Gets or sets the code gen CLI version.
        /// </summary>
        public string CodeGenCliVersion { get; set; } = "";
        private static AppSettings? instance;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings"/> class.
        /// </summary>
        internal AppSettings()
        {
            // Use a default config path but don't require it to exist
            this.ConfigurationFileName = "{ASSEMBLY_PATH}ezdbcodegen.config.json".ResolvePathVars(Environment.GetEnvironmentVariable);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings"/> class.
        /// </summary>
        /// <param name="configurationFileName">Name of the configuration file.</param>
        internal AppSettings(string configurationFileName)
        {
            this.ConfigurationFileName = configurationFileName;
        }

        /// <summary>
        /// Loads the application settings from the specified configuration file.
        /// </summary>
        /// <param name="configurationFileName">Name of the configuration file.</param>
        /// <returns>An instance of the <see cref="AppSettings"/> class.</returns>
        internal static AppSettings LoadFrom(string configurationFileName)
        {
            if (string.IsNullOrEmpty(configurationFileName))
            {
                throw new ArgumentException("Configuration file name cannot be null or empty.", nameof(configurationFileName));
            }

            // If file doesn't exist, create a default AppSettings instance
            if (!File.Exists(configurationFileName))
            {
                var defaultSettings = new AppSettings(configurationFileName);
                return defaultSettings;
            }

            var appsettingsText = File.ReadAllText(configurationFileName);
            var jsonObject = JsonNode.Parse(appsettingsText)?.AsObject() 
                ?? throw new System.Text.Json.JsonException($"Failed to parse {configurationFileName} as JSON object");

            var instance = new AppSettings();
            foreach (var property in jsonObject)
            {
                var propertyInfo = instance.GetType().GetProperty(property.Key);
                if (propertyInfo != null) 
                {
                    var stringValue = property.Value?.GetValue<string>();
                    if (propertyInfo.PropertyType == typeof(bool))
                    {
                        if (bool.TryParse(stringValue, out bool boolValue))
                        {
                            propertyInfo.SetValue(instance, boolValue);
                        }
                    }
                    else
                    {
                        propertyInfo.SetValue(instance, stringValue ?? "");
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// Gets the instance of the application settings.
        /// </summary>
        internal static AppSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    var configFileName = "{ASSEMBLY_PATH}appsettings.json".ResolvePathVars(Environment.GetEnvironmentVariable);
                    try
                    {
                        if (File.Exists(configFileName))
                        {
                            instance = AppSettings.LoadFrom(configFileName);
                        }
                        else
                        {
                            instance = new AppSettings();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception($"Error while parsing {configFileName}. {ex.Message}", ex);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the value of a configuration variable by name.
        /// </summary>
        /// <param name="VarName">The name of the variable to fetch.</param>
        /// <returns>The value of the variable or null if not found.</returns>
        internal static string? Var(string VarName)
        {
            if (string.IsNullOrEmpty(VarName))
            {
                throw new ArgumentException("Variable name cannot be null or empty.", nameof(VarName));
            }

            if (VarName.Equals("ConnectionString"))
            {
                return AppSettings.Instance.ConnectionString;
            }
            return null;
        }
    }
}
