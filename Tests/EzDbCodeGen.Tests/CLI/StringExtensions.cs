using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for string manipulation
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to a code-friendly format (snake_case)
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The code-friendly string</returns>
        public static string ToCodeFriendly(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Convert to snake_case
            return ToSnakeCase(input);
        }

        /// <summary>
        /// Converts a string to PascalCase
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The PascalCase string</returns>
        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Split by underscores or case changes
            var words = SplitIntoWords(input);

            // Capitalize first letter of each word
            return string.Join("", words.Select(word => 
                word.Length > 0 ? char.ToUpper(word[0]) + word.Substring(1).ToLower() : ""));
        }

        /// <summary>
        /// Converts a string to camelCase
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The camelCase string</returns>
        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Handle numeric start
            if (input.Length > 0 && char.IsDigit(input[0]))
                return "_" + input;

            // Split by underscores or case changes
            var words = SplitIntoWords(input);

            if (words.Length == 0)
                return string.Empty;

            // First word lowercase, rest capitalized
            return words[0].ToLower() + 
                   string.Join("", words.Skip(1).Select(word => 
                       word.Length > 0 ? char.ToUpper(word[0]) + word.Substring(1).ToLower() : ""));
        }

        /// <summary>
        /// Converts a string to snake_case
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The snake_case string</returns>
        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Handle already snake_case input (like USER_NAME)
            if (input.Contains('_'))
            {
                return input.ToLower();
            }

            // Insert spaces before capital letters (for camelCase or PascalCase)
            string result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
            
            // Convert to lowercase
            return result.ToLower();
        }

        private static string[] SplitIntoWords(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<string>();

            // Replace underscores with spaces
            input = input.Replace("_", " ");

            // Insert spaces before capital letters
            input = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1 $2");

            // Split by spaces
            return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
