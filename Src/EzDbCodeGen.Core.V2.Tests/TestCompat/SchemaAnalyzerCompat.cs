using System;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Schema;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbSchema.Core.V2.Interfaces;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Compatibility extensions for SchemaAnalyzer
    /// </summary>
    public static class SchemaAnalyzerCompat
    {
        /// <summary>
        /// Compatibility wrapper for SchemaAnalyzer constructor
        /// </summary>
        public static SchemaAnalyzer CreateSchemaAnalyzer()
        {
            var mockFileSystem = new MockFileSystem();
            return new SchemaAnalyzer(mockFileSystem);
        }
        
        /// <summary>
        /// Compatibility method for V1 synchronous AnalyzeSchema method
        /// </summary>
        public static IDatabase AnalyzeSchema(this ISchemaAnalyzer analyzer, IDatabase schema)
        {
            return analyzer.AnalyzeSchemaAsync(schema).GetAwaiter().GetResult();
        }
    }
}
