using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbSchema.Core.V2.Interfaces;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Compatibility extensions for DefaultCodeGenerator to support V1 test scenarios
    /// </summary>
    public static class DefaultCodeGeneratorCompat
    {
        // Storage dictionaries for backwards compatibility
        private static readonly Dictionary<ICodeGenerator, Dictionary<string, object>> _variables = 
            new Dictionary<ICodeGenerator, Dictionary<string, object>>();
            
        /// <summary>
        /// Creates a DefaultCodeGenerator with mock dependencies for testing
        /// Following interface-first approach with proper null handling
        /// </summary>
        public static DefaultCodeGenerator CreateDefaultCodeGenerator(
            ITemplateProcessor templateProcessor, 
            IFileSystem fileSystem, 
            ISchemaAnalyzer schemaAnalyzer, 
            ISchemaDiff? schemaDiff = null)
        {
            ArgumentNullException.ThrowIfNull(templateProcessor, nameof(templateProcessor));
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            ArgumentNullException.ThrowIfNull(schemaAnalyzer, nameof(schemaAnalyzer));
            
            return new DefaultCodeGenerator(
                templateProcessor, 
                fileSystem, 
                schemaAnalyzer, 
                schemaDiff);
        }
        
        /// <summary>
        /// Extension to access the SchemaAnalyzer from a DefaultCodeGenerator
        /// </summary>
        public static ISchemaAnalyzer SchemaAnalyzer(this ICodeGenerator generator)
        {
            ArgumentNullException.ThrowIfNull(generator, nameof(generator));
            
            // Get the schema analyzer from the DefaultCodeGenerator
            if (generator is DefaultCodeGenerator defaultGenerator)
            {
                // Use reflection to get the private field
                var field = typeof(DefaultCodeGenerator).GetField("_schemaAnalyzer", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                    
                return field?.GetValue(defaultGenerator) as ISchemaAnalyzer ?? 
                    TestCompatibility.CreateSchemaAnalyzer();
            }
            
            return TestCompatibility.CreateSchemaAnalyzer();
        }
        
        /// <summary>
        /// Extension method for template registration with compatibility for V1 style methods
        /// </summary>
        public static void RegisterTemplate(this ICodeGenerator generator, ITemplate template)
        {
            ArgumentNullException.ThrowIfNull(generator, nameof(generator));
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            generator.AddTemplate(template);
        }
        
        /// <summary>
        /// Property-like extension for accessing Variables (V1 compatibility)
        /// </summary>
        public static Dictionary<string, object> Variables(this ICodeGenerator generator)
        {
            ArgumentNullException.ThrowIfNull(generator, nameof(generator));
            
            // Create dictionary if it doesn't exist
            if (!_variables.TryGetValue(generator, out var variables))
            {
                variables = new Dictionary<string, object>();
                _variables[generator] = variables;
            }
            
            return variables;
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
            // Ensure null safety
            ArgumentNullException.ThrowIfNull(generator, nameof(generator));
            ArgumentNullException.ThrowIfNull(originalSchema, nameof(originalSchema));
            ArgumentNullException.ThrowIfNull(newSchema, nameof(newSchema));
            ArgumentException.ThrowIfNullOrEmpty(outputDir, nameof(outputDir));
            
            // Set output directory
            generator.OutputDirectory = outputDir;
            
            // For compatibility, when generateAll is true, just call Generate instead of GenerateDiff
            if (generateAll)
            {
                return generator.Generate(newSchema, outputDir);
            }
            
            // Call the V2 GenerateDiff method only with required parameters
            if (generator is DefaultCodeGenerator defaultGenerator)
            {
                return defaultGenerator.GenerateDiff(originalSchema, newSchema).GetAwaiter().GetResult();
            }
            
            throw new InvalidOperationException("Generator must be DefaultCodeGenerator");
        }
    }
}
