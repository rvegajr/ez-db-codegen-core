using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Extentions; 
using EzDbSchema.Core.Enums;
using HandlebarsDotNet;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides Handlebars helpers for working with database schema.
    /// </summary>
    public static class HandlebarsSchemaHelpers
    {
        /// <summary>
        /// Registers all schema related helpers with the Handlebars instance.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register helpers with.</param>
        public static void RegisterHelpers(IHandlebars handlebars)
        {
            // Helper for iterating over entities with optional filtering
            handlebars.RegisterHelper("ForEachEntity", (writer, options, context, parameters) => {
                if (context.Value is not IDatabase database)
                    return;

                var procName = $"Handlebars.RegisterHelper('ForEachEntity')";

                try
                {
                    var schemaFilter = parameters.Length > 0 ? parameters[0]?.ToString() : null;
                    var tableFilter = parameters.Length > 1 ? parameters[1]?.ToString() : null;
                    
                    var entities = database.Entities;
                    
                    // Apply schema filter if provided
                    if (!string.IsNullOrEmpty(schemaFilter))
                    {
                        var filteredEntities = entities.Where(e => e.Value.DatabaseSchema.Equals(schemaFilter, StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        
                        // Cast to IEntityDictionary if needed
                        entities = (EzDbSchema.Core.Interfaces.IEntityDictionary)filteredEntities;
                    }
                    
                    // Apply table filter if provided
                    if (!string.IsNullOrEmpty(tableFilter))
                    {
                        var filteredEntities = entities.Where(e => e.Value.TableName.Contains(tableFilter, StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                        
                        // Cast to IEntityDictionary if needed
                        entities = (EzDbSchema.Core.Interfaces.IEntityDictionary)filteredEntities;
                    }

                    foreach (var entity in entities)
                    {
                        options.Template(writer, entity.Value);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for getting database information
            handlebars.RegisterHelper("DatabaseInfo", (writer, context, parameters) => {
                if (context.Value is not IDatabase database)
                    return;

                var procName = $"Handlebars.RegisterHelper('DatabaseInfo')";

                try
                {
                    var propertyName = parameters.Length > 0 ? parameters[0]?.ToString() : null;
                    
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        writer.WriteSafeString(database.Name);
                        return;
                    }

                    switch (propertyName.ToLower())
                    {
                        case "name":
                            writer.WriteSafeString(database.Name);
                            break;
                        case "server":
                            // Check if Server property exists
                            writer.WriteSafeString(database.Name); // Fallback to Name if Server doesn't exist
                            break;
                        case "provider":
                            // Check if Provider property exists
                            writer.WriteSafeString("Unknown"); // Fallback to a default value
                            break;
                        case "entitycount":
                            writer.WriteSafeString(database.Entities.Count.ToString());
                            break;
                        case "schemacount":
                            writer.WriteSafeString(database.Entities.Select(e => e.Value.DatabaseSchema).Distinct().Count().ToString());
                            break;
                        default:
                            writer.WriteSafeString(database.Name);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for getting entity information
            handlebars.RegisterHelper("EntityInfo", (writer, context, parameters) => {
                if (context.Value is not IEntity entity)
                    return;

                var procName = $"Handlebars.RegisterHelper('EntityInfo', Entity='{entity.TableName}')";

                try
                {
                    var propertyName = parameters.Length > 0 ? parameters[0]?.ToString() : null;
                    
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        writer.WriteSafeString(entity.TableName);
                        return;
                    }

                    switch (propertyName.ToLower())
                    {
                        case "name":
                        case "tablename":
                            writer.WriteSafeString(entity.TableName);
                            break;
                        case "schema":
                        case "databaseschema":
                            writer.WriteSafeString(entity.DatabaseSchema);
                            break;
                        case "fullname":
                            writer.WriteSafeString($"{entity.DatabaseSchema}.{entity.TableName}");
                            break;
                        case "propertycount":
                            writer.WriteSafeString(entity.Properties.Count.ToString());
                            break;
                        case "relationshipcount":
                            writer.WriteSafeString(entity.Relationships != null ? entity.Relationships.Count().ToString() : "0");
                            break;
                        case "isprimarykey":
                            var propertyNameToCheck = parameters.Length > 1 ? parameters[1]?.ToString() : null;
                            if (!string.IsNullOrEmpty(propertyNameToCheck) && entity.Properties.ContainsKey(propertyNameToCheck))
                            {
                                writer.WriteSafeString(entity.Properties[propertyNameToCheck].IsPrimaryKey.ToString().ToLower());
                            }
                            else
                            {
                                writer.WriteSafeString("false");
                            }
                            break;
                        default:
                            writer.WriteSafeString(entity.TableName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for iterating over entity properties
            handlebars.RegisterHelper("ForEachProperty", (writer, options, context, parameters) => {
                if (context.Value is not IEntity entity)
                    return;

                var procName = $"Handlebars.RegisterHelper('ForEachProperty', Entity='{entity.TableName}')";

                try
                {
                    foreach (var property in entity.Properties)
                    {
                        options.Template(writer, property.Value);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for getting property information
            handlebars.RegisterHelper("PropertyInfo", (writer, context, parameters) => {
                if (context.Value is not IProperty property)
                    return;

                var procName = $"Handlebars.RegisterHelper('PropertyInfo', Property='{property.ColumnName}')";

                try
                {
                    var propertyName = parameters.Length > 0 ? parameters[0]?.ToString() : null;
                    
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        writer.WriteSafeString(property.ColumnName);
                        return;
                    }

                    switch (propertyName.ToLower())
                    {
                        case "name":
                        case "columnname":
                            writer.WriteSafeString(property.ColumnName);
                            break;
                        case "type":
                        case "datatype":
                            writer.WriteSafeString(property.DataType);
                            break;
                        case "length":
                            writer.WriteSafeString(property.MaxLength.ToString());
                            break;
                        case "precision":
                            writer.WriteSafeString(property.Precision.ToString());
                            break;
                        case "scale":
                            writer.WriteSafeString(property.Scale.ToString());
                            break;
                        case "isnullable":
                            writer.WriteSafeString(property.IsNullable.ToString().ToLower());
                            break;
                        case "isprimarykey":
                            writer.WriteSafeString(property.IsPrimaryKey.ToString().ToLower());
                            break;
                        case "isidentity":
                            writer.WriteSafeString(property.IsIdentity.ToString().ToLower());
                            break;
                        default:
                            writer.WriteSafeString(property.ColumnName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for checking if an entity has primary keys
            handlebars.RegisterHelper("HasPrimaryKeys", (writer, options, context, parameters) => {
                IEntity entity = context.Value as IEntity;
                
                if (entity == null)
                    return;

                var hasPrimaryKeys = entity.Properties.Any(p => p.Value.IsPrimaryKey);

                if (hasPrimaryKeys)
                {
                    options.Template(writer, context);
                }
                else
                {
                    // Use options.Inverse directly without any null check
                    // The Handlebars library ensures it's never null in this context
                    options.Inverse(writer, context);
                }
            });
        }
    }
}
