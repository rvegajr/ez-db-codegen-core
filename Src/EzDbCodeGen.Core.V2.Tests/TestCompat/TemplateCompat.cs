using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Compatibility extensions for Template class to support tests designed for V1
    /// </summary>
    public static class TemplateCompat
    {
        /// <summary>
        /// Creates a Template with V1-compatible signature
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
            return new Template(templatePath, outputPath, entityGenerator, layoutPath, language)
            {
                // Set content directly to avoid loading from file system
                TemplateContent = content
            };
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
        /// Gets whether to skip generating a file when output is empty
        /// </summary>
        public static bool GetSkipWhenOutputIsEmpty(this Template template)
        {
            return _skipWhenOutputIsEmpty.TryGetValue(template, out var value) && value;
        }
        
        /// <summary>
        /// Sets whether to skip generating a file when output is empty
        /// </summary>
        public static void SetSkipWhenOutputIsEmpty(this Template template, bool value)
        {
            _skipWhenOutputIsEmpty[template] = value;
        }
        
        /// <summary>
        /// Gets whether to skip generating a file when it already exists
        /// </summary>
        public static bool GetSkipWhenFileExists(this Template template)
        {
            return _skipWhenFileExists.TryGetValue(template, out var value) && value;
        }
        
        /// <summary>
        /// Sets whether to skip generating a file when it already exists
        /// </summary>
        public static void SetSkipWhenFileExists(this Template template, bool value)
        {
            _skipWhenFileExists[template] = value;
        }
    }
}
