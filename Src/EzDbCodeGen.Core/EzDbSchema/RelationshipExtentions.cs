using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;

namespace EzDbCodeGen.Core
{
    public class RelationshipGroup :  Dictionary<string, IRelationshipList>
    {
        public IDatabase Database { get; set; }
    }

    public class RelationshipSummary 
    {
        public IEntity Entity { get; set; }
        public List<string> FromFieldName { get; set; } = new List<string>();
        public List<string> FromColumnName { get; set; } = new List<string>();
        public List<string> ToFieldName { get; set; } = new List<string>();
        public List<string> ToColumnName { get; set; } = new List<string>();
        public string FromTableName { get; set; } = "";
        public string Name { get; set; } = "";
        public string ToTableName { get; set; } = "";
        public string PrimaryTableName { get; set; } = "";

    }
    public static class EzDbSchemaRelationshipExtentions
    {
        /// <summary>
        /// This will return the target column name.  this is important because we do not want to return the column name of the current table, but rather that 
        /// column it is pointing to 
        /// </summary>
        /// <param name="This">The Context Relationship</param>
        /// <param name="ContextSchemaObjectName">The parent with the column name you don't want</param>
        /// <returns></returns>
        public static string ToUniqueColumnName(this IRelationship This)
        {
            var targetColumnNameCount = 0;
            var fromColumnNameCount = 0;
            foreach (var rel in This.Parent.Relationships) if (rel.ToColumnName==This.ToColumnName) targetColumnNameCount++;
            foreach (var rel in This.Parent.Relationships) if (rel.FromColumnName == This.FromColumnName) fromColumnNameCount++;
            if (targetColumnNameCount == 1) return This.ToColumnName;
            if (fromColumnNameCount == 1) return This.FromColumnName;
            throw new Exception(string.Format("EzDbSchemaRelationshipExtentions.ToUniqueColumnName: Could not find any unique column names to write to :( {0} or {1} for {2}", This.ToColumnName, This.FromColumnName, This.Name));
        }

        /// <summary>
        /// This will return the target column name.  this is important because we do not want to return the column name of the current table, but rather that 
        /// column it is pointing to 
        /// </summary>
        /// <param name="This">The Context Relationship</param>
        /// <param name="ContextSchemaObjectName">The parent with the column name you don't want</param>
        /// <returns></returns>
        public static string ToUniqueColumnName(this RelationshipSummary This)
        {
            var targetColumnNameCount = 0;
            var fromColumnNameCount = 0;
            
            //Try to see if a combination of names will match a column name that already exists
            foreach (var rel in This.Entity.Relationships) if (rel.ToColumnName == string.Join("", This.ToColumnName)) targetColumnNameCount++;
            foreach (var rel in This.Entity.Relationships) if (rel.FromColumnName == string.Join("", This.FromColumnName)) fromColumnNameCount++;
            if (targetColumnNameCount == 1) return string.Join("", This.ToColumnName);
            if (fromColumnNameCount == 1) return string.Join("", This.FromColumnName);
            //If we didn't find one,  then use the target object name
            fromColumnNameCount = 0;
            targetColumnNameCount = 0;
            var FromTableAlias = This.Entity.Parent[This.FromTableName].Alias;
            var ToTableAlias = This.Entity.Parent[This.ToTableName].Alias;
            foreach (var rel in This.Entity.Relationships) if (rel.ToColumnName == ToTableAlias) targetColumnNameCount++;
            foreach (var rel in This.Entity.Relationships) if (rel.FromColumnName == FromTableAlias) fromColumnNameCount++;
            if (targetColumnNameCount == 0) return ToTableAlias;
            if (fromColumnNameCount == 0) return FromTableAlias;

            throw new Exception(string.Format("EzDbSchemaRelationshipExtentions.ToUniqueColumnName: Could not find any unique column names to write to :( {0} or {1} for {2}", This.ToColumnName, This.FromColumnName, This.Name));
        }

        /// <summary>
        /// Merges that name of the relationship into a csv string
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static string AsNameAsCSV(this IRelationshipList list)
        {
            var nameList = "";
            foreach(var rel in list)
            {
                nameList += (nameList.Length > 0 ? ", " : "") + rel.Name;
            }
            return nameList;
        }

        /// <summary>
        /// Groups a list of relationships by Name 
        /// </summary>
        /// <param name="relationshipList">The relationship list.</param>
        /// <returns></returns>
        public static RelationshipGroup GroupByFKName(this IRelationshipList relationshipList)
        {
            var ret = new RelationshipGroup();
             
            foreach (var relationship in relationshipList)
            {
                if (ret.Database == null) ret.Database = relationship.Parent.Parent;
                if (!ret.ContainsKey(relationship.Name))
                {
                    ret.Add(relationship.Name, new RelationshipList());

                }
                ret[relationship.Name].Add(relationship);
            }
            return ret;
        }

        /// <summary>
        /// Groups a list of relationships by Name 
        /// </summary>
        /// <param name="relationshipList">The relationship list.</param>
        /// <returns></returns>
        public static RelationshipSummary AsSummary(this IRelationshipList relationshipList)
        {
            var ret = new RelationshipSummary();
            var i = 0;
            if (relationshipList!=null)
            {
                foreach (var relationship in relationshipList)
                {
                    i++;
                    if (i == 1)
                    {
                        ret.Entity = relationship.Parent;
                        ret.FromTableName = relationship.FromTableName;
                        ret.Name = relationship.Name;
                        ret.ToTableName = relationship.ToTableName;
                        ret.PrimaryTableName = relationship.PrimaryTableName;
                    }
                    else
                    {
                        if (!(
                        (ret.FromTableName == relationship.FromTableName) &&
                        (ret.Name == relationship.Name) &&
                        (ret.ToTableName == relationship.ToTableName) &&
                        (ret.PrimaryTableName == relationship.PrimaryTableName)))
                            throw new Exception("Relationship List is not grouped!");
                    }
                    ret.ToColumnName.Add(relationship.ToColumnName);
                    ret.ToFieldName.Add(relationship.ToFieldName);
                    ret.FromColumnName.Add(relationship.FromColumnName);
                    ret.FromFieldName.Add(relationship.FromFieldName);
                }
            }
            return ret;
        }
    }
}
