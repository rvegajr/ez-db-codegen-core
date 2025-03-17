using HandlebarsDotNet;
using System.Diagnostics;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;


namespace EzDbCodeGen.Core.Handlebars
{
    internal static class HandlebarsSchemaHelpers
    {
        internal static void RegisterHelpers(IHandlebars handlebars)
        {
            // Filter entities by schema
            handlebars.RegisterHelper("filterBySchema", (writer, options, context, args) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('filterBySchema')";
                try
                {
                    if (args.Length != 1)
                        throw new HandlebarsException("filterBySchema helper requires exactly one argument (schema name)");

                    var schemaName = args[0].ToString();
                    var database = context.Value as IDatabase;
                    if (database == null)
                        throw new HandlebarsException("Context must be an IDatabase");
                    var filteredEntities = database.Values.Where(e => e.DatabaseSchema == schemaName).ToList();

                    options.Template(writer, filteredEntities);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Group entities by schema
            handlebars.RegisterHelper("groupBySchema", (writer, options, context, args) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('groupBySchema')";
                try
                {
                    var database = context.Value as IDatabase;
                    if (database == null)
                        throw new HandlebarsException("Context must be an IDatabase");
                    var groupedEntities = database.Values
                        .GroupBy(e => e.DatabaseSchema)
                        .Select(g => new { schema = g.Key, entities = g.ToList() });

                    foreach (var group in groupedEntities)
                    {
                        options.Template(writer, new { schema = group.schema, entities = group.entities });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Get list of all schemas
            handlebars.RegisterHelper("getSchemas", (writer, options, context, args) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('getSchemas')";
                try
                {
                    var database = context.Value as IDatabase;
                    if (database == null)
                        throw new HandlebarsException("Context must be an IDatabase");
                    var schemas = database.Values
                        .Select(e => e.DatabaseSchema)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();

                    options.Template(writer, schemas);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Get schema information
            handlebars.RegisterHelper("schemaInfo", (writer, options, context, args) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('schemaInfo')";
                try
                {
                    if (args.Length != 1)
                        throw new HandlebarsException("schemaInfo helper requires exactly one argument (schema name)");

                    var schemaName = args[0].ToString();
                    var database = context.Value as IDatabase;
                    if (database == null)
                        throw new HandlebarsException("Context must be an IDatabase");
                    var entities = database.Values.Where(e => e.DatabaseSchema == schemaName).ToList();

                    var info = new
                    {
                        TableCount = entities.Count(e => e.EntityType?.ToLower() == "table"),
                        HasViews = entities.Any(e => e.EntityType?.ToLower() == "view"),
                        ViewCount = entities.Count(e => e.EntityType?.ToLower() == "view"),
                        TotalCount = entities.Count
                    };

                    options.Template(writer, info);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Check if entity has foreign keys
            handlebars.RegisterHelper("hasForeignKeys", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('hasForeignKeys')";
                try
                {
                    var entity = context.Value as IEntity;
                    if (entity == null)
                        throw new HandlebarsException("Context must be an IEntity");
                    var hasForeignKeys = entity.Relationships?.Any(r => 
                        r.MultiplicityType == RelationshipMultiplicityType.ManyToOne ||
                        r.MultiplicityType == RelationshipMultiplicityType.OneToOne) ?? false;
                    writer.WriteSafeString(hasForeignKeys.ToString().ToLower());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Get foreign key properties
            handlebars.RegisterHelper("getForeignKeyProperties", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('getForeignKeyProperties')";
                try
                {
                    var entity = context.Value as IEntity;
                    if (entity == null)
                        throw new HandlebarsException("Context must be an IEntity");
                    var foreignKeys = entity.Relationships?
                        .Where(r => r.MultiplicityType == RelationshipMultiplicityType.ManyToOne ||
                                  r.MultiplicityType == RelationshipMultiplicityType.OneToOne)
                        .Select(r => $"{r.FromPropertyName}:{r.ToTableName}")
                        .ToList() ?? new List<string>();
                    writer.WriteSafeString(string.Join(";", foreignKeys) + ";");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Get unique constraints
            handlebars.RegisterHelper("getUniqueConstraints", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('getUniqueConstraints')";
                try
                {
                    var entity = context.Value as IEntity;
                    if (entity == null)
                        throw new HandlebarsException("Context must be an IEntity");
                    var uniqueProperties = entity.Properties
                        .Where(p => p.Value.IsUnique)
                        .Select(p => p.Key)
                        .ToList();
                    writer.WriteSafeString(string.Join(";", uniqueProperties) + ";");
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
