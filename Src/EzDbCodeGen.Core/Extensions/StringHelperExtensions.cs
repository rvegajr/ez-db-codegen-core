using System;
using System.Collections.Generic;
using System.Linq;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for string operations.
    /// </summary>
    public static class StringHelperExtensions
    {
        /// <summary>
        /// Checks if a string is in a collection of strings.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="collection">The collection of strings to check against.</param>
        /// <returns>True if the string is in the collection, false otherwise.</returns>
        public static bool IsIn(this string source, params string[] collection)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return collection != null && collection.Contains(source, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a string to a C# object name.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A string formatted as a C# object name.</returns>
        public static string ToCsObjectName(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove any non-alphanumeric characters
            string result = new string(input.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

            // Ensure it starts with a letter or underscore
            if (result.Length > 0 && !char.IsLetter(result[0]) && result[0] != '_')
                result = "_" + result;

            return result;
        }
    }
}
