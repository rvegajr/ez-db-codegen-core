using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbSchema.Core.V2.Interfaces;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions
{
    /// <summary>
    /// Extension methods for DefaultCodeGenerator compatibility with V1 tests
    /// </summary>
    public static class DefaultCodeGeneratorExtensions
    {
        /// <summary>
        /// Compatibility method for V1 Generate method
        /// </summary>
        public static CodeGenerationResult Generate(
            this ICodeGenerator generator,
            IDatabase schema,
            string outputDir)
        {
            // Ensure the generator is not null following proper null reference handling
            ArgumentNullException.ThrowIfNull(generator, nameof(generator));
            ArgumentNullException.ThrowIfNull(schema, nameof(schema));
            ArgumentException.ThrowIfNullOrEmpty(outputDir, nameof(outputDir));
            
            // Set output directory
            generator.OutputDirectory = outputDir;
            
            // Call the async method synchronously
            return generator.GenerateCodeAsync(schema).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Compatibility method for V1 GenerateDiff with 4 parameters
        /// </summary>
        public static CodeGenerationResult GenerateDiff(
            this ICodeGenerator generator,
            IDatabase originalSchema,
            IDatabase newSchema,
            string outputDir,
            bool generateAll = false)
        {
            // Ensure the generator is not null following proper null reference handling
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
