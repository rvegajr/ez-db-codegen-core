using System;
using System.Collections.Generic;
using System.IO;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Enums;
using EzDbSchema.Core.Interfaces;
using HandlebarsDotNet;

namespace EzDbCodeGen.Tests.EFCore
{
    /// <summary>
    /// Template type enumeration for testing
    /// </summary>
    public enum TemplateType
    {
        Model,
        Controller,
        Repository,
        Service,
        Entity
    }

    /// <summary>
    /// Template class for testing
    /// </summary>
    public class Template
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public TemplateType Type { get; set; }
        public string Output { get; set; } = string.Empty;
        public string TemplatePath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string OutputFilename { get; set; } = string.Empty;
        
        public Template()
        {
        }
        
        public Template(string name, string path, TemplateType type, string output)
        {
            Name = name;
            Path = path;
            Type = type;
            Output = output;
        }
    }

    /// <summary>
    /// Extension methods for Configuration
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets the namespace from the configuration
        /// </summary>
        public static string Namespace(this EzDbCodeGen.Core.Config.Configuration config)
        {
            return config.HasConfigValue("Namespace") ? config.GetConfigValue<string>("Namespace") : "DefaultNamespace";
        }

        /// <summary>
        /// Gets the entity security settings from the configuration
        /// </summary>
        public static Dictionary<string, EntitySecurity> EntitySecurity(this EzDbCodeGen.Core.Config.Configuration config)
        {
            return config.HasConfigValue("EntitySecurity") 
                ? config.GetConfigValue<Dictionary<string, EntitySecurity>>("EntitySecurity") 
                : new Dictionary<string, EntitySecurity>();
        }
    }

    /// <summary>
    /// Entity security settings
    /// </summary>
    public class EntitySecurity
    {
        public bool Secured { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets the template path from a string
        /// </summary>
        public static string TemplatePath(this string basePath, string templateName)
        {
            return System.IO.Path.Combine(basePath, templateName);
        }
    }

    /// <summary>
    /// Helper classes and methods for tests
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Template type enum for tests
        /// </summary>
        public enum TemplateType
        {
            /// <summary>
            /// Entity template
            /// </summary>
            Entity,
            
            /// <summary>
            /// Database template
            /// </summary>
            Database
        }
        
        /// <summary>
        /// Template class for tests
        /// </summary>
        public class Template
        {
            /// <summary>
            /// Gets or sets the name of the template
            /// </summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the template path
            /// </summary>
            public string TemplatePath { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the output path
            /// </summary>
            public string OutputPath { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the output filename
            /// </summary>
            public string OutputFilename { get; set; } = string.Empty;
            
            /// <summary>
            /// Gets or sets the template type
            /// </summary>
            public TemplateType Type { get; set; }
        }
        
        /// <summary>
        /// Entity security class for tests
        /// </summary>
        public class EntitySecurity
        {
            /// <summary>
            /// Gets or sets a value indicating whether security is enabled for the entity
            /// </summary>
            public bool Enabled { get; set; }
        }
        
        /// <summary>
        /// Test code generator that doesn't rely on AppSettings
        /// </summary>
        public class TestCodeGenerator : CodeGenBase
        {
            private readonly Configuration _configuration;
            
            /// <summary>
            /// Initializes a new instance of the <see cref="TestCodeGenerator"/> class
            /// </summary>
            /// <param name="configuration">The configuration</param>
            public TestCodeGenerator(Configuration configuration)
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }
            
            /// <summary>
            /// Processes a template
            /// </summary>
            /// <param name="templatePath">Path to the template file</param>
            /// <param name="templateDataInput">Template data input</param>
            /// <param name="outputPath">Output path</param>
            public new void ProcessTemplate(string templatePath, ITemplateDataInput templateDataInput, string outputPath)
            {
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template file not found: {templatePath}");
                }
                
                // Read the template content
                var templateContent = File.ReadAllText(templatePath);
                
                // Register helpers
                HandlebarsExtensions.RegisterHelpers();
                
                // Compile the template
                var template = Handlebars.Compile(templateContent);
                
                // Create the output directory if it doesn't exist
                Directory.CreateDirectory(outputPath);
                
                // Get the entity name from the template data input
                var database = templateDataInput.Schema;
                
                // Process each entity
                foreach (var entity in database.Values)
                {
                    // Skip entities with security enabled
                    var entitySecurity = _configuration.GetConfigValue<Dictionary<string, EntitySecurity>>("EntitySecurity");
                    if (entitySecurity != null && 
                        entitySecurity.TryGetValue(entity.TableName, out var security) && 
                        !security.Enabled)
                    {
                        continue;
                    }
                    
                    // Create the output file path
                    var outputFilename = entity.TableName + ".cs";
                    var entityOutputPath = Path.Combine(outputPath, "Models");
                    Directory.CreateDirectory(entityOutputPath);
                    var outputFilePath = Path.Combine(entityOutputPath, outputFilename);
                    
                    // Create the template data
                    var templateData = new
                    {
                        Namespace = _configuration.GetConfigValue<string>("Namespace"),
                        TableName = entity.TableName,
                        Entity = entity,
                        Database = database
                    };
                    
                    // Apply the template
                    var result = template(templateData);
                    
                    // Write the result to the output file
                    File.WriteAllText(outputFilePath, result);
                }
            }
        }
    }
}
