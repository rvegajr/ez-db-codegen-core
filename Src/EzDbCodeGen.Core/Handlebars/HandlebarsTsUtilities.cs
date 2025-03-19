using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HandlebarsDotNet;
using EzDbSchema.Core.Extentions;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Core.Handlebars
{
    public static class HandlebarsTsUtilities
    {
        public static void RegisterTsHelpers(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("tsType", (writer, context, parameters) =>
            {
                if (context.Value is null) 
                {
                    writer.WriteSafeString("any");
                    return;
                }
                writer.WriteSafeString(ConvertToTypeScriptType(parameters.Length > 0 ? parameters[0]?.ToString() : context.Value.ToString()));
            });

            handlebars.RegisterHelper("tsInterface", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)));
            });

            handlebars.RegisterHelper("tsModel", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Model");
            });

            handlebars.RegisterHelper("tsService", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Service");
            });

            handlebars.RegisterHelper("tsComponent", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Component");
            });

            handlebars.RegisterHelper("tsModule", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Module");
            });

            handlebars.RegisterHelper("tsRouting", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Routing");
            });

            handlebars.RegisterHelper("tsStore", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Store");
            });

            handlebars.RegisterHelper("tsState", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "State");
            });

            handlebars.RegisterHelper("tsActions", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Actions");
            });

            handlebars.RegisterHelper("tsMutations", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Mutations");
            });

            handlebars.RegisterHelper("tsGetters", (writer, context, parameters) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                var name = context.Value.ToString() ?? string.Empty;
                writer.WriteSafeString(ToTitleCase(
                    EzDbSchema.Core.Extentions.StringExtensions.ToSingular(name)) + "Getters");
            });
        }

        private static string ConvertToTypeScriptType(string? type)
        {
            return type?.ToLower() switch
            {
                "int" => "number",
                "int32" => "number",
                "int64" => "number",
                "long" => "number",
                "double" => "number",
                "decimal" => "number",
                "float" => "number",
                "bool" => "boolean",
                "boolean" => "boolean",
                "datetime" => "Date",
                "date" => "Date",
                "string" => "string",
                "guid" => "string",
                _ => "any"
            };
        }
        
        // Helper method to convert a string to title case
        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
                
            // Use TextInfo to properly handle title casing
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }
    }
}