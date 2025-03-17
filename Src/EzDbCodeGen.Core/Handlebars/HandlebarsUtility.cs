using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using HandlebarsDotNet.IO;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using EzDbCodeGen.Core.Extensions;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Handlebars
{
    public static class HandlebarsUtility
    {
        private static string? _currentSwitchValue;
        private static bool _currentSwitchMatched;

        private static int CompareValues(object value1, object value2)
        {
            if (value1 == null && value2 == null) return 0;
            if (value1 == null) return -1;
            if (value2 == null) return 1;

            if (value1 is IComparable comparable1 && value2 is IComparable comparable2)
            {
                try
                {
                    // Try to convert types if they don't match
                    if (value1.GetType() != value2.GetType())
                    {
                        if (double.TryParse(value1.ToString(), out double d1) && 
                            double.TryParse(value2.ToString(), out double d2))
                            return d1.CompareTo(d2);

                        if (DateTime.TryParse(value1.ToString(), out DateTime dt1) && 
                            DateTime.TryParse(value2.ToString(), out DateTime dt2))
                            return dt1.CompareTo(dt2);
                    }

                    return comparable1.CompareTo(comparable2);
                }
                catch
                {
                    // If comparison fails, fall back to string comparison
                    return (value1?.ToString() ?? string.Empty).CompareTo(value2?.ToString() ?? string.Empty);
                }
            }

            return (value1?.ToString() ?? string.Empty).CompareTo(value2?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Registers all Handlebars helpers for code generation.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register helpers with.</param>
        public static void RegisterHelpers(IHandlebars? handlebars = null)
        {
            if (handlebars == null)
                handlebars = HandlebarsDotNet.Handlebars.Create();
            
            RegisterBasicHelpers(handlebars);
            RegisterStringFormatHelpers(handlebars);
            RegisterTypeConversionHelpers(handlebars);
            RegisterContextHelpers(handlebars);
            RegisterComparisonHelpers(handlebars);
        }

        /// <summary>
        /// Registers basic helpers for debugging and utility functions.
        /// </summary>
        private static void RegisterBasicHelpers(IHandlebars handlebars)
        {
            // Debugging helper
            handlebars.RegisterHelper("Debugger", (writer, context, parameters) => {
                if (parameters.Length > 0)
                {
                    var prefix = parameters[0]?.ToString() ?? "";
                    writer.WriteSafeString(prefix);
                }
                // In a real debugger, you might add a breakpoint here
            });

            // Simple comma helper
            handlebars.RegisterHelper("Comma", (writer, context, parameters) => {
                writer.WriteSafeString(",");
            });

            // JSON serialization helper
            handlebars.RegisterHelper("ToJson", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var indented = parameters.Length > 1 && parameters[1] is bool b && b;
                    var formatting = indented ? Formatting.Indented : Formatting.None;
                    var json = JsonConvert.SerializeObject(parameters[0], formatting);
                    writer.WriteSafeString(json);
                }
            });
        }

        /// <summary>
        /// Registers type conversion helpers.
        /// </summary>
        private static void RegisterTypeConversionHelpers(IHandlebars handlebars)
        {
            // Convert database type to .NET type
            handlebars.RegisterHelper("ToNetType", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var dbType = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(dbType.ToNetType());
                }
            });

            // Convert to nullable type if needed
            handlebars.RegisterHelper("AsNullableType", (writer, context, parameters) => {
                if (parameters.Length < 2) return;
                
                var typeName = parameters[0]?.ToString();
                var isNullable = parameters[1] is bool nullable && nullable;
                
                if (string.IsNullOrEmpty(typeName)) return;
                
                // Value types that can be nullable
                var valueTypes = new[] { "int", "long", "short", "byte", "bool", "float", "double", "decimal", "DateTime", "TimeSpan", "Guid" };
                
                if (isNullable && valueTypes.Contains(typeName))
                {
                    writer.WriteSafeString($"{typeName}?");
                }
                else
                {
                    writer.WriteSafeString(typeName);
                }
            });
        }

        /// <summary>
        /// Registers string formatting helpers.
        /// </summary>
        private static void RegisterStringFormatHelpers(IHandlebars handlebars)
        {
            // Convert to PascalCase
            handlebars.RegisterHelper("ToPascalCase", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var input = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(input.ToPascalCase());
                }
            });

            // Convert to camelCase
            handlebars.RegisterHelper("ToCamelCase", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var input = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(input.ToCamelCase());
                }
            });

            // Convert to snake_case
            handlebars.RegisterHelper("ToSnakeCase", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var input = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(input.ToSnakeCase());
                }
            });

            // Extract table name from schema.table format
            handlebars.RegisterHelper("ExtractTableName", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var input = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(input.ExtractTableName());
                }
            });

            // Extract schema name from schema.table format
            handlebars.RegisterHelper("ExtractSchemaName", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var input = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(input.ExtractSchemaName());
                }
            });

            // Format name by removing common suffixes
            handlebars.RegisterHelper("AsFormattedName", (writer, context, parameters) => {
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    var input = parameters[0]?.ToString() ?? string.Empty;
                    writer.WriteSafeString(input.AsFormattedName());
                }
            });
        }

        /// <summary>
        /// Registers context-related helpers.
        /// </summary>
        private static void RegisterContextHelpers(IHandlebars handlebars)
        {
            // Serialize context as JSON
            handlebars.RegisterHelper("ContextAsJson", (writer, context, parameters) => {
                try
                {
                    var indented = parameters.Length > 0 && parameters[0] is bool b && b;
                    var formatting = indented ? Formatting.Indented : Formatting.None;
                    var settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    
                    var json = JsonConvert.SerializeObject(context, formatting, settings);
                    writer.WriteSafeString(json);
                }
                catch (Exception ex)
                {
                    writer.WriteSafeString($"Error serializing context: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Registers comparison helpers.
        /// </summary>
        private static void RegisterComparisonHelpers(IHandlebars handlebars)
        {
            // If condition helper
            handlebars.RegisterHelper("IfCond", (output, options, context, arguments) => {
                if (arguments.Length < 3) return;
                
                var left = arguments[0];
                var op = arguments[1]?.ToString();
                var right = arguments[2];
                bool condition = false;
                
                switch (op)
                {
                    case "==":
                        condition = CompareValues(left, right) == 0;
                        break;
                    case "===":
                        condition = left?.ToString() == right?.ToString();
                        break;
                    case "!=":
                        condition = CompareValues(left, right) != 0;
                        break;
                    case "!==":
                        condition = left?.ToString() != right?.ToString();
                        break;
                    case "<":
                        condition = CompareValues(left, right) < 0;
                        break;
                    case "<=":
                        condition = CompareValues(left, right) <= 0;
                        break;
                    case ">":
                        condition = CompareValues(left, right) > 0;
                        break;
                    case ">=":
                        condition = CompareValues(left, right) >= 0;
                        break;
                    case "&&":
                        condition = left != null && right != null;
                        break;
                    case "||":
                        condition = left != null || right != null;
                        break;
                }
                
                if (condition)
                {
                    options.Template(output, context);
                }
                else
                {
                    options.Inverse(output, context);
                }
            });

            // Switch/case helper
            handlebars.RegisterHelper("Switch", (output, options, context, arguments) => {
                if (arguments.Length < 1) return;
                
                _currentSwitchValue = arguments[0]?.ToString();
                _currentSwitchMatched = false;
                
                options.Template(output, context);
            });

            handlebars.RegisterHelper("Case", (output, options, context, arguments) => {
                if (_currentSwitchValue == null || _currentSwitchMatched) return;
                
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i]?.ToString() == _currentSwitchValue)
                    {
                        _currentSwitchMatched = true;
                        options.Template(output, context);
                        break;
                    }
                }
            });

            handlebars.RegisterHelper("Default", (output, options, context, arguments) => {
                if (_currentSwitchValue == null || _currentSwitchMatched) return;
                
                options.Template(output, context);
            });
        }
    }
}