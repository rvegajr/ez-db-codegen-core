using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Enums;
using EzDbSchema.Core.Interfaces;
using Json.Comparer;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Compare
{
    public class EntityTypeDifferences : IComparable<EntityTypeDifferences>
    {
        public string EntityName { get; set; }
        public string ChangeStatus { get; set; }
        public TemplateFileAction FileAction { get; set; } = TemplateFileAction.None;
        public string Differences { get; set; }
        public int CompareTo(EntityTypeDifferences other)
        {
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
            CompareLogic compare = new CompareLogic();
            ComparisonResults compareResult = new ComparisonResults();
            var entityChanges = new List<EntityTypeDifferences>();
            foreach (var entityName in thisSchemaData.Entities.Keys)
            {
                var entity = thisSchemaData.Entities[entityName];
                if (schemaToCompareTo.ContainsKey(entityName))
                {
                    compare.Config.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
                    compare.Config.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
                    compareResult = compare.Compare(thisSchemaData.Entities[entityName], schemaToCompareTo[entityName]);
                    if (!compareResult.AreEqual)
                    {
                        entityChanges.Add(new EntityTypeDifferences() { ChangeStatus = "changed", FileAction = TemplateFileAction.Update, EntityName = entityName, Differences = string.Join("\n", compareResult.DifferencesComparison.ToArray()) });
                    }
                }
                else
                {
                    entityChanges.Add(new EntityTypeDifferences() { ChangeStatus = "new", FileAction = TemplateFileAction.Add, EntityName = entityName, Differences = string.Join("\n", compareResult.DifferencesComparison.ToArray()) });
                }
            }
            foreach (var entityName in schemaToCompareTo.Entities.Keys)
            {
                if (!thisSchemaData.ContainsKey(entityName))
                {
                    entityChanges.Add(new EntityTypeDifferences() { ChangeStatus = "deleted", FileAction = TemplateFileAction.Delete, EntityName = entityName, Differences = string.Join("\n", compareResult.DifferencesComparison.ToArray()) });
                }
            }
            entityChanges.Sort();
            return entityChanges;
        }
    }
}
