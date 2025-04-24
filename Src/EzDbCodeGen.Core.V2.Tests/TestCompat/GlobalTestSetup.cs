using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.V2.Handlebars;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbCodeGen.Core.V2.Schema;
using EzDbCodeGen.Core.V2.Tests.Mocks;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Global test setup class to enable compatibility between V1 tests and V2 implementation
    /// </summary>
    public static class GlobalTestSetup
    {
        private static readonly MockFileSystem DefaultMockFileSystem = new MockFileSystem();
        private static bool _initialized;
        
        /// <summary>
        /// Initializes test environment with compatibility patches
        /// </summary>
        public static void InitializeTests()
        {
            if (_initialized)
            {
                return;
            }
            
            // Apply compatibility patches
            PatchConstructors();
            PatchMethods();
            
            _initialized = true;
        }
        
        /// <summary>
        /// Patches constructors to handle missing parameters
        /// </summary>
        private static void PatchConstructors()
        {
            // This is a conceptual example - in a real implementation, we would use a library like
            // Harmony or similar to do runtime patching. For our current implementation, we're using
            // the compatibility layer classes we've already created.
        }
        
        /// <summary>
        /// Patches methods to provide compatibility with V1 tests
        /// </summary>
        private static void PatchMethods()
        {
            // Similarly, this would use method patching but we're using our compatibility classes instead
        }
        
        /// <summary>
        /// Creates a SchemaAnalyzer with default mock file system
        /// </summary>
        public static SchemaAnalyzer CreateSchemaAnalyzer()
        {
            return new SchemaAnalyzer(DefaultMockFileSystem);
        }
        
        /// <summary>
        /// Creates a HandlebarsTemplateEngine with default mock file system
        /// </summary>
        public static HandlebarsTemplateEngine CreateHandlebarsTemplateEngine()
        {
            return new HandlebarsTemplateEngine(DefaultMockFileSystem);
        }
        
        /// <summary>
        /// Creates a TemplateProcessor with default mock file system
        /// </summary>
        public static TemplateProcessor CreateTemplateProcessor(ITemplateEngine templateEngine)
        {
            return new TemplateProcessor(templateEngine, DefaultMockFileSystem);
        }
        
        /// <summary>
        /// Creates a SchemaDiff with default mock file system
        /// </summary>
        public static SchemaDiff CreateSchemaDiff()
        {
            return new SchemaDiff(DefaultMockFileSystem);
        }
        
        /// <summary>
        /// Creates a DefaultCodeGenerator with default mock dependencies
        /// </summary>
        public static DefaultCodeGenerator CreateDefaultCodeGenerator(
            ITemplateProcessor templateProcessor,
            ISchemaAnalyzer schemaAnalyzer,
            ISchemaDiff? schemaDiff = null)
        {
            return new DefaultCodeGenerator(
                templateProcessor,
                DefaultMockFileSystem,
                schemaAnalyzer,
                schemaDiff);
        }
    }
}
