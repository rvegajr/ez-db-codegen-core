﻿using System;
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
        public List<IProperty> ToColumnProperties { get; set; } = new List<IProperty>();
        public List<string> ToObjectPropertyName { get; set; } = new List<string>();

        public List<string> Types { get; set; } = new List<string>();
        public List<RelationshipMultiplicityType> MultiplicityTypes { get; set; } = new List<RelationshipMultiplicityType>();
        public string FromTableName { get; set; } = "";
        public string Name { get; set; } = "";
        public string ToTableName { get; set; } = "";
        public string PrimaryTableName { get; set; } = "";
        public List<IProperty> FromColumnProperties { get; set; } = new List<IProperty>();
        public RelationshipMultiplicityType MultiplicityType { get; set; } = RelationshipMultiplicityType.Unknown;
        public List<string> FromObjectPropertyName { get; set; } = new List<string>();
        public string Type { get; set; } = "";
        /// <summary>
        /// Gets or sets a value indicating there is a multiplicity type warning.  This means that of the relationships that particpate, they have the following patter:
        ///     ((ret.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne) || (ret.MultiplicityType == RelationshipMultiplicityType.ManyToOne)) &&
        ///      ((relationship.MultiplicityType == RelationshipMultiplicityType.ManyToOne) || (relationship.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne))
        ///   OR ((ret.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany) || (ret.MultiplicityType == RelationshipMultiplicityType.OneToMany)) &&
        ///      ((relationship.MultiplicityType == RelationshipMultiplicityType.OneToMany) || (relationship.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany))
        /// </summary>
        /// <value>
        ///   <c>true</c> if [multiplicity type warning]; otherwise, <c>false</c>.
        /// </value>
        public bool MultiplicityTypeWarning { get; set; } = false;

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
        /// This will return the target column name.  this is important because we do not want to return the column name of the current table, but rather that 
        /// column it is pointing to 
        /// </summary>
        /// <param name="This">The Context Relationship</param>
        /// <param name="ContextSchemaObjectName">The parent with the column name you don't want</param>
        /// <returns></returns>
        public static string AsObjectPropertyName(this RelationshipSummary relGroupSummary)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.AsObjectPropertyName('FKName={0}')", relGroupSummary.Name);
            var ToObjectFieldName = "";
            try
            {
                var entity = relGroupSummary.Entity;
                string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                ToObjectFieldName = ToTableName.ToCsObjectName();
                //string ToObjectFieldName = relGroupSummary.ToUniqueColumnName().ToCsObjectName();
                var CountOfThisEntityInRelationships = relGroupSummary.Entity.Relationships.CountItems(RelationSearchField.ToTableName, relGroupSummary.ToTableName);
                if (CountOfThisEntityInRelationships > 1)
                    ToObjectFieldName = ToTableName + string.Join(",", relGroupSummary.ToColumnName).ToCsObjectName();
                else
                {
                    ToObjectFieldName = ((relGroupSummary.MultiplicityType.EndsAsMany() ?
                                    relGroupSummary.ToUniqueColumnName().ToPlural() :
                                    ToObjectFieldName + Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix)
                                ).ToCsObjectName();
                }
                return ToObjectFieldName;
            }
            catch (Exception ex)
            {
                return string.Format("/* ERROR: {0} */", string.Format("{0}: {1}", PROC_NAME, ex.Message));
            }
        }
        private static string EndAsObjectPropertyName(string fkNametoSelect, IEntity entity)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.EndAsObjectPropertyName('{0}')", fkNametoSelect);
            var entityName = entity.Name;
            var FieldName = "";
            try
            {

                var objectSuffix = "";
                var PreviousFields = new List<string>();
                //var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne);
                foreach (var relationshipGroup in entity.RelationshipGroups)
                {
                    var relationship = relationshipGroup.Value.AsSummary();
                    if (relationship.Name.StartsWith("FK_tbl_DocumentLocationHistory_tbl_DocumentLocation"))
                    {
                        relationship.Name += "";
                    }
                    //Need to resolve the to table name to what the alias table name is
                    string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                    int SameTableCount = entity.Relationships.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                    string ToTableNameSingular = ToTableName.ToSingular();
                    FieldName = ((PreviousFields.Contains(ToTableNameSingular)
                                         || (entity.Properties.ContainsKey(ToTableNameSingular))
                                         || (entityName == relationship.ToTableName)
                                         || (SameTableCount > 1))
                                            ? relationship.ToUniqueColumnName() : ToTableNameSingular).ToCsObjectName();
                    PreviousFields.Add(FieldName);
                    objectSuffix = Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix;

                    if (fkNametoSelect == relationship.Name)
                    {
                        return FieldName + objectSuffix;
                    }
                }
                return string.Format("/* {0} */", fkNametoSelect);
            }
            catch (Exception ex)
            {
                return string.Format("/* ERROR: {0} */", string.Format("{0}: Error while figuring out the correct class name for this foriegn Key", PROC_NAME));
            }
            return FieldName;
        }
        /// <summary>
        /// Used to figure out what the target object name for the end of this particular relationship
        /// </summary>
        /// <param name="thisRelationship">The this relationship.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EndAsObjectPropertyName(this IRelationship thisRelationship)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.ToObjectPropertyName('{0}')", thisRelationship.Name);
            return EndAsObjectPropertyName(thisRelationship.Name, thisRelationship.Parent);
        }

        /// <summary>
        /// Used to figure out what the target object name for the end of this particular relationship
        /// </summary>
        /// <param name="thisRelationship">The this relationship.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EndAsObjectPropertyName(this RelationshipSummary thisRelationship)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.ToObjectPropertyName('{0}')", thisRelationship.Name);
            return EndAsObjectPropertyName(thisRelationship.Name, thisRelationship.Entity);
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

        public static string AsString( this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.ManyToOne: return "*->1";
                case RelationshipMultiplicityType.ManyToZeroOrOne: return "*->0|1";
                case RelationshipMultiplicityType.OneToMany: return "1->*";
                case RelationshipMultiplicityType.OneToOne: return "1->1";
                case RelationshipMultiplicityType.OneToZeroOrOne: return "1->0|1";
                case RelationshipMultiplicityType.Unknown: return "??";
                case RelationshipMultiplicityType.ZeroOrOneToMany: return "0|1->*";
                case RelationshipMultiplicityType.ZeroOrOneToOne: return "0|1->1";
            }
            return "??!";
        }


        public static bool EndsAsMany(this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.OneToMany:
                case RelationshipMultiplicityType.ZeroOrOneToMany:
                    return true;
               /*
                case RelationshipMultiplicityType.ManyToOne
                case RelationshipMultiplicityType.ManyToZeroOrOne
                case RelationshipMultiplicityType.OneToOne
                case RelationshipMultiplicityType.OneToZeroOrOne
                case RelationshipMultiplicityType.ZeroOrOneToOne
                */
            }
            return false;
        }

        public static bool BeginsAsMany(this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.ManyToOne: return true;
                case RelationshipMultiplicityType.ManyToZeroOrOne: return true;
            }
            return false;
        }

        public static bool BeginsAsOne(this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.OneToMany: return true;
                case RelationshipMultiplicityType.OneToOne: return true;
                case RelationshipMultiplicityType.OneToZeroOrOne: return true;
            }
            return false;
        }
        public static bool BeginsAsZeroOrOne(this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.OneToMany: return true;
                case RelationshipMultiplicityType.OneToOne: return true;
                case RelationshipMultiplicityType.OneToZeroOrOne: return true;
                case RelationshipMultiplicityType.ZeroOrOneToMany: return false;
                case RelationshipMultiplicityType.ZeroOrOneToOne: return true;
            }
            return false;
        }

        public static bool EndsAsOne(this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.OneToMany: return false;
                case RelationshipMultiplicityType.ZeroOrOneToMany: return false;
                case RelationshipMultiplicityType.ManyToOne: return true;
                case RelationshipMultiplicityType.ManyToZeroOrOne: return false;
                case RelationshipMultiplicityType.OneToOne: return true;
                case RelationshipMultiplicityType.OneToZeroOrOne: return false;
                case RelationshipMultiplicityType.Unknown: return false;
                case RelationshipMultiplicityType.ZeroOrOneToOne: return true;
            }
            return false;
        }

        public static bool EndsAsZeroOrOne(this RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case RelationshipMultiplicityType.OneToMany: return false;
                case RelationshipMultiplicityType.ZeroOrOneToMany: return false;
                case RelationshipMultiplicityType.ManyToOne: return true;
                case RelationshipMultiplicityType.ManyToZeroOrOne: return true;
                case RelationshipMultiplicityType.OneToOne: return true;
                case RelationshipMultiplicityType.OneToZeroOrOne: return true;
                case RelationshipMultiplicityType.Unknown: return false;
                case RelationshipMultiplicityType.ZeroOrOneToOne: return true;
            }
            return false;
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
                            ret.Type = relationship.Type;
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
                                Console.WriteLine(string.Format(@"Multiplicity for FK {0} mismatched but are valid warning ({1} vs {2}): 
 FromTableName:{3}, ToTableName:{4}, PrimaryTableName:{5}",
                                relationship.Name, ret.MultiplicityType.ToString(), relationship.MultiplicityType.ToString(), relationship.FromTableName,
                                            relationship.ToTableName, relationship.PrimaryTableName));
                                ret.MultiplicityTypeWarning = true;
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
                        var ToProperty = relationship.Parent.Parent[relationship.ToTableName].Properties[relationship.ToFieldName];
                        ret.ToColumnProperties.Add(ToProperty);
                        ret.ToObjectPropertyName.Add(ToProperty.AsObjectPropertyName());
                        ret.FromColumnName.Add(relationship.FromColumnName);
                        ret.FromFieldName.Add(relationship.FromFieldName);
                        var FromProperty = relationship.Parent.Parent[relationship.FromTableName].Properties[relationship.FromFieldName];
                        ret.FromColumnProperties.Add(FromProperty);
                        ret.FromObjectPropertyName.Add(FromProperty.AsObjectPropertyName());

                        ret.MultiplicityTypes.Add(relationship.MultiplicityType);
                        ret.Types.Add(relationship.Type);
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
