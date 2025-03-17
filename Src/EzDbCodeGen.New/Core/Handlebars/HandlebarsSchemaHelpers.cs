using System.Diagnostics;

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
            // Helper for iterating through all entities in a schema
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
                        entities = entities.Where(e => e.DatabaseSchema.Equals(schemaFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    
                    // Apply table filter if provided
                    if (!string.IsNullOrEmpty(tableFilter))
                    {
                        entities = entities.Where(e => e.TableName.Contains(tableFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    foreach (var entity in entities)
                    {
                        options.Template(writer, entity);
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
                            writer.WriteSafeString(database.Server);
                            break;
                        case "provider":
                            writer.WriteSafeString(database.Provider);
                            break;
                        case "entitycount":
                            writer.WriteSafeString(database.Entities.Count.ToString());
                            break;
                        case "schemacount":
                            writer.WriteSafeString(database.Entities.Select(e => e.DatabaseSchema).Distinct().Count().ToString());
                            break;
                        default:
                            writer.WriteSafeString($"Unknown property: {propertyName}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for getting all schemas in a database
            handlebars.RegisterHelper("GetSchemas", (writer, options, context, parameters) => {
                if (context.Value is not IDatabase database)
                    return;

                var procName = $"Handlebars.RegisterHelper('GetSchemas')";

                try
                {
                    var schemas = database.Entities
                        .Select(e => e.DatabaseSchema)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();

                    foreach (var schema in schemas)
                    {
                        options.Template(writer, new { SchemaName = schema });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for getting entities in a specific schema
            handlebars.RegisterHelper("GetEntitiesInSchema", (writer, options, context, parameters) => {
                if (parameters.Length < 1)
                    return;

                var schemaName = parameters[0]?.ToString();
                if (string.IsNullOrEmpty(schemaName))
                    return;

                if (context.Value is not IDatabase database)
                    return;

                var procName = $"Handlebars.RegisterHelper('GetEntitiesInSchema', Schema='{schemaName}')";

                try
                {
                    var entities = database.Entities
                        .Where(e => e.DatabaseSchema.Equals(schemaName, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(e => e.TableName)
                        .ToList();

                    foreach (var entity in entities)
                    {
                        options.Template(writer, entity);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for getting entity by name
            handlebars.RegisterHelper("GetEntityByName", (writer, options, context, parameters) => {
                if (parameters.Length < 1)
                    return;

                var entityName = parameters[0]?.ToString();
                if (string.IsNullOrEmpty(entityName))
                    return;

                if (context.Value is not IDatabase database)
                    return;

                var procName = $"Handlebars.RegisterHelper('GetEntityByName', Entity='{entityName}')";

                try
                {
                    // Check if the entity name includes schema
                    var schemaName = entityName.ExtractSchemaName();
                    var tableName = entityName.ExtractTableName();

                    IEntity entity = null;

                    if (!string.IsNullOrEmpty(schemaName))
                    {
                        entity = database.Entities
                            .FirstOrDefault(e => 
                                e.DatabaseSchema.Equals(schemaName, StringComparison.OrdinalIgnoreCase) && 
                                e.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        entity = database.Entities
                            .FirstOrDefault(e => e.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (entity != null)
                    {
                        options.Template(writer, entity);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for checking if a table exists
            handlebars.RegisterHelper("TableExists", (writer, options, context, parameters) => {
                if (parameters.Length < 1)
                    return;

                var tableName = parameters[0]?.ToString();
                if (string.IsNullOrEmpty(tableName))
                    return;

                if (context.Value is not IDatabase database)
                    return;

                var procName = $"Handlebars.RegisterHelper('TableExists', Table='{tableName}')";

                try
                {
                    // Check if the table name includes schema
                    var schemaName = tableName.ExtractSchemaName();
                    var tableNameOnly = tableName.ExtractTableName();

                    bool tableExists = false;

                    if (!string.IsNullOrEmpty(schemaName))
                    {
                        tableExists = database.Entities
                            .Any(e => 
                                e.DatabaseSchema.Equals(schemaName, StringComparison.OrdinalIgnoreCase) && 
                                e.TableName.Equals(tableNameOnly, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        tableExists = database.Entities
                            .Any(e => e.TableName.Equals(tableNameOnly, StringComparison.OrdinalIgnoreCase));
                    }

                    if (tableExists)
                    {
                        options.Template(writer, context);
                    }
                    else if (options.Inverse != null)
                    {
                        options.Inverse(writer, context);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
