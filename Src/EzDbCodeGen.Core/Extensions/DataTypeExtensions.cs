using System;
using System.Collections.Generic;
using System.Linq;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for data type operations.
    /// </summary>
    public static class DataTypeExtensions
    {
        private static readonly Dictionary<string, string> DataTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Updates the .NET data type mapping.
        /// </summary>
        /// <param name="sourceDataType">The source data type.</param>
        /// <param name="targetDataType">The target .NET data type.</param>
        public static void UpdateDotNetDataType(string sourceDataType, string targetDataType)
        {
            if (string.IsNullOrEmpty(sourceDataType))
                throw new ArgumentNullException(nameof(sourceDataType));

            if (string.IsNullOrEmpty(targetDataType))
                throw new ArgumentNullException(nameof(targetDataType));

            // Add or update the mapping
            DataTypeMap[sourceDataType] = targetDataType;
        }

        /// <summary>
        /// Gets the .NET data type for a given source data type.
        /// </summary>
        /// <param name="sourceDataType">The source data type.</param>
        /// <returns>The mapped .NET data type, or the source type if no mapping exists.</returns>
        public static string GetDotNetDataType(string sourceDataType)
        {
            if (string.IsNullOrEmpty(sourceDataType))
                return string.Empty;

            return DataTypeMap.TryGetValue(sourceDataType, out var targetType) 
                ? targetType 
                : sourceDataType;
        }
        
        /// <summary>
        /// Converts a database type to its .NET equivalent.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <param name="isNullable">Whether the type is nullable.</param>
        /// <returns>The .NET type as a string.</returns>
        public static string ToNetType(this string dbType, bool isNullable = false)
        {
            if (string.IsNullOrEmpty(dbType))
                return "object";

            string netType = dbType.ToLower() switch
            {
                "int" => "int",
                "bigint" => "long",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",
                "decimal" => "decimal",
                "money" => "decimal",
                "float" => "float",
                "real" => "double",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "date" => "DateTime",
                "time" => "TimeSpan",
                "char" => "string",
                "nchar" => "string",
                "varchar" => "string",
                "nvarchar" => "string",
                "text" => "string",
                "ntext" => "string",
                "uniqueidentifier" => "Guid",
                "binary" => "byte[]",
                "varbinary" => "byte[]",
                "image" => "byte[]",
                _ => "object"
            };

            // If the type is a value type and is nullable, add the nullable modifier
            if (isNullable && netType != "string" && netType != "byte[]" && netType != "object")
            {
                netType += "?";
            }

            return netType;
        }
    }
}
