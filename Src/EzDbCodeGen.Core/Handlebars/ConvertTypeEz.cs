using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides data type conversion capabilities for Handlebars templates
    /// </summary>
    public class ConvertTypeEz
    {
        private readonly Dictionary<string, Dictionary<string, string>> _typeMap;

        /// <summary>
        /// Initializes a new instance of the ConvertTypeEz class
        /// </summary>
        public ConvertTypeEz()
        {
            _typeMap = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["csharp"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["int"] = "int",
                    ["bigint"] = "long",
                    ["smallint"] = "short",
                    ["tinyint"] = "byte",
                    ["bit"] = "bool",
                    ["decimal"] = "decimal",
                    ["numeric"] = "decimal",
                    ["money"] = "decimal",
                    ["smallmoney"] = "decimal",
                    ["float"] = "double",
                    ["real"] = "float",
                    ["datetime"] = "DateTime",
                    ["datetime2"] = "DateTime",
                    ["smalldatetime"] = "DateTime",
                    ["date"] = "DateTime",
                    ["time"] = "TimeSpan",
                    ["datetimeoffset"] = "DateTimeOffset",
                    ["char"] = "string",
                    ["varchar"] = "string",
                    ["nchar"] = "string",
                    ["nvarchar"] = "string",
                    ["text"] = "string",
                    ["ntext"] = "string",
                    ["xml"] = "string",
                    ["uniqueidentifier"] = "Guid",
                    ["binary"] = "byte[]",
                    ["varbinary"] = "byte[]",
                    ["image"] = "byte[]",
                    ["rowversion"] = "byte[]",
                    ["timestamp"] = "byte[]"
                },
                ["typescript"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["int"] = "number",
                    ["bigint"] = "number",
                    ["smallint"] = "number",
                    ["tinyint"] = "number",
                    ["bit"] = "boolean",
                    ["decimal"] = "number",
                    ["numeric"] = "number",
                    ["money"] = "number",
                    ["smallmoney"] = "number",
                    ["float"] = "number",
                    ["real"] = "number",
                    ["datetime"] = "Date",
                    ["datetime2"] = "Date",
                    ["smalldatetime"] = "Date",
                    ["date"] = "Date",
                    ["time"] = "string",
                    ["datetimeoffset"] = "Date",
                    ["char"] = "string",
                    ["varchar"] = "string",
                    ["nchar"] = "string",
                    ["nvarchar"] = "string",
                    ["text"] = "string",
                    ["ntext"] = "string",
                    ["xml"] = "string",
                    ["uniqueidentifier"] = "string",
                    ["binary"] = "any",
                    ["varbinary"] = "any",
                    ["image"] = "any",
                    ["rowversion"] = "any",
                    ["timestamp"] = "any"
                },
                ["java"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["int"] = "Integer",
                    ["bigint"] = "Long",
                    ["smallint"] = "Short",
                    ["tinyint"] = "Byte",
                    ["bit"] = "Boolean",
                    ["decimal"] = "BigDecimal",
                    ["numeric"] = "BigDecimal",
                    ["money"] = "BigDecimal",
                    ["smallmoney"] = "BigDecimal",
                    ["float"] = "Double",
                    ["real"] = "Float",
                    ["datetime"] = "Date",
                    ["datetime2"] = "Date",
                    ["smalldatetime"] = "Date",
                    ["date"] = "Date",
                    ["time"] = "Time",
                    ["datetimeoffset"] = "Date",
                    ["char"] = "String",
                    ["varchar"] = "String",
                    ["nchar"] = "String",
                    ["nvarchar"] = "String",
                    ["text"] = "String",
                    ["ntext"] = "String",
                    ["xml"] = "String",
                    ["uniqueidentifier"] = "UUID",
                    ["binary"] = "byte[]",
                    ["varbinary"] = "byte[]",
                    ["image"] = "byte[]",
                    ["rowversion"] = "byte[]",
                    ["timestamp"] = "byte[]"
                },
                ["python"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["int"] = "int",
                    ["bigint"] = "int",
                    ["smallint"] = "int",
                    ["tinyint"] = "int",
                    ["bit"] = "bool",
                    ["decimal"] = "Decimal",
                    ["numeric"] = "Decimal",
                    ["money"] = "Decimal",
                    ["smallmoney"] = "Decimal",
                    ["float"] = "float",
                    ["real"] = "float",
                    ["datetime"] = "datetime",
                    ["datetime2"] = "datetime",
                    ["smalldatetime"] = "datetime",
                    ["date"] = "date",
                    ["time"] = "time",
                    ["datetimeoffset"] = "datetime",
                    ["char"] = "str",
                    ["varchar"] = "str",
                    ["nchar"] = "str",
                    ["nvarchar"] = "str",
                    ["text"] = "str",
                    ["ntext"] = "str",
                    ["xml"] = "str",
                    ["uniqueidentifier"] = "UUID",
                    ["binary"] = "bytes",
                    ["varbinary"] = "bytes",
                    ["image"] = "bytes",
                    ["rowversion"] = "bytes",
                    ["timestamp"] = "bytes"
                }
            };
        }

        /// <summary>
        /// Converts a SQL data type to a target language type
        /// </summary>
        /// <param name="sqlType">The SQL data type to convert</param>
        /// <param name="targetLanguage">The target language (csharp, typescript, java, python)</param>
        /// <param name="nullable">Whether the type should be nullable</param>
        /// <returns>The converted type in the target language</returns>
        public string Convert(string sqlType, string targetLanguage, bool nullable = false)
        {
            if (string.IsNullOrEmpty(sqlType) || string.IsNullOrEmpty(targetLanguage))
                return sqlType;

            if (!_typeMap.TryGetValue(targetLanguage.ToLowerInvariant(), out var languageMap))
                throw new ArgumentException($"Unsupported target language: {targetLanguage}");

            // Extract base type from SQL type (e.g., nvarchar(50) -> nvarchar)
            string baseType = ExtractBaseType(sqlType);

            if (!languageMap.TryGetValue(baseType, out var targetType))
                return sqlType; // Return original if no mapping found

            // Handle nullable types based on language
            if (nullable)
            {
                switch (targetLanguage.ToLowerInvariant())
                {
                    case "csharp":
                        // Add ? suffix for all types in C# when nullable is true
                        return targetType + "?";
                    case "typescript":
                        return targetType + " | null";
                    case "java":
                        // Java uses wrapper types which are already nullable
                        break;
                    case "python":
                        // Python 3.10+ supports type hints with Optional
                        return $"Optional[{targetType}]";
                }
            }

            return targetType;
        }

        private string ExtractBaseType(string sqlType)
        {
            // Use regex to extract the base type without size or precision
            var match = Regex.Match(sqlType, @"^([a-zA-Z]+)");
            return match.Success ? match.Groups[1].Value : sqlType;
        }

        /// <summary>
        /// Registers the ConvertTypeEz helper with Handlebars
        /// </summary>
        /// <param name="context">The Handlebars context</param>
        public static void RegisterHelper(IHandlebars context)
        {
            var convertTypeEz = new ConvertTypeEz();

            // Register the main helper
            context.RegisterHelper("ConvertTypeEz", (writer, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: ConvertTypeEz requires at least 2 arguments");
                    return;
                }

                string sqlType = arguments[0]?.ToString() ?? string.Empty;
                string targetLanguage = arguments[1]?.ToString() ?? string.Empty;
                bool nullable = arguments.Length > 2 && System.Convert.ToBoolean(arguments[2]);

                string result = convertTypeEz.Convert(sqlType, targetLanguage, nullable);
                writer.WriteSafeString(result);
            });

            // Register a shorter alias
            context.RegisterHelper("convertType", (writer, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: convertType requires at least 2 arguments");
                    return;
                }

                string sqlType = arguments[0]?.ToString() ?? string.Empty;
                string targetLanguage = arguments[1]?.ToString() ?? string.Empty;
                bool nullable = arguments.Length > 2 && System.Convert.ToBoolean(arguments[2]);

                string result = convertTypeEz.Convert(sqlType, targetLanguage, nullable);
                writer.WriteSafeString(result);
            });
        }
    }
}
