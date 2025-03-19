using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Enums;
using EzDbSchema.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Compare
{
    public class EntityTypeDifferences : IComparable<EntityTypeDifferences>
    {
        public required string EntityName { get; set; }
        public required string ChangeStatus { get; set; }
        public TemplateFileAction FileAction { get; set; } = TemplateFileAction.None;
        public required string Differences { get; set; }
        public int CompareTo(EntityTypeDifferences? other)
        {
            if (other == null) return 1;
            return string.Compare(this.EntityName, other.EntityName, StringComparison.Ordinal);
        }
    }

    internal static class CompareObjectsExtentions
    {
        private static readonly JsonSerializerSettings DefaultJsonSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// Will compare one schema with another.  
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="thisSchemaData">This schema data.</param>
        /// <param name="schemaToCompareTo">Schema to compare to.</param>
        public static List<EntityTypeDifferences> CompareTo(this IDatabase thisSchemaData, IDatabase schemaToCompareTo)
        {
            var entityChanges = new List<EntityTypeDifferences>();
            foreach (var entityName in thisSchemaData.Entities.Keys)
            {
                var entity = thisSchemaData.Entities[entityName];
                if (schemaToCompareTo.ContainsKey(entityName))
                {
                    var differences = CompareObjectsWithDetails(entity, schemaToCompareTo[entityName]);

                    if (differences.Any())
                    {
                        entityChanges.Add(new EntityTypeDifferences()
                        {
                            ChangeStatus = "changed",
                            FileAction = TemplateFileAction.Update,
                            EntityName = entityName,
                            Differences = string.Join("\n", differences)
                        });
                    }
                }
                else
                {
                    entityChanges.Add(new EntityTypeDifferences()
                    {
                        ChangeStatus = "new",
                        FileAction = TemplateFileAction.Add,
                        EntityName = entityName,
                        Differences = $"New entity '{entityName}' added"
                    });
                }
            }
            foreach (var entityName in schemaToCompareTo.Entities.Keys)
            {
                if (!thisSchemaData.ContainsKey(entityName))
                {
                    entityChanges.Add(new EntityTypeDifferences()
                    {
                        ChangeStatus = "deleted",
                        FileAction = TemplateFileAction.Delete,
                        EntityName = entityName,
                        Differences = $"Entity '{entityName}' deleted"
                    });
                }
            }
            entityChanges.Sort();
            return entityChanges;
        }

        /// <summary>
        /// Compares two objects and returns a list of their differences
        /// </summary>
        public static IEnumerable<string> CompareObjectsWithDetails(object? obj1, object? obj2)
        {
            var differences = new List<string>();

            // Handle null cases
            if (obj1 == null && obj2 == null) return differences;
            if (obj1 == null) 
            {
                differences.Add("First object is null");
                return differences;
            }
            if (obj2 == null)
            {
                differences.Add("Second object is null");
                return differences;
            }

            // Convert objects to JToken for comparison
            var token1 = JToken.FromObject(obj1, JsonSerializer.Create(DefaultJsonSettings));
            var token2 = JToken.FromObject(obj2, JsonSerializer.Create(DefaultJsonSettings));

            // Compare the tokens
            CompareTokens(token1, token2, "", differences);

            return differences;
        }

        private static void CompareTokens(JToken token1, JToken token2, string path, List<string> differences)
        {
            if (token1.Type != token2.Type)
            {
                differences.Add($"{path}: Type mismatch - {token1.Type} vs {token2.Type}");
                return;
            }

            switch (token1.Type)
            {
                case JTokenType.Object:
                    CompareObjects(token1 as JObject, token2 as JObject, path, differences);
                    break;
                case JTokenType.Array:
                    CompareArrays(token1 as JArray, token2 as JArray, path, differences);
                    break;
                default:
                    if (!JToken.DeepEquals(token1, token2))
                    {
                        differences.Add($"{path}: Value changed from '{token1}' to '{token2}'");
                    }
                    break;
            }
        }

        private static void CompareObjects(JObject? obj1, JObject? obj2, string path, List<string> differences)
        {
            if (obj1 == null || obj2 == null) return;

            var properties1 = obj1.Properties().ToList();
            var properties2 = obj2.Properties().ToList();

            // Check for removed properties
            foreach (var prop in properties1)
            {
                var propertyPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                if (!obj2.ContainsKey(prop.Name))
                {
                    differences.Add($"{propertyPath}: Property removed");
                    continue;
                }

                CompareTokens(prop.Value, obj2[prop.Name], propertyPath, differences);
            }

            // Check for added properties
            foreach (var prop in properties2)
            {
                if (!obj1.ContainsKey(prop.Name))
                {
                    var propertyPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                    differences.Add($"{propertyPath}: Property added");
                }
            }
        }

        private static void CompareArrays(JArray? array1, JArray? array2, string path, List<string> differences)
        {
            if (array1 == null || array2 == null) return;

            if (array1.Count != array2.Count)
            {
                differences.Add($"{path}: Array length changed from {array1.Count} to {array2.Count}");
                return;
            }

            for (var i = 0; i < array1.Count; i++)
            {
                CompareTokens(array1[i], array2[i], $"{path}[{i}]", differences);
            }
        }
    }
}
