using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.V2.Handlebars;
using EzDbCodeGen.Core.V2.Interfaces;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Compatibility extensions for HandlebarsTemplateEngine to bridge V1 tests with V2 implementation
    /// </summary>
    public static class HandlebarsTemplateEngineCompat
    {
        /// <summary>
        /// Compatibility version of Compile method that doesn't require data parameter
        /// </summary>
        public static Func<object, string> Compile(this HandlebarsTemplateEngine engine, string template)
        {
            // Get the version that requires data parameter
            var compiledTemplate = engine.Compile(template, new Dictionary<string, object>());
            
            // Return a function that wraps the template
            return (data) => 
            {
                // If data is null, use empty dictionary
                var actualData = data ?? new Dictionary<string, object>();
                
                // Call directly with the template content
                return engine.CompileAndRender(template, actualData);
            };
        }
        
        /// <summary>
        /// Compatibility version of CompileAndRender method
        /// </summary>
        public static string CompileAndRender(this HandlebarsTemplateEngine engine, string template, object data)
        {
            // If data is null, use empty dictionary
            var dataDict = data as IDictionary<string, object> ?? new Dictionary<string, object>();
            
            // Use the existing method that already works
            return engine.CompileAndRender(template, dataDict);
        }
        
        /// <summary>
        /// Compatibility version of Render method
        /// </summary>
        public static string Render(this HandlebarsTemplateEngine engine, string compiledTemplate, object data)
        {
            // If data is null, use empty dictionary following safe null handling patterns
            var dataDict = data as IDictionary<string, object> ?? new Dictionary<string, object>();
            
            // Process directly 
            return engine.CompileAndRender(compiledTemplate, dataDict);
        }
        
        /// <summary>
        /// Compatibility version of RegisterHelper that matches V1 signature
        /// </summary>
        public static void RegisterHelper(this ITemplateEngine engine, string name, HandlebarsHelper helper)
        {
            if (engine is HandlebarsTemplateEngine handlebarsEngine)
            {
                handlebarsEngine.RegisterHelper(name, helper);
            }
            else
            {
                throw new InvalidOperationException($"Expected HandlebarsTemplateEngine but got {engine.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Compatibility version of RegisterBlockHelper that matches V1 signature
        /// </summary>
        public static void RegisterBlockHelper(this ITemplateEngine engine, string name, HandlebarsBlockHelper helper)
        {
            if (engine is HandlebarsTemplateEngine handlebarsEngine)
            {
                handlebarsEngine.RegisterBlockHelper(name, helper);
            }
            else
            {
                throw new InvalidOperationException($"Expected HandlebarsTemplateEngine but got {engine.GetType().Name}");
            }
        }
    }
}
