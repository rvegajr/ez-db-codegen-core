using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EzDbCodeGen.Core.V2.Handlebars;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbCodeGen.Core.V2.Schema;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbSchema.Core.V2.Interfaces;
using EzDbSchema.Core.V2.Models;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Master compatibility class that provides extension methods to bridge V1 tests with V2 implementation
    /// </summary>
    public static class TestCompatibility
    {
        /// <summary>
        /// Singleton mock file system for testing
        /// </summary>
        private static readonly MockFileSystem DefaultMockFileSystem = new MockFileSystem();
        
        #region Template Compatibility
        
        /// <summary>
        /// Create a template with V2 constructor and proper interface-based design
        /// </summary>
        public static Template CreateTemplate(
            string name, 
            string templateContent, 
            string templatePath, 
            string outputPath, 
            bool entityGenerator = false,
            string? layoutPath = null)
        {
            // Create a template with the appropriate parameter order for V2
            var template = new Template(
                templatePath,
                outputPath,
                entityGenerator,
                layoutPath);
                
            // Set content directly
            template.TemplateContent = templateContent;
            
            return template;
        }
        
        /// <summary>
        /// Creates a template with V1 constructor but proper properties
        /// </summary>
        public static Template CreateLegacyTemplate(
            string name,
            string content,
            string templatePath,
            string outputPath,
            string language,
            bool entityGenerator = false,
            string? layoutPath = null)
        {
            // Create template with core parameters
            var template = new Template(
                templatePath, 
                outputPath, 
                entityGenerator,
                layoutPath);
                
            // Set content property directly
            template.TemplateContent = content;
            
            return template;
        }
        
        /// <summary>
        /// Loads a template from a file with V2 compatibility
        /// </summary>
        public static Template LoadTemplateFromFile(
            IFileSystem fileSystem,
            string templateFilePath,
            string name,
            string outputPath,
            string category)
        {
            // Check file exists
            if (!fileSystem.FileExists(templateFilePath))
            {
                throw new System.IO.FileNotFoundException($"Template file not found: {templateFilePath}");
            }
            
            // Read the content
            string content = File.ReadAllText(templateFilePath);
            
            // Create the template
            return CreateTemplate(
                name,
                content,
                templateFilePath,
                outputPath);
        }
        
        /// <summary>
        /// Extension to add SkipWhenOutputIsEmpty property to Template
        /// </summary>
        private static readonly Dictionary<Template, bool> _skipWhenOutputIsEmpty = new();
        
        /// <summary>
        /// Extension to add SkipWhenFileExists property to Template
        /// </summary>
        private static readonly Dictionary<Template, bool> _skipWhenFileExists = new();
        
        /// <summary>
        /// Extension for template to support ExecuteOnce property from V1
        /// </summary>
        public static bool ExecuteOnce(this Template template)
        {
            template.ExecuteOnce();
            return true;
        }
        
        /// <summary>
        /// Extension for template to support ExecuteForEachEntity property from V1
        /// </summary>
        public static bool ExecuteForEachEntity(this Template template)
        {
            template.ExecuteForEachEntity();
            return true;
        }
        
        #endregion
        
        #region TemplateProcessor Compatibility
        
        /// <summary>
        /// Creates a V2 TemplateProcessor with default mock file system
        /// </summary>
        public static TemplateProcessor CreateTemplateProcessor(ITemplateEngine templateEngine)
        {
            return new TemplateProcessor(templateEngine, DefaultMockFileSystem);
        }
        
        /// <summary>
        /// Synchronous process to file method for backward compatibility with tests
        /// </summary>
        public static void ProcessToFile(
            this ITemplateProcessor processor,
            ITemplate template,
            string outputPath,
            IDictionary<string, object>? data = null,
            IFileSystem? fileSystem = null)
        {
            // Ensure data is not null
            data ??= new Dictionary<string, object>();
            
            // Use default file system if not provided
            fileSystem ??= DefaultMockFileSystem;
            
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            ArgumentException.ThrowIfNullOrEmpty(outputPath, nameof(outputPath));
            
            // Process template
            string content = processor.ProcessTemplateStringAsync(template.TemplateContent ?? string.Empty, data).GetAwaiter().GetResult();
            
            // Check if content is empty and should be skipped
            if (string.IsNullOrEmpty(content) && template is Template templateImpl && TemplateCompat.GetSkipWhenOutputIsEmpty(templateImpl))
            {
                return;
            }
            
            // Check if file exists and should be skipped
            if (File.Exists(outputPath) && template is Template templateImplExists && TemplateCompat.GetSkipWhenFileExists(templateImplExists))
            {
                return;
            }
            
            // Create directory if it doesn't exist
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write content to file
            File.WriteAllText(outputPath, content);
        }
        
        #endregion
        
        #region SchemaDiff Compatibility
        
        /// <summary>
        /// Creates a SchemaDiff instance with a mock file system
        /// </summary>
        public static SchemaDiff CreateSchemaDiff()
        {
            return new SchemaDiff(DefaultMockFileSystem);
        }
        
        // ChangedEntities method moved to SchemaDiffCompat to avoid ambiguity
        
        /// <summary>
        /// Extension to provide an alias for IEntityDiffResult in case tests use IEntityDiff
        /// </summary>
        public class EntityDiff : IEntityDiffResult
        {
            private readonly IEntityDiffResult _innerResult;
            
            public EntityDiff(IEntityDiffResult innerResult)
            {
                _innerResult = innerResult;
            }
            
            public IEntity Entity => _innerResult.Entity;
            
            public IReadOnlyCollection<IProperty> AddedProperties => _innerResult.AddedProperties;
            
            public IReadOnlyCollection<IProperty> RemovedProperties => _innerResult.RemovedProperties;
            
            public IReadOnlyCollection<IProperty> ModifiedProperties => _innerResult.ModifiedProperties;
            
            public IReadOnlyCollection<IRelationship> AddedRelationships => _innerResult.AddedRelationships;
            
            public IReadOnlyCollection<IRelationship> RemovedRelationships => _innerResult.RemovedRelationships;
            
            public IReadOnlyCollection<IRelationship> ModifiedRelationships => _innerResult.ModifiedRelationships;
            
            public bool HasChanges => _innerResult.HasChanges;
        }
        
        /// <summary>
        /// Factory method to create a compatible EntityDiff from an IEntityDiffResult
        /// </summary>
        public static EntityDiff ToEntityDiff(this IEntityDiffResult result)
        {
            return new EntityDiff(result);
        }
        
        #endregion
        
        #region SchemaAnalyzer Compatibility
        
        /// <summary>
        /// Creates a SchemaAnalyzer instance with a mock file system
        /// </summary>
        public static SchemaAnalyzer CreateSchemaAnalyzer()
        {
            return new SchemaAnalyzer(DefaultMockFileSystem);
        }
        
        // AnalyzeSchema method moved to SchemaAnalyzerCompat to avoid ambiguity
        
        #endregion
        
        #region DefaultCodeGenerator Compatibility
        
        /// <summary>
        /// Creates a DefaultCodeGenerator with V2-compatible constructor
        /// </summary>
        public static DefaultCodeGenerator CreateDefaultCodeGenerator(
            ITemplateProcessor templateProcessor,
            IFileSystem fileSystem,
            ISchemaDiff? schemaDiff = null)
        {
            var schemaAnalyzer = new SchemaAnalyzer(fileSystem);
            return new DefaultCodeGenerator(templateProcessor, fileSystem, schemaAnalyzer, schemaDiff);
        }
        
        /// <summary>
        /// Creates a DefaultCodeGenerator with V1-compatible parameter order (reversed)
        /// </summary>
        public static DefaultCodeGenerator CreateDefaultCodeGeneratorV1Style(
            IFileSystem fileSystem,
            ITemplateProcessor templateProcessor,
            ISchemaDiff? schemaDiff = null)
        {
            // Swapping order for V2 compatibility
            return CreateDefaultCodeGenerator(templateProcessor, fileSystem, schemaDiff);
        }
        
        /// <summary>
        /// Extension method for template registration with compatibility for V1 style methods
        /// </summary>
        public static void RegisterTemplate(this ICodeGenerator generator, ITemplate template)
        {
            generator.AddTemplate(template);
        }
        
        /// <summary>
        /// Extension method for generating code with diff with compatibility for V1 style methods
        /// </summary>
        public static CodeGenerationResult GenerateDiff(
            this ICodeGenerator generator,
            IDatabase originalSchema,
            IDatabase newSchema,
            string outputDir,
            bool generateAll = false)
        {
            // Set output directory
            generator.OutputDirectory = outputDir;
            
            // Call the async method synchronously
            if (generator is DefaultCodeGenerator defaultGenerator)
            {
                return defaultGenerator.GenerateDiff(originalSchema, newSchema).GetAwaiter().GetResult();
            }
            
            throw new InvalidOperationException("Generator must be DefaultCodeGenerator");
        }
        
        #endregion
        
        #region HandlebarsTemplateEngine Compatibility
        
        /// <summary>
        /// Creates a HandlebarsTemplateEngine instance for testing
        /// </summary>
        public static HandlebarsTemplateEngine CreateHandlebarsTemplateEngine()
        {
            return new HandlebarsTemplateEngine(DefaultMockFileSystem);
        }
        
        #endregion
    }
}
