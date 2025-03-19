using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides string formatting capabilities for Handlebars templates
    /// </summary>
    public class FormatEz
    {
        /// <summary>
        /// Formats a string using specified operations
        /// </summary>
        /// <param name="operations">Comma or pipe-separated list of operations to apply</param>
        /// <param name="input">Input string to format</param>
        /// <returns>Formatted string</returns>
        public string Format(string operations, string input)
        {
            if (string.IsNullOrEmpty(operations) || string.IsNullOrEmpty(input))
                return input;

            string result = input;
            string[] operationList = operations.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var operation in operationList)
            {
                result = ApplyOperation(operation.Trim(), result);
            }

            return result;
        }

        private string ApplyOperation(string operation, string input)
        {
            // Case transformations
            if (operation.Equals("camelCase", StringComparison.OrdinalIgnoreCase))
                return ToCamelCase(input);
            if (operation.Equals("pascalCase", StringComparison.OrdinalIgnoreCase))
                return ToPascalCase(input);
            if (operation.Equals("snakeCase", StringComparison.OrdinalIgnoreCase))
                return ToSnakeCase(input);
            if (operation.Equals("kebabCase", StringComparison.OrdinalIgnoreCase))
                return ToKebabCase(input);
            if (operation.Equals("lowercase", StringComparison.OrdinalIgnoreCase))
                return input.ToLowerInvariant();
            if (operation.Equals("uppercase", StringComparison.OrdinalIgnoreCase))
            {
                // Simply preserve all characters and convert to uppercase
                return input.ToUpperInvariant();
            }

            // Indentation
            if (operation.Equals("indent", StringComparison.OrdinalIgnoreCase))
                return IndentText(input, 4);
            if (operation.StartsWith("indent", StringComparison.OrdinalIgnoreCase) && 
                int.TryParse(operation.Substring(6), out int indentSize))
                return IndentText(input, indentSize);

            // Tabbing
            if (operation.Equals("tab", StringComparison.OrdinalIgnoreCase))
                return TabText(input, 1);
            if (operation.StartsWith("tab", StringComparison.OrdinalIgnoreCase) && 
                int.TryParse(operation.Substring(3), out int tabCount))
                return TabText(input, tabCount);

            // Trimming
            if (operation.Equals("trim", StringComparison.OrdinalIgnoreCase))
                return input.Trim();
            if (operation.Equals("trimStart", StringComparison.OrdinalIgnoreCase))
                return input.TrimStart();
            if (operation.Equals("trimEnd", StringComparison.OrdinalIgnoreCase))
                return input.TrimEnd();

            // If operation not recognized, return input unchanged
            return input;
        }

        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Handle snake_case or kebab-case
            if (input.Contains('_') || input.Contains('-'))
            {
                string[] parts = input.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return input;

                StringBuilder result = new StringBuilder(parts[0].ToLowerInvariant());
                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i].Length > 0)
                        result.Append(char.ToUpperInvariant(parts[i][0]) + (parts[i].Length > 1 ? parts[i].Substring(1).ToLowerInvariant() : ""));
                }
                return result.ToString();
            }

            // Handle PascalCase
            if (input.Length > 0 && char.IsUpper(input[0]))
                return char.ToLowerInvariant(input[0]) + input.Substring(1);

            return input;
        }

        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Handle snake_case or kebab-case
            if (input.Contains('_') || input.Contains('-'))
            {
                string[] parts = input.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder result = new StringBuilder();
                foreach (var part in parts)
                {
                    if (part.Length > 0)
                        result.Append(char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part.Substring(1).ToLowerInvariant() : ""));
                }
                return result.ToString();
            }

            // Handle camelCase
            if (input.Length > 0 && char.IsLower(input[0]))
                return char.ToUpperInvariant(input[0]) + input.Substring(1);

            return input;
        }

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Handle kebab-case
            if (input.Contains('-'))
                input = input.Replace('-', '_');

            // Handle PascalCase or camelCase
            if (!input.Contains('_'))
            {
                var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2");
                return result.ToLowerInvariant();
            }

            return input.ToLowerInvariant();
        }

        private string ToKebabCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Handle snake_case
            if (input.Contains('_'))
                input = input.Replace('_', '-');

            // Handle PascalCase or camelCase
            if (!input.Contains('-'))
            {
                var result = Regex.Replace(input, "([a-z0-9])([A-Z])", "$1-$2");
                return result.ToLowerInvariant();
            }

            return input.ToLowerInvariant();
        }

        private string IndentText(string input, int spaces)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string indent = new string(' ', spaces);
            return indent + input.Replace("\n", "\n" + indent);
        }

        private string TabText(string input, int tabCount)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string tabs = new string('\t', tabCount);
            return tabs + input.Replace("\n", "\n" + tabs);
        }

        /// <summary>
        /// Registers the FormatEz helper with Handlebars
        /// </summary>
        /// <param name="context">The Handlebars context</param>
        public static void RegisterHelper(IHandlebars context)
        {
            var formatEz = new FormatEz();

            context.RegisterHelper("FormatEz", (writer, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: FormatEz requires at least 2 arguments");
                    return;
                }

                string operations = arguments[0]?.ToString() ?? string.Empty;
                string input = arguments[1]?.ToString() ?? string.Empty;

                string result = formatEz.Format(operations, input);
                writer.WriteSafeString(result);
            });

            // Register a shorter alias
            context.RegisterHelper("format", (writer, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: format requires at least 2 arguments");
                    return;
                }

                string operations = arguments[0]?.ToString() ?? string.Empty;
                string input = arguments[1]?.ToString() ?? string.Empty;

                string result = formatEz.Format(operations, input);
                writer.WriteSafeString(result);
            });
        }
    }
}
