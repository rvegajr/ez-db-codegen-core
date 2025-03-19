using HandlebarsDotNet;
using System.Diagnostics;

namespace EzDbCodeGen.Core.Handlebars
{
    internal static class HandlebarsAuditHelpers
    {
        internal static void RegisterHelpers(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("IsAuditableOutput", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('IsAuditableOutput')";
                try
                {
                    var outputText = parameters[0]?.ToString() ?? string.Empty;
                    var entity = (IEntity)context.Value;
                    writer.WriteSafeString(entity.IsAuditable() ? outputText : "");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            handlebars.RegisterHelper("IsNotAuditableOutput", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('IsNotAuditableOutput')";
                try
                {
                    var outputText = parameters[0]?.ToString() ?? string.Empty;
                    var entity = (IEntity)context.Value;
                    writer.WriteSafeString(!entity.IsAuditable() ? outputText : "");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
