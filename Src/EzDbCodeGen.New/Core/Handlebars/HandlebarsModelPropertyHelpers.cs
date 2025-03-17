using System.Diagnostics;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides Handlebars helpers for working with model properties.
    /// </summary>
    public static class HandlebarsModelPropertyHelpers
    {
        /// <summary>
        /// Registers all model property related helpers with the Handlebars instance.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register helpers with.</param>
        public static void RegisterHelpers(IHandlebars handlebars)
        {
            // Helper for generating POCO model properties
            handlebars.RegisterHelper("POCOModelProperties", (writer, context, parameters) => {
                if (context.Value is not IEntity entity)
                    return;

                var procName = $"Handlebars.RegisterHelper('POCOModelProperties', Entity='{entity.TableName}')";

                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var includeAnnotations = parameters.Length > 1 && parameters[1] is bool b && b;

                    foreach (var property in entity.Properties)
                    {
                        if (includeAnnotations)
                        {
                            // Generate data annotations
                            if (!property.IsNullable)
                            {
                                writer.WriteSafeString($"\n{prefix}[Required]");
                            }

                            if (property.DataType.Contains("char") && property.MaxLength > 0)
                            {
                                writer.WriteSafeString($"\n{prefix}[MaxLength({property.MaxLength})]");
                            }

                            // Add column attribute
                            writer.WriteSafeString($"\n{prefix}[Column(\"{property.ColumnName}\")]");
                        }

                        // Generate property
                        var typeName = property.DataType.ToNetType();
                        var nullableMark = property.IsNullable && typeName != "string" && !typeName.EndsWith("[]") ? "?" : "";
                        
                        writer.WriteSafeString($"\n{prefix}public {typeName}{nullableMark} {property.PropertyName} {{ get; set; }}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for generating TypeScript model properties
            handlebars.RegisterHelper("TSModelProperties", (writer, context, parameters) => {
                if (context.Value is not IEntity entity)
                    return;

                var procName = $"Handlebars.RegisterHelper('TSModelProperties', Entity='{entity.TableName}')";

                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var includeComments = parameters.Length > 1 && parameters[1] is bool b && b;

                    foreach (var property in entity.Properties)
                    {
                        if (includeComments && !string.IsNullOrEmpty(property.Description))
                        {
                            writer.WriteSafeString($"\n{prefix}/** {property.Description} */");
                        }

                        // Generate property
                        var typeName = property.DataType.ToJsType();
                        var nullableMark = property.IsNullable ? "?" : "";
                        
                        writer.WriteSafeString($"\n{prefix}{property.PropertyName.ToCamelCase()}{nullableMark}: {typeName};");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for generating property with validation attributes
            handlebars.RegisterHelper("PropertyWithValidation", (writer, context, parameters) => {
                if (context.Value is not IProperty property)
                    return;

                var procName = $"Handlebars.RegisterHelper('PropertyWithValidation')";

                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    
                    // Generate validation attributes
                    if (!property.IsNullable)
                    {
                        writer.WriteSafeString($"\n{prefix}[Required]");
                    }

                    if (property.DataType.Contains("char") && property.MaxLength > 0)
                    {
                        writer.WriteSafeString($"\n{prefix}[MaxLength({property.MaxLength})]");
                    }

                    if (property.DataType.Contains("decimal") || property.DataType.Contains("numeric"))
                    {
                        if (property.Precision > 0 && property.Scale > 0)
                        {
                            writer.WriteSafeString($"\n{prefix}[Range(0, {new string('9', property.Precision - property.Scale)}.{new string('9', property.Scale)})]");
                        }
                    }

                    // Add column attribute
                    writer.WriteSafeString($"\n{prefix}[Column(\"{property.ColumnName}\")]");

                    // Generate property
                    var typeName = property.DataType.ToNetType();
                    var nullableMark = property.IsNullable && typeName != "string" && !typeName.EndsWith("[]") ? "?" : "";
                    
                    writer.WriteSafeString($"\n{prefix}public {typeName}{nullableMark} {property.PropertyName} {{ get; set; }}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for generating primary key properties
            handlebars.RegisterHelper("PrimaryKeyProperties", (writer, context, parameters) => {
                if (context.Value is not IEntity entity)
                    return;

                var procName = $"Handlebars.RegisterHelper('PrimaryKeyProperties', Entity='{entity.TableName}')";

                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var separator = parameters.Length > 1 ? parameters[1]?.ToString() ?? ", " : ", ";
                    var includeType = parameters.Length > 2 && parameters[2] is bool b && b;

                    var pkProperties = entity.Properties.Where(p => p.IsPrimaryKey).ToList();
                    
                    for (int i = 0; i < pkProperties.Count; i++)
                    {
                        var property = pkProperties[i];
                        
                        if (includeType)
                        {
                            var typeName = property.DataType.ToNetType();
                            writer.WriteSafeString($"{typeName} {property.PropertyName}");
                        }
                        else
                        {
                            writer.WriteSafeString($"{property.PropertyName}");
                        }
                        
                        if (i < pkProperties.Count - 1)
                        {
                            writer.WriteSafeString(separator);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for checking if an entity has a primary key
            handlebars.RegisterHelper("HasPrimaryKey", (writer, options, context, parameters) => {
                IEntity entity = context.Value switch
                {
                    IEntity ent => ent,
                    _ => null
                };

                if (entity == null)
                    return;

                bool hasPrimaryKey = entity.Properties.Any(p => p.IsPrimaryKey);

                if (hasPrimaryKey)
                {
                    options.Template(writer, context);
                }
                else if (options.Inverse != null)
                {
                    options.Inverse(writer, context);
                }
            });

            // Helper for getting all primary key properties
            handlebars.RegisterHelper("GetPrimaryKeyProperties", (writer, options, context, parameters) => {
                IEntity entity = context.Value switch
                {
                    IEntity ent => ent,
                    _ => null
                };

                if (entity == null)
                    return;

                var pkProperties = entity.Properties.Where(p => p.IsPrimaryKey).ToList();

                foreach (var pkProperty in pkProperties)
                {
                    options.Template(writer, pkProperty);
                }
            });
        }
    }
}
