using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions
{
    /// <summary>
    /// Extension methods for Template property compatibility with V1 tests
    /// Following interface-first design principles and proper null handling patterns.
    /// </summary>
    public static class TemplatePropertyExtensions
    {
        // Storage dictionaries for extension properties, following proper collection initialization
        private static readonly Dictionary<ITemplate, bool> _executeOnce = new Dictionary<ITemplate, bool>();
        private static readonly Dictionary<ITemplate, bool> _executeForEachEntity = new Dictionary<ITemplate, bool>();
        private static readonly Dictionary<ITemplate, bool> _executeForEachSchema = new Dictionary<ITemplate, bool>();
        
        /// <summary>
        /// Gets or sets the ExecuteOnce property for a template
        /// </summary>
        public static bool GetExecuteOnce(this ITemplate template)
        {
            // Follow null safety practices
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Check in the backing dictionary or default to true
            return _executeOnce.TryGetValue(template, out var value) ? value : !template.EntityGenerator;
        }
        
        /// <summary>
        /// Sets the ExecuteOnce property for a template
        /// </summary>
        public static void SetExecuteOnce(this ITemplate template, bool value)
        {
            // Follow null safety practices
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Store in the backing dictionary
            _executeOnce[template] = value;
            
            // For Template instances, also call the method to ensure consistency
            if (template is Template t)
            {
                if (value)
                {
                    t.ExecuteOnce();
                }
                else
                {
                    t.ExecuteForEachEntity();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the ExecuteForEachEntity property for a template
        /// </summary>
        public static bool GetExecuteForEachEntity(this ITemplate template)
        {
            // Follow null safety practices
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Check in the backing dictionary or default based on EntityGenerator
            return _executeForEachEntity.TryGetValue(template, out var value) ? value : template.EntityGenerator;
        }
        
        /// <summary>
        /// Sets the ExecuteForEachEntity property for a template
        /// </summary>
        public static void SetExecuteForEachEntity(this ITemplate template, bool value)
        {
            // Follow null safety practices
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Store in the backing dictionary
            _executeForEachEntity[template] = value;
            
            // For Template instances, also call the method to ensure consistency
            if (template is Template t)
            {
                if (value)
                {
                    t.ExecuteForEachEntity();
                }
                else
                {
                    t.ExecuteOnce();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the ExecuteForEachSchema property for a template
        /// </summary>
        public static bool GetExecuteForEachSchema(this ITemplate template)
        {
            // Follow null safety practices
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Check in the backing dictionary or default to false
            return _executeForEachSchema.TryGetValue(template, out var value) ? value : false;
        }
        
        /// <summary>
        /// Sets the ExecuteForEachSchema property for a template
        /// </summary>
        public static void SetExecuteForEachSchema(this ITemplate template, bool value)
        {
            // Follow null safety practices
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            
            // Store in the backing dictionary
            _executeForEachSchema[template] = value;
        }
    }
}
