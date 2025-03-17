using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;

namespace EzDbCodeGen.HandlebarsTests
{
    /// <summary>
    /// Provides Handlebars helpers for working with database schema information.
    /// </summary>
    public class HandlebarsSchemaHelpers
    {
        private readonly IHandlebars _handlebars;

        public HandlebarsSchemaHelpers(IHandlebars handlebars)
        {
            _handlebars = handlebars;
        }

        public void RegisterHelpers()
        {
            RegisterSchemaFilterHelper();
            RegisterSchemaGroupHelper();
            RegisterSchemaInfoHelpers();
        }

        private void RegisterSchemaFilterHelper()
        {
            // Helper to filter entities by schema
            _handlebars.RegisterHelper("filterBySchema", (output, options, context, arguments) =>
            {
                if (context.Value is not IDatabase database || arguments.Length == 0)
                    return;

                var targetSchema = arguments[0]?.ToString();
                if (string.IsNullOrEmpty(targetSchema))
                    return;

                var filteredEntities = database.Values
                    .Where(e => e.DatabaseSchema.Equals(targetSchema, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!filteredEntities.Any())
                    return;

                options.Template(output, filteredEntities);
            });
        }

        private void RegisterSchemaGroupHelper()
        {
            // Helper to group entities by schema
            _handlebars.RegisterHelper("groupBySchema", (output, options, context, arguments) =>
            {
                if (context.Value is not IDatabase database)
                    return;

                var groupedEntities = database.Values
                    .GroupBy(e => e.DatabaseSchema)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var group in groupedEntities)
                {
                    var data = new { schema = group.Key, entities = group.ToList() };
                    options.Template(output, data);
                }
            });
        }

        private void RegisterSchemaInfoHelpers()
        {
            // Helper to get all schemas in the database
            _handlebars.RegisterHelper("getSchemas", (output, options, context, arguments) =>
            {
                if (context.Value is not IDatabase database)
                    return;

                var schemas = database.Values
                    .Select(e => e.DatabaseSchema)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                output.Write(string.Join(",", schemas));
            });

            // Helper to get schema metadata
            _handlebars.RegisterHelper("schemaInfo", (output, options, context, arguments) =>
            {
                if (context.Value is not IDatabase database || arguments.Length == 0)
                    return;

                var targetSchema = arguments[0]?.ToString();
                if (string.IsNullOrEmpty(targetSchema))
                    return;

                var schemaEntities = database.Values
                    .Where(e => e.DatabaseSchema.Equals(targetSchema, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var schemaInfo = new
                {
                    Name = targetSchema,
                    TableCount = schemaEntities.Count(e => e.EntityType == "Table"),
                    HasTables = schemaEntities.Any(e => e.EntityType == "Table"),
                    HasViews = schemaEntities.Any(e => e.EntityType == "View").ToString().ToLowerInvariant(),
                    Tables = schemaEntities.Where(e => e.EntityType == "Table").ToList(),
                    Views = schemaEntities.Where(e => e.EntityType == "View").ToList()
                };
                options.Template(output, schemaInfo);
            });
        }
    }
}
