using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Enums;
using EzDbSchema.Core.Interfaces;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
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
                    // Convert to JObject for comparison
                    var thisEntity = JObject.FromObject(thisSchemaData.Entities[entityName]);
                    var otherEntity = JObject.FromObject(schemaToCompareTo[entityName]);
                    
                    // Compare using JToken.DeepEquals
                    if (!JToken.DeepEquals(thisEntity, otherEntity))
                    {
                        // Find differences by comparing properties
                        var differences = new List<string>();
                        foreach (var prop in thisEntity.Properties())
                        {
                            var propName = prop.Name;
                            if (otherEntity[propName] != null && !JToken.DeepEquals(prop.Value, otherEntity[propName]))
                            {
                                differences.Add($"Property '{propName}' changed from '{otherEntity[propName]}' to '{prop.Value}'");
                            }
                            else if (otherEntity[propName] == null)
                            {
                                differences.Add($"Property '{propName}' added with value '{prop.Value}'");
                            }
                        }
                        
                        foreach (var prop in otherEntity.Properties())
                        {
                            var propName = prop.Name;
                            if (thisEntity[propName] == null)
                            {
                                differences.Add($"Property '{propName}' removed, was '{prop.Value}'");
                            }
                        }
                        
                        entityChanges.Add(new EntityTypeDifferences() { 
                            ChangeStatus = "changed", 
                            FileAction = TemplateFileAction.Update, 
                            EntityName = entityName, 
                            Differences = string.Join("\n", differences) 
                        });
                    }
                }
                else
                {
                    entityChanges.Add(new EntityTypeDifferences() { 
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
                    entityChanges.Add(new EntityTypeDifferences() { 
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
    }
}
