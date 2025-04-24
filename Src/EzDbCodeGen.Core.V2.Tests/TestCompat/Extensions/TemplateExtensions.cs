using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions
{
    /// <summary>
    /// Extension methods for Template compatibility with V1 tests
    /// Following interface-first design principles and proper null handling
    /// </summary>
    public static class TemplateExtensions
    {
        // Storage dictionaries for backwards compatibility
        private static readonly Dictionary<ITemplate, string> _categories = new Dictionary<ITemplate, string>();
        private static readonly Dictionary<ITemplate, bool> _skipWhenFileExists = new Dictionary<ITemplate, bool>();
        private static readonly Dictionary<ITemplate, bool> _processAsPartial = new Dictionary<ITemplate, bool>();
        
        /// <summary>
        /// Gets or sets the Content property (TemplateContent in V2)
        /// </summary>
        public static string Content(this ITemplate template)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            return template.TemplateContent ?? string.Empty;
        }
        
        /// <summary>
        /// Sets the Content property (TemplateContent in V2)
        /// </summary>
        public static void Content(this ITemplate template, string value)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            if (template is Template t)
            {
                t.TemplateContent = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the Category property (not in V2)
        /// </summary>
        public static string Category(this ITemplate template)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            return _categories.TryGetValue(template, out var value) ? value : "Default";
        }
        
        /// <summary>
        /// Sets the Category property (not in V2)
        /// </summary>
        public static void Category(this ITemplate template, string value)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            _categories[template] = value;
        }
        
        /// <summary>
        /// Gets or sets the SkipWhenFileExists property (not in V2)
        /// </summary>
        public static bool SkipWhenFileExists(this ITemplate template)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            return _skipWhenFileExists.TryGetValue(template, out var value) ? value : false;
        }
        
        /// <summary>
        /// Sets the SkipWhenFileExists property (not in V2)
        /// </summary>
        public static void SkipWhenFileExists(this ITemplate template, bool value)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            _skipWhenFileExists[template] = value;
        }
        
        /// <summary>
        /// Gets or sets the ProcessAsPartial property (not in V2)
        /// </summary>
        public static bool ProcessAsPartial(this ITemplate template)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            return _processAsPartial.TryGetValue(template, out var value) ? value : false;
        }
        
        /// <summary>
        /// Sets the ProcessAsPartial property (not in V2)
        /// </summary>
        public static void ProcessAsPartial(this ITemplate template, bool value)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            _processAsPartial[template] = value;
        }
    }
}
