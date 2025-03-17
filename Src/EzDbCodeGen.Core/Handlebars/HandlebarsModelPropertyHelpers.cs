using HandlebarsDotNet;
using System.Diagnostics;

namespace EzDbCodeGen.Core.Handlebars
{
    internal static class HandlebarsModelPropertyHelpers
    {
        internal static void RegisterHelpers(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("POCOModelPropertyAttributes", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelPropertyAttributes')";
                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var property = (EzDbSchema.Core.Interfaces.IProperty)context.Value;
                    var parentEntity = property.ParentEntity;
                    var propertyName = property.PropertyName ?? string.Empty;
                    var columnAlias = property.ColumnName ?? string.Empty;
                    var primaryKeyOrder = property.PrimaryKeyOrder;
                    var database = parentEntity?.ParentDatabase;
                    var entityName = parentEntity?.TableName ?? string.Empty;
                    var decimalAttribute = "";
                    var keyAttribute = "";
                    var fkAttributes = "";
                    var identityAttribute = "";
                    var columnAttribute = "";

                    if (property.IsIdentity)
                    {
                        identityAttribute = ", DatabaseGenerated(DatabaseGeneratedOption.Identity)";
                    }

                    if (property.IsPrimaryKey)
                    {
                        if (parentEntity?.PrimaryKeys.Count > 1)
                        {
                            keyAttribute = $@"[Key, Column(""{property.PropertyName}"", Order={property.PrimaryKeyOrder}){identityAttribute}]";
                            columnAttribute = "";  // Clear the column attribute since we have already added it here
                        }
                        else
                        {
                            keyAttribute = $"[Key{identityAttribute}]";
                        }
                    }

                    if (property.RelatedTo?.Any() ?? false)
                    {
                        foreach (var relationship in property.RelatedTo)
                        {
                            fkAttributes += $@"[ForeignKey(""{relationship.FromPropertyName}"")]";
                        }
                    }

                    if (!string.IsNullOrEmpty(columnAttribute))
                    {
                        columnAttribute = $@"[Column(""{property.PropertyName}"")]";
                    }

                    if (property.DataType == "decimal")
                    {
                        var precision = property.Precision > 0 ? property.Precision : 18;
                        var scale = property.Scale > 0 ? property.Scale : 0;
                        decimalAttribute = $"[Column(TypeName = \"decimal({precision},{scale})\")]";
                    }

                    writer.WriteSafeString($"{prefix}{keyAttribute}{columnAttribute}{decimalAttribute}{fkAttributes}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            handlebars.RegisterHelper("PropertyAsObjectName", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('PropertyAsObjectName')";
                try
                {
                    var property = (IProperty)context.Value;
                    writer.WriteSafeString(property.PropertyName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            handlebars.RegisterHelper("GetDefaultValue", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('GetDefaultValue')";
                try
                {
                    var property = (IProperty)context.Value;
                    var defaultValue = property.DefaultValue?.ToString() ?? "";
                    writer.WriteSafeString(defaultValue);
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
