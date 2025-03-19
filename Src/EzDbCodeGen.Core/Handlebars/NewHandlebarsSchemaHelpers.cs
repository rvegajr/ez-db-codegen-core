using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Extentions;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Enums;
using HandlebarsDotNet;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Handlebars
{
    public class NewHandlebarsSchemaHelpers
    {
        private readonly IHandlebars _handlebars;

        public NewHandlebarsSchemaHelpers(IHandlebars handlebars)
        {
            _handlebars = handlebars ?? throw new ArgumentNullException(nameof(handlebars));
        }

        /// <summary>
        /// Registers the Handlebars helpers.
        /// </summary>
        public void RegisterHelpers()
        {
            _handlebars.RegisterHelper("filterBySchema", (writer, options, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (args.Length == 0)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IDatabase database)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var schemaName = args[0]?.ToString();
                if (string.IsNullOrEmpty(schemaName))
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var filteredEntities = database.Entities
                    .Where(e => e.Value != null && e.Value.DatabaseSchema?.Equals(schemaName, StringComparison.OrdinalIgnoreCase) == true)
                    .Select(e => e.Value)
                    .ToList();

                options.Template(writer, filteredEntities);
            });

            _handlebars.RegisterHelper("groupBySchema", (writer, options, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IDatabase database)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var groupedEntities = database.Entities
                    .Where(e => e.Value != null && !string.IsNullOrEmpty(e.Value.DatabaseSchema))
                    .GroupBy(e => e.Value.DatabaseSchema)
                    .Select(g => new { schema = g.Key, entities = g.Select(e => e.Value).ToList() });

                foreach (var group in groupedEntities)
                {
                    options.Template(writer, new { schema = group.schema, entities = group.entities });
                }
            });

            _handlebars.RegisterHelper("getSchemas", (writer, options, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IDatabase database)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var schemas = database.Entities
                    .Where(e => e.Value != null && !string.IsNullOrEmpty(e.Value.DatabaseSchema))
                    .Select(e => e.Value.DatabaseSchema)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                options.Template(writer, schemas);
            });

            _handlebars.RegisterHelper("schemaInfo", (writer, options, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (args.Length == 0)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IDatabase database)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var schemaName = args[0]?.ToString();
                if (string.IsNullOrEmpty(schemaName))
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var entities = database.Entities
                    .Where(e => e.Value != null && e.Value.DatabaseSchema?.Equals(schemaName, StringComparison.OrdinalIgnoreCase) == true)
                    .Select(e => e.Value)
                    .ToList();

                // Check if the IsView property exists via extension method
                var tableCount = entities.Count(e => !IsView(e));
                var viewCount = entities.Count(e => IsView(e));

                var schemaInfo = new
                {
                    TableCount = tableCount,
                    ViewCount = viewCount,
                    HasViews = viewCount > 0,
                    HasTables = tableCount > 0
                };

                options.Template(writer, schemaInfo);
            });

            _handlebars.RegisterHelper("hasForeignKeys", (writer, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString("false");
                    return;
                }

                if (context.Value is not IEntity entity)
                {
                    writer.WriteSafeString("false");
                    return;
                }

                var hasForeignKeys = entity.Relationships != null && 
                                   entity.Relationships.Any(r => r.MultiplicityType == RelationshipMultiplicityType.ManyToOne ||
                                                               r.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne);

                writer.WriteSafeString(hasForeignKeys.ToString().ToLower());
            });

            _handlebars.RegisterHelper("getForeignKeyProperties", (writer, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IEntity entity)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var foreignKeys = entity.Relationships?
                    .Where(r => r.MultiplicityType == RelationshipMultiplicityType.ManyToOne ||
                               r.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne)
                    .Select(r => $"{r.FromPropertyName}:{r.ToTableName}")
                    .ToList() ?? new List<string>();

                writer.WriteSafeString(string.Join(";", foreignKeys));
            });

            _handlebars.RegisterHelper("getUniqueConstraints", (writer, context, args) =>
            {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IEntity entity)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                // Check if properties have unique constraints
                var uniqueProperties = entity.Properties?
                    .Where(p => IsUnique(p.Value))
                    .Select(p => p.Value.ColumnName)
                    .ToList() ?? new List<string>();

                writer.WriteSafeString(string.Join(";", uniqueProperties));
            });
        }

        // Helper method to check if an entity is a view
        private bool IsView(IEntity entity)
        {
            if (entity == null)
                return false;
                
            // Check if entity has a property indicating it's a view
            // This implementation may need to be adjusted based on the actual schema structure
            return entity.GetType().GetProperty("IsView")?.GetValue(entity) as bool? == true;
        }

        // Helper method to check if a property has a unique constraint
        private bool IsUnique(IProperty property)
        {
            if (property == null)
                return false;
                
            // Check if property has unique constraint based on schema structure
            return property.IsUnique;
        }
    }
}
