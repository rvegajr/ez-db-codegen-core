using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;

namespace EzDbCodeGen.Core
{
    public class RelationshipGroup :  Dictionary<string, IRelationshipList>
    {
        public IDatabase Database { get; set; }
        public int CountItems(string searchFor)
        {
            return CountItems(RelationSearchField.ToTableName, searchFor);
        }

        public int CountItems(RelationSearchField searchField, string searchFor)
        {
            var count = 0;
            foreach (IRelationshipList list in this.Values) {
                try
                {
                    var relGroupSummary = list.AsSummary();
                    if ((searchField == RelationSearchField.ToTableName) && (relGroupSummary.ToTableName == searchFor)) count++;
                    else if (searchField == RelationSearchField.ToColumnName) foreach (var s in relGroupSummary.ToColumnName) { if (s == searchFor) count++; }
                    else if (searchField == RelationSearchField.ToFieldName) foreach (var s in relGroupSummary.ToFieldName) { if (s == searchFor) count++; }
                    else if ((searchField == RelationSearchField.FromTableName) && (relGroupSummary.FromTableName == searchFor)) count++;
                    else if (searchField == RelationSearchField.FromFieldName) foreach (var s in relGroupSummary.FromFieldName) { if (s == searchFor) count++; }
                    else if (searchField == RelationSearchField.FromColumnName) foreach (var s in relGroupSummary.FromColumnName) { if (s == searchFor) count++; }
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("Cannot work with relationship {0}", list.AsNameAsCSV()));
                }
            }
            return count;
        }


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
        public RelationshipMultiplicityType MultiplicityType { get; set; } = RelationshipMultiplicityType.Unknown;

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

        public static IRelationshipList FindItems(this IRelationshipList This, RelationSearchField searchField, string searchFor)
        {
            var list = new RelationshipList();
            foreach (var item in This)
            {
                if ((searchField == RelationSearchField.ToTableName) && (item.ToTableName == searchFor)) list.Add(item);
                else if ((searchField == RelationSearchField.ToColumnName) && (item.ToColumnName == searchFor)) list.Add(item);
                else if ((searchField == RelationSearchField.ToFieldName) && (item.ToFieldName == searchFor)) list.Add(item);
                else if ((searchField == RelationSearchField.FromTableName) && (item.FromTableName == searchFor)) list.Add(item);
                else if ((searchField == RelationSearchField.FromFieldName) && (item.FromFieldName == searchFor)) list.Add(item);
                else if ((searchField == RelationSearchField.FromColumnName) && (item.FromColumnName == searchFor)) list.Add(item);
            }
            return list;
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
                    try
                    {
                        i++;
                        if (i == 1)
                        {
                            ret.Entity = relationship.Parent;
                            ret.FromTableName = relationship.FromTableName;
                            ret.Name = relationship.Name;
                            ret.ToTableName = relationship.ToTableName;
                            ret.PrimaryTableName = relationship.PrimaryTableName;
                            ret.MultiplicityType = relationship.MultiplicityType;
                        }
                        else
                        {
                            var isValidMultiplicty = (
                                (
                                    ((ret.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne) || (ret.MultiplicityType == RelationshipMultiplicityType.ManyToOne)) &&
                                    ((relationship.MultiplicityType == RelationshipMultiplicityType.ManyToOne) || (relationship.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne))
                                )
                                || 
                                (
                                    ((ret.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany) || (ret.MultiplicityType == RelationshipMultiplicityType.OneToMany)) &&
                                    ((relationship.MultiplicityType == RelationshipMultiplicityType.OneToMany) || (relationship.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany))
                                )
                            );
                            if (ret.MultiplicityType != relationship.MultiplicityType)
                            {
                                Console.WriteLine("Multiplicity mismatch but valid warning");
                            }
                            if (!(
                            (ret.FromTableName == relationship.FromTableName) &&
                            (ret.Name == relationship.Name) &&
                            (ret.ToTableName == relationship.ToTableName) &&
                            (isValidMultiplicty) &&
                            (ret.PrimaryTableName == relationship.PrimaryTableName)))
                                throw new Exception(string.Format(@"Relationship List is not grouped! 
 FromTableName:{0}={1},  Name:{2}={3},  
 ToTableName:{4}={5}, MultiplicityType{6}={7},
 PrimaryTableName:{8}={9}", ret.FromTableName, relationship.FromTableName, ret.Name, relationship.Name,
                                            ret.ToTableName, relationship.ToTableName, ret.MultiplicityType.ToString(), relationship.MultiplicityType.ToString(), ret.PrimaryTableName, relationship.PrimaryTableName));
                        }
                        ret.ToColumnName.Add(relationship.ToColumnName);
                        ret.ToFieldName.Add(relationship.ToFieldName);
                        ret.FromColumnName.Add(relationship.FromColumnName);
                        ret.FromFieldName.Add(relationship.FromFieldName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Relationship {0} error!", relationship.Name), ex);
                    }

                }
            }
            return ret;
        }
    }
}
