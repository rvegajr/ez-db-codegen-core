using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides utility methods for Handlebars templates, including helper registration.
    /// </summary>
    public static class HandlebarsUtility
    {
        private static string? _currentSwitchValue;
        private static bool _currentSwitchMatched;

        /// <summary>
        /// Compares two values for equality or comparison.
        /// </summary>
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
                    return value1.ToString().CompareTo(value2.ToString());
                }
            }

            return value1.ToString().CompareTo(value2.ToString());
        }

        /// <summary>
        /// Registers all Handlebars helpers for code generation.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register helpers with.</param>
        public static void RegisterHelpers(IHandlebars? handlebars = null)
        {
            handlebars ??= HandlebarsDotNet.Handlebars.Create();
            
            RegisterBasicHelpers(handlebars);
            RegisterStringFormatHelpers(handlebars);
            RegisterTypeConversionHelpers(handlebars);
            RegisterContextHelpers(handlebars);
            RegisterComparisonHelpers(handlebars);
            
            // Register other helper categories
            HandlebarsForeignKeyHelpers.RegisterHelpers(handlebars);
            HandlebarsModelPropertyHelpers.RegisterHelpers(handlebars);
            HandlebarsSchemaHelpers.RegisterHelpers(handlebars);
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
                    var formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(parameters[0], formatting);
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
                if (context.Value is null) 
                {
                    writer.WriteSafeString("object");
                    return;
                }
                string dbType = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(dbType.ToNetType());
            });

            // Convert database type to JavaScript type
            handlebars.RegisterHelper("ToJsType", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString("Object");
                    return;
                }
                string dbType = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(dbType.ToJsType());
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
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string input = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(input.ToPascalCase());
            });

            // Convert to camelCase
            handlebars.RegisterHelper("ToCamelCase", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string input = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(input.ToCamelCase());
            });

            // Convert to snake_case
            handlebars.RegisterHelper("ToSnakeCase", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string input = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(input.ToSnakeCase());
            });

            // Convert to singular form
            handlebars.RegisterHelper("ToSingular", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string input = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(input.ToSingular());
            });

            // Convert to plural form
            handlebars.RegisterHelper("ToPlural", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string input = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(input.ToPlural());
            });

            // Convert to code-friendly format
            handlebars.RegisterHelper("ToCodeFriendly", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string input = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(input.ToCodeFriendly());
            });

            // Extract table name from schema.table format
            handlebars.RegisterHelper("ExtractTableName", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string schemaTableName = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(schemaTableName.ExtractTableName());
            });

            // Extract schema name from schema.table format
            handlebars.RegisterHelper("ExtractSchemaName", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string schemaTableName = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(schemaTableName.ExtractSchemaName());
            });

            // Format name by removing common suffixes
            handlebars.RegisterHelper("AsFormattedName", (writer, context, parameters) => {
                if (context.Value is null) 
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                string name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(name.AsFormattedName());
            });

            // Add prefix to string
            handlebars.RegisterHelper("Prefix", (writer, context, parameters) => {
                if (parameters.Length < 2) return;
                
                var prefix = parameters[0]?.ToString() ?? "";
                var value = parameters[1]?.ToString() ?? "";
                
                writer.WriteSafeString($"{prefix}{value}");
            });

            // Apply multiple string formats
            handlebars.RegisterHelper("StringFormat", (writer, context, parameters) => {
                if (parameters.Length < 2) return;
                
                var input = parameters[0]?.ToString();
                if (string.IsNullOrEmpty(input)) return;
                
                var result = input;
                
                for (int i = 1; i < parameters.Length; i++)
                {
                    var format = parameters[i]?.ToString();
                    if (string.IsNullOrEmpty(format)) continue;
                    
                    result = format.ToLower() switch
                    {
                        "pascal" => result.ToPascalCase(),
                        "camel" => result.ToCamelCase(),
                        "snake" => result.ToSnakeCase(),
                        "singular" => result.ToSingular(),
                        "plural" => result.ToPlural(),
                        "upper" => result.ToUpper(),
                        "lower" => result.ToLower(),
                        "codefriendly" => result.ToCodeFriendly(),
                        "tablename" => result.ExtractTableName(),
                        "schemaname" => result.ExtractSchemaName(),
                        "formatted" => result.AsFormattedName(),
                        _ => result
                    };
                }
                
                writer.WriteSafeString(result);
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
                    var formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;
                    var settings = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    };
                    
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(context, formatting, settings);
                    writer.WriteSafeString(json);
                }
                catch (Exception ex)
                {
                    writer.WriteSafeString($"Error serializing context: {ex.Message}");
                }
            });

            // Check if property exists in context
            handlebars.RegisterHelper("IfPropertyExists", (writer, options, context, parameters) => {
                if (parameters.Length < 1) return;
                
                var propertyName = parameters[0]?.ToString();
                if (string.IsNullOrEmpty(propertyName)) return;
                
                bool exists = false;
                
                try
                {
                    var contextType = context.GetType();
                    var property = contextType.GetProperty(propertyName);
                    exists = property != null;
                }
                catch
                {
                    exists = false;
                }
                
                if (exists)
                {
                    options.Template(writer, context);
                }
                else
                {
                    options.Inverse(writer, context);
                }
            });

            // Check if function exists
            handlebars.RegisterHelper("IfFunctionExists", (writer, options, context, parameters) => {
                if (parameters.Length < 1) return;
                
                var functionName = parameters[0]?.ToString();
                if (string.IsNullOrEmpty(functionName)) return;
                
                bool exists = false;
                
                try
                {
                    var contextType = context.GetType();
                    var method = contextType.GetMethod(functionName);
                    exists = method != null;
                }
                catch
                {
                    exists = false;
                }
                
                if (exists)
                {
                    options.Template(writer, context);
                }
                else
                {
                    options.Inverse(writer, context);
                }
            });
        }

        /// <summary>
        /// Registers comparison helpers.
        /// </summary>
        private static void RegisterComparisonHelpers(IHandlebars handlebars)
        {
            // If condition helper
            handlebars.RegisterHelper("IfCond", (writer, options, context, parameters) => {
                if (parameters.Length < 3) return;
                
                var left = parameters[0];
                var op = parameters[1]?.ToString();
                var right = parameters[2];
                
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
                    options.Template(writer, context);
                }
                else
                {
                    options.Inverse(writer, context);
                }
            });

            // Switch/case helper
            handlebars.RegisterHelper("Switch", (writer, options, context, parameters) => {
                if (parameters.Length < 1) return;
                
                _currentSwitchValue = parameters[0]?.ToString();
                _currentSwitchMatched = false;
                
                options.Template(writer, context);
            });

            handlebars.RegisterHelper("Case", (writer, options, context, parameters) => {
                if (_currentSwitchValue == null || _currentSwitchMatched) return;
                
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i]?.ToString() == _currentSwitchValue)
                    {
                        _currentSwitchMatched = true;
                        options.Template(writer, context);
                        break;
                    }
                }
            });

            handlebars.RegisterHelper("Default", (writer, options, context, parameters) => {
                if (_currentSwitchValue == null || _currentSwitchMatched) return;
                
                options.Template(writer, context);
            });
        }
    }
}
