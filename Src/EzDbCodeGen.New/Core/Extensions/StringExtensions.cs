using System.Globalization;
using System.Text.RegularExpressions;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for string manipulation commonly used in code generation.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to a code-friendly format by removing special characters and spaces.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A code-friendly string.</returns>
        public static string ToCodeFriendly(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Replace spaces and special characters with underscores
            var result = Regex.Replace(input, @"[^\w]", "_");
            
            // Ensure it starts with a letter
            if (char.IsDigit(result[0]))
                result = "_" + result;
                
            return result;
        }

        /// <summary>
        /// Converts a string to PascalCase.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A PascalCase string.</returns>
        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Split by non-alphanumeric characters
            var words = Regex.Split(input, @"[^a-zA-Z0-9]")
                .Where(w => !string.IsNullOrEmpty(w))
                .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower())
                .ToArray();

            return string.Join("", words);
        }

        /// <summary>
        /// Converts a string to camelCase.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A camelCase string.</returns>
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var pascal = input.ToPascalCase();
            return char.ToLower(pascal[0]) + pascal.Substring(1);
        }

        /// <summary>
        /// Converts a string to snake_case.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A snake_case string.</returns>
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Insert underscore before uppercase letters
            var result = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2");
            
            // Replace non-alphanumeric with underscore
            result = Regex.Replace(result, @"[^a-zA-Z0-9]", "_");
            
            return result.ToLower();
        }

        /// <summary>
        /// Converts a string to singular form.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A singular form of the string.</returns>
        public static string ToSingular(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Use EzDbSchema's StringExtensions for pluralization
            return CoreExtentions.StringExtensions.ToSingular(input);
        }

        /// <summary>
        /// Converts a string to plural form.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>A plural form of the string.</returns>
        public static string ToPlural(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Use EzDbSchema's StringExtensions for pluralization
            return CoreExtentions.StringExtensions.ToPlural(input);
        }

        /// <summary>
        /// Extracts the table name from a schema.table format.
        /// </summary>
        /// <param name="schemaTableName">The schema.table string.</param>
        /// <returns>The table name without schema.</returns>
        public static string ExtractTableName(this string schemaTableName)
        {
            if (string.IsNullOrEmpty(schemaTableName))
                return schemaTableName;

            var parts = schemaTableName.Split('.');
            return parts.Length > 1 ? parts[1] : schemaTableName;
        }

        /// <summary>
        /// Extracts the schema name from a schema.table format.
        /// </summary>
        /// <param name="schemaTableName">The schema.table string.</param>
        /// <returns>The schema name.</returns>
        public static string ExtractSchemaName(this string schemaTableName)
        {
            if (string.IsNullOrEmpty(schemaTableName))
                return string.Empty;

            var parts = schemaTableName.Split('.');
            return parts.Length > 1 ? parts[0] : string.Empty;
        }

        /// <summary>
        /// Converts a database type to a .NET type.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <returns>The corresponding .NET type.</returns>
        public static string ToNetType(this string dbType)
        {
            if (string.IsNullOrEmpty(dbType))
                return "object";

            return dbType.ToLower() switch
            {
                "bigint" => "long",
                "binary" => "byte[]",
                "bit" => "bool",
                "char" => "string",
                "date" => "DateTime",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "datetimeoffset" => "DateTimeOffset",
                "decimal" => "decimal",
                "float" => "double",
                "image" => "byte[]",
                "int" => "int",
                "money" => "decimal",
                "nchar" => "string",
                "ntext" => "string",
                "numeric" => "decimal",
                "nvarchar" => "string",
                "real" => "float",
                "rowversion" => "byte[]",
                "smalldatetime" => "DateTime",
                "smallint" => "short",
                "smallmoney" => "decimal",
                "text" => "string",
                "time" => "TimeSpan",
                "timestamp" => "byte[]",
                "tinyint" => "byte",
                "uniqueidentifier" => "Guid",
                "varbinary" => "byte[]",
                "varchar" => "string",
                "xml" => "string",
                _ => "object"
            };
        }

        /// <summary>
        /// Converts a database type to a JavaScript type.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <returns>The corresponding JavaScript type.</returns>
        public static string ToJsType(this string dbType)
        {
            if (string.IsNullOrEmpty(dbType))
                return "any";

            return dbType.ToLower() switch
            {
                "bigint" => "number",
                "binary" => "Uint8Array",
                "bit" => "boolean",
                "char" => "string",
                "date" => "Date",
                "datetime" => "Date",
                "datetime2" => "Date",
                "datetimeoffset" => "Date",
                "decimal" => "number",
                "float" => "number",
                "image" => "Uint8Array",
                "int" => "number",
                "money" => "number",
                "nchar" => "string",
                "ntext" => "string",
                "numeric" => "number",
                "nvarchar" => "string",
                "real" => "number",
                "rowversion" => "Uint8Array",
                "smalldatetime" => "Date",
                "smallint" => "number",
                "smallmoney" => "number",
                "text" => "string",
                "time" => "string",
                "timestamp" => "Uint8Array",
                "tinyint" => "number",
                "uniqueidentifier" => "string",
                "varbinary" => "Uint8Array",
                "varchar" => "string",
                "xml" => "string",
                _ => "any"
            };
        }

        /// <summary>
        /// Formats a name by removing common suffixes like 'Id'.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <returns>The formatted name.</returns>
        public static string AsFormattedName(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // Remove Id suffix
            if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                return name.Substring(0, name.Length - 2);

            return name;
        }
    }
}
