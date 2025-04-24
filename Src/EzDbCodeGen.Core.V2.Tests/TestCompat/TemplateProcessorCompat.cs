using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using EzDbCodeGen.Core.V2.Handlebars;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbSchema.Core.V2.Interfaces;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Compatibility class for TemplateProcessor
    /// </summary>
    public static class TemplateProcessorCompat
    {
        /// <summary>
        /// Create a template processor with default mock file system
        /// </summary>
        public static TemplateProcessor CreateTemplateProcessor(ITemplateEngine templateEngine)
        {
            return new TemplateProcessor(templateEngine, new MockFileSystem());
        }
        
        /// <summary>
        /// Process a template synchronously (V1 compatibility)
        /// </summary>
        public static string Process(
            this ITemplateProcessor processor, 
            ITemplate template, 
            IDictionary<string, object>? data = null)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Ensure data is not null to follow null safety patterns
            data ??= new Dictionary<string, object>();
            
            // Get template content
            string templateContent = template.TemplateContent ?? string.Empty;
            
            // If the processor has a ProcessTemplateStringAsync method, use it
            if (processor.GetType().GetMethod("ProcessTemplateStringAsync") != null)
            {
                // Use reflection to call the method
                var method = processor.GetType().GetMethod("ProcessTemplateStringAsync");
                var task = method?.Invoke(processor, new object[] { templateContent, data });
                
                // Get the result
                if (task is Task<string> stringTask)
                {
                    return stringTask.GetAwaiter().GetResult();
                }
            }
            
            // Use a direct approach as fallback
            // Create a temporary file with the template content
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, templateContent);
                
                // Process the template file
                var tempTemplate = new Template("TempTemplate", templateContent, tempFile, "output.txt", "Temp");
                
                // Create a template engine if needed
                var templateEngine = new HandlebarsTemplateEngine(new MockFileSystem());
                
                // Do direct processing
                var compiledTemplate = templateEngine.Compile(templateContent, data);
                return templateEngine.Render(compiledTemplate, data);
            }
            finally
            {
                // Clean up the temporary file
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        
        /// <summary>
        /// Process a template for each entity synchronously (V1 compatibility)
        /// </summary>
        public static IDictionary<string, string> ProcessForEachEntity(
            this ITemplateProcessor processor, 
            ITemplate template, 
            IEnumerable<IEntity> entities, 
            IDictionary<string, object>? data = null)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }
            
            var results = new Dictionary<string, string>();
            foreach (var entity in entities)
            {
                var entityData = new Dictionary<string, object>(data)
                {
                    ["entity"] = entity
                };
                
                var output = processor.Process(template, entityData);
                var outputPath = template is Template t ? t.GetOutputFilePath(entity, "") : entity.Name;
                
                results[outputPath] = output;
            }
            
            return results;
        }
        
        /// <summary>
        /// Process a template to a file synchronously (V1 compatibility)
        /// </summary>
        public static void ProcessToFile(
            this ITemplateProcessor processor,
            ITemplate template,
            string outputPath,
            IDictionary<string, object>? data = null)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }
            
            // Process the template
            string result = Process(processor, template, data);
            
            // Skip empty output if configured
            if (string.IsNullOrWhiteSpace(result) && template is Template t && TemplateCompat.GetSkipWhenOutputIsEmpty(t))
            {
                return;
            }
            
            // Skip if file exists and configured to do so
            if (File.Exists(outputPath) && template is Template t2 && TemplateCompat.GetSkipWhenFileExists(t2))
            {
                return;
            }
            
            // Create directory if needed
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write to file
            File.WriteAllText(outputPath, result);
        }
        
        /// <summary>
        /// Process a template to a file for a specific entity synchronously (V1 compatibility)
        /// </summary>
        public static void ProcessToFile(
            this ITemplateProcessor processor,
            ITemplate template,
            string outputPath,
            IEntity entity,
            IDictionary<string, object>? data = null)
        {
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }
            
            data["entity"] = entity;
            ProcessToFile(processor, template, outputPath, data);
        }
        
        /// <summary>
        /// Process a template to a file with multiple parameter signatures for compatibility
        /// </summary>
        public static void ProcessToFile(
            this ITemplateProcessor processor,
            ITemplate template,
            IDictionary<string, object> data,
            string outputPath,
            IFileSystem fileSystem)
        {
            // Follow interface-first approach with proper null handling
            ArgumentNullException.ThrowIfNull(processor, nameof(processor));
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            ArgumentException.ThrowIfNullOrEmpty(outputPath, nameof(outputPath));
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            
            // Get the processed template content
            string content = processor.Process(template, data);
            
            // Ensure directory exists
            string? directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                fileSystem.CreateDirectory(directory);
            }
            
            // Write the file
            fileSystem.WriteAllText(outputPath, content);
        }
        
        /// <summary>
        /// Process a template to a file with entity parameter signature for compatibility
        /// </summary>
        public static void ProcessToFile(
            this ITemplateProcessor processor,
            ITemplate template,
            string outputPath,
            IEntity entity,
            IDictionary<string, object>? extraData = null)
        {
            // Follow interface-first approach with proper null handling
            ArgumentNullException.ThrowIfNull(processor, nameof(processor));
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            ArgumentException.ThrowIfNullOrEmpty(outputPath, nameof(outputPath));
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            
            // Create data dictionary
            var data = extraData ?? new Dictionary<string, object>();
            data["entity"] = entity;
            
            // Use the other overload
            processor.ProcessToFile(template, data, outputPath, new MockFileSystem());
        }
        
        /// <summary>
        /// Process a template to a file with schema parameter signature for compatibility
        /// </summary>
        public static void ProcessToFile(
            this ITemplateProcessor processor,
            ITemplate template,
            string outputPath,
            string schema,
            IDictionary<string, object>? extraData = null)
        {
            // Follow interface-first approach with proper null handling
            ArgumentNullException.ThrowIfNull(processor, nameof(processor));
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            ArgumentException.ThrowIfNullOrEmpty(outputPath, nameof(outputPath));
            ArgumentException.ThrowIfNullOrEmpty(schema, nameof(schema));
            
            // Create data dictionary
            var data = extraData ?? new Dictionary<string, object>();
            data["schema"] = schema;
            
            // Use the other overload
            processor.ProcessToFile(template, data, outputPath, new MockFileSystem());
        }
    }
}
