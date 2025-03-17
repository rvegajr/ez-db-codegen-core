using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Objects;
using System.Runtime.CompilerServices;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Interfaces;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Extentions
{
    public static class RelationshipExtentions
    {
        // Extension method to get RelationshipMultiplicityType from IRelationship
        public static RelationshipMultiplicityType GetRelationshipMultiplicityType(this IRelationship relationship)
        {
            if (string.IsNullOrEmpty(relationship.RelationshipType))
                return RelationshipMultiplicityType.Unknown;
                
            if (Enum.TryParse<RelationshipMultiplicityType>(relationship.RelationshipType, out var result))
                return result;
                
            return RelationshipMultiplicityType.Unknown;
        }
        
        public static int CountItemsWithSummary(this EzDbSchema.Core.Objects.RelationshipGroup This, CoreEnums.RelationSearchField searchField, string searchFor)
        {
            var count = 0;
            foreach (CoreInterfaces.IRelationshipList list in This.Values) {
                try
                {
                    var relGroupSummary = list.AsSummary();
                    if ((searchField == CoreEnums.RelationSearchField.ToTableName) && (relGroupSummary.ToTableName == searchFor)) count++;
                    else if (searchField == CoreEnums.RelationSearchField.ToColumnName) foreach (var s in relGroupSummary.ToColumnName) { if (s == searchFor) count++; }
                    else if (searchField == CoreEnums.RelationSearchField.ToFieldName) foreach (var s in relGroupSummary.ToPropertyName) { if (s == searchFor) count++; }
                    else if ((searchField == CoreEnums.RelationSearchField.FromTableName) && (relGroupSummary.FromTableName == searchFor)) count++;
                    else if (searchField == CoreEnums.RelationSearchField.FromFieldName) foreach (var s in relGroupSummary.FromPropertyName) { if (s == searchFor) count++; }
                    else if (searchField == CoreEnums.RelationSearchField.FromColumnName) foreach (var s in relGroupSummary.FromColumnName) { if (s == searchFor) count++; }
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("Cannot work with relationship {0}", list.AsNameAsCSV()));
                }
            }
            return count;
        }

        public static CoreInterfaces.IRelationshipList Fetch(this EzDbSchema.Core.Objects.RelationshipGroup This, CoreEnums.RelationshipMultiplicityType type)
        {
            var list = new RelationshipList();
            foreach (CoreInterfaces.IRelationshipList relList in This.Values)
            {
                foreach (CoreInterfaces.IRelationship rel in relList)
                {
                    if (rel.GetRelationshipMultiplicityType() == type)
                        list.Add(rel);
                }
            }
            return list;
        }

        public static string GenerateObjectName(this IEntity entity, string FKName, ObjectNameGeneratedFrom generatedFrom)
        {
            var PROC_NAME = string.Format( "RelationshipExtentions.GenerateObjectName(entity='{0}', FKName='{1}')", entity?.GetType().GetProperty("TableName")?.GetValue(entity), FKName);

            string FieldName = "";
            try
            {
                var relationshipGroups = entity?.GetType().GetProperty("RelationshipGroups")?.GetValue(entity) as IDictionary<string, CoreInterfaces.IRelationshipList>;
                var relationship = relationshipGroups?[FKName];
                var asSummaryMethod = relationship?.GetType().GetMethod("AsSummary");
                var relGroupSummary = asSummaryMethod?.Invoke(relationship, null) as dynamic;
                var entityName = entity?.GetType().GetProperty("DatabaseSchema")?.GetValue(entity) + "." + entity?.GetType().GetProperty("TableName")?.GetValue(entity);

                int SameTableCount = 0;
                foreach (var rg in relationshipGroups?.Values ?? Array.Empty<CoreInterfaces.IRelationshipList>())
                    if ((rg?.GetType().GetMethod("AsSummary")?.Invoke(rg, null) as dynamic)?.ToTableName?.Equals(relGroupSummary?.ToTableName) == true) SameTableCount++;
                string ToTableNameSingular = relGroupSummary?.ToTableName?.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "")?.ToSingular() ?? "";

                var AltName = ToTableNameSingular;
                if (relGroupSummary?.FromTableName?.Equals(relGroupSummary?.ToTableName) == true) generatedFrom = ObjectNameGeneratedFrom.ToUniqueColumnName;
                switch (generatedFrom)
                {
                    case ObjectNameGeneratedFrom.JoinFromColumnName:
                        AltName = string.Join(",", relGroupSummary?.FromColumnName ?? Array.Empty<string>());
                        break;
                    case ObjectNameGeneratedFrom.ToUniqueColumnName:
                        var toUniqueColumnNameMethod = relGroupSummary?.GetType().GetMethod("ToUniqueColumnName");
                        AltName = toUniqueColumnNameMethod?.Invoke(relGroupSummary, new object[] { false })?.ToString() ?? "";
                        break;
                    case ObjectNameGeneratedFrom.JoinToColumnName:
                        AltName = string.Join(",", relGroupSummary?.ToColumnName ?? Array.Empty<string>());
                        break;
                }

                var properties = entity?.GetType().GetProperty("Properties")?.GetValue(entity) as IDictionary<string, CoreInterfaces.IProperty>;
                FieldName = (((properties?.ContainsKey(ToTableNameSingular) == true)
                                     || (entityName == relGroupSummary?.ToTableName)
                                     || (SameTableCount > 1))
                                        ? AltName : ToTableNameSingular);

            } catch (Exception ex)
            {
                throw new Exception(PROC_NAME + ".  " + ex.Message);
            }
            return FieldName;
        }
        /// <summary>
        /// Starting from a relationship, function will figure out what the name of the object should be for a foriegn key, taking into account the potential names 
        /// </summary>
        /// <param name="This">The FK to generate the object from</param>
        /// <param name="entity">Entity So we know the perspective of the relationship</param>
        /// <returns></returns>
        public static string GenerateObjectName(this IRelationship This, IEntity entity, ObjectNameGeneratedFrom generatedFrom)
        {
            var FKName = This.ConstraintName;
            return entity.GenerateObjectName(FKName, generatedFrom);

        }
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
            foreach (var rel in This.ParentEntity.Relationships) if (rel.ToPropertyName == This.ToPropertyName) targetColumnNameCount++;
            foreach (var rel in This.ParentEntity.Relationships) if (rel.FromPropertyName == This.FromPropertyName) fromColumnNameCount++;
            if (targetColumnNameCount == 1) return This.ToPropertyName;
            if (fromColumnNameCount == 1) return This.FromPropertyName;
            throw new Exception(string.Format("RelationshipExtentions.ToUniqueColumnName: Could not find any unique column names to write to :( {0} or {1} for {2}", This.ToColumnName, This.FromColumnName, This.ConstraintName));
        }
    
        /// <summary>
        /// This will return the target column name.  this is important because we do not want to return the column name of the current table, but rather that 
        /// column it is pointing to 
        /// </summary>
        /// <param name="This"></param>
        /// <param name="UseFromAsDefault">Use From Table name as an alias, otherwise use the ToTableAlias</param>
        /// <returns></returns>
        public static string ToUniqueColumnName(this RelationshipSummary This, bool? UseFromAsDefault = null)
        {
            var targetColumnNameCount = 0;
            var fromColumnNameCount = 0;
            if ((This.FromColumnName.Count==2) && (This.ToColumnName.Count == 2) && (This.FromTableName.Equals(This.ToTableName)))
            {
                //This relationship is self referencing,  I am sure this will cause an issue with compound self referencing keys,  lets cross the bridge when we get there 
                return This.ToColumnName[0];
            }
            //Try to see if a combination of names will match a column name that already exists
            foreach (var rel in This.Entity.Relationships) if (rel.ToColumnName == string.Join("", This.ToColumnName)) targetColumnNameCount++;
            foreach (var rel in This.Entity.Relationships) if (rel.FromColumnName == string.Join("", This.FromColumnName)) fromColumnNameCount++;
            if (targetColumnNameCount == 1) return string.Join("", This.ToColumnName).Trim();
            if (fromColumnNameCount == 1) return string.Join("", This.FromColumnName).Trim();
            //If we didn't find one,  then use the target object name
            fromColumnNameCount = 0;
            targetColumnNameCount = 0;
            var FromTableAlias = This.Entity.ParentDatabase[This.FromTableName].TableAlias;
            var ToTableAlias = This.Entity.ParentDatabase[This.ToTableName].TableAlias;
            foreach (var rel in This.Entity.Relationships) if (rel.ToColumnName == ToTableAlias) targetColumnNameCount++;
            foreach (var rel in This.Entity.Relationships) if (rel.FromColumnName == FromTableAlias) fromColumnNameCount++;
            if (targetColumnNameCount == 0) return ToTableAlias.Trim();
            if (fromColumnNameCount == 0) return FromTableAlias.Trim();
            if (!UseFromAsDefault.HasValue) throw new Exception(string.Format("RelationshipExtentions.ToUniqueColumnName: Could not find any unique column names to write to :( {0} or {1} for {2}", This.ToColumnName, This.FromColumnName, This.ConstraintName));
            if (UseFromAsDefault.Value) return FromTableAlias.Trim();
            return ToTableAlias.Trim();
        }
        /// <summary>
        /// Returns a list of Object names with counts to check and see if that object will exist
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Dictionary<string, int> ObjectNameCounts(this IEntity entity) {
            var ret = new Dictionary<string, int>();
            foreach(var FKName in entity.RelationshipGroups.Keys)
            {
                var relGroupSummary = entity.RelationshipGroups[FKName].AsSummary();
                var objectName = relGroupSummary.AsObjectPropertyNameAttempt();
                if (!ret.ContainsKey(objectName)) ret.Add(objectName, 0);
                ret[objectName]++;
            }
            return ret;
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
            return relGroupSummary.AsObjectPropertyName(true);
        }


        /// <summary>
        /// This will return the target column name.  this is important because we do not want to return the column name of the current table, but rather that 
        /// column it is pointing to 
        /// </summary>
        /// <param name="This">The Context Relationship</param>
        /// <param name="ContextSchemaObjectName">The parent with the column name you don't want</param>
        /// <returns></returns>
        public static string AsObjectPropertyName(this RelationshipSummary relGroupSummary, bool CheckForObjectNameExistance)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.AsObjectPropertyName('FKName={0}')", relGroupSummary.ConstraintName);
            var ToObjectFieldName = "";
            try
            {
                ToObjectFieldName = relGroupSummary.AsObjectPropertyNameAttempt();

                var objectNameList = new Dictionary<string, int>();
                if (CheckForObjectNameExistance) objectNameList = relGroupSummary.Entity.ObjectNameCounts();
                if ((objectNameList.ContainsKey(ToObjectFieldName)) && (objectNameList[ToObjectFieldName] > 1))
                {
                    //If we already have this object name, then lets try to see if we can make a unique name with the Column names
                    ToObjectFieldName = ToObjectFieldName + string.Join(",", relGroupSummary.ToColumnName).ToCsObjectName();
                }

                if ((objectNameList.ContainsKey(ToObjectFieldName)) && (objectNameList[ToObjectFieldName] > 1))
                {
                    throw new Exception(string.Format("Object name {0} already exists in the object list :(", ToObjectFieldName));
                }
                return ToObjectFieldName;
            }
            catch (Exception ex)
            {
                return string.Format("/* ERROR: {0} */", string.Format("{0}: {1}", PROC_NAME, ex.Message));
            }

        }
        /// <summary>
        /// This will return the target column name.  this is important because we do not want to return the column name of the current table, but rather that 
        /// column it is pointing to.  this DOES NOT check and see if the item already exists.
        /// </summary>
        /// <param name="This">The Context Relationship</param>
        /// <param name="ContextSchemaObjectName">The parent with the column name you don't want</param>
        /// <returns></returns>
        private static string AsObjectPropertyNameAttempt(this RelationshipSummary relGroupSummary)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.AsObjectPropertyNameAttempt('FKName={0}')", relGroupSummary.ConstraintName);
            var ToObjectFieldName = "";
            try
            {
                var entity = relGroupSummary.Entity;
                var RelationshipObjectNameList = new List<string>();
                string ToTableName = entity.ParentDatabase.Entities[relGroupSummary.ToTableName].TableAlias;
                ToObjectFieldName = ToTableName.ToCsObjectName();
                //string ToObjectFieldName = relGroupSummary.ToUniqueColumnName().ToCsObjectName();
                var CountOfThisEntityInRelationships = relGroupSummary.Entity.Relationships.CountItems(CoreEnums.RelationSearchField.ToTableName, relGroupSummary.ToTableName);
                if (CountOfThisEntityInRelationships > 1) { 
                    //ToObjectFieldName = ToTableName + string.Join(",", relGroupSummary.ToColumnName).ToCsObjectName();
                    ToObjectFieldName = ((relGroupSummary.MultiplicityType.EndsAsMany() ?
                                relGroupSummary.ToUniqueColumnName().ToPlural() :
                                string.Join(",", relGroupSummary.ToColumnName) + Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix)
                            ).ToCsObjectName();
                }
                else
                {
                    ToObjectFieldName = ((relGroupSummary.MultiplicityType.EndsAsMany() ?
                                    relGroupSummary.ToUniqueColumnName().ToPlural() :
                                    ToObjectFieldName + Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix)
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
            var entityName = entity.TableName;
            var FieldName = "";
            try
            {

                var objectSuffix = "";
                var PreviousFields = new List<string>();
                //var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne);
                foreach (var relationshipGroup in entity.RelationshipGroups)
                {
                    var relationship = relationshipGroup.Value.AsSummary();
                    if (relationship.ConstraintName.StartsWith("FK_FracFleets_FracFleets"))
                        relationship.ConstraintName += "";
                    //Need to resolve the to table name to what the alias table name is
                    string ToTableName = entity.ParentDatabase.Entities[relationship.ToTableName].TableAlias;
                    int AdditionSameTableCount = 0; //If there is another foriegn key that targets entity,  then we will have colliding names,  lets try to get the name from the target column name so it is clearer
                    foreach (var regGroup in entity.RelationshipGroups)
                    {
                        if (regGroup.Key.StartsWith("FK_Wells_CompletionDesigns_CompletionDesignId"))
                            relationship.ConstraintName += "";
                        if ((regGroup.Key != relationship.ConstraintName) && (regGroup.Value.AsSummary()?.ToTableName == relationship.ToTableName)) AdditionSameTableCount++;
                    }
                    string ToTableNameSingular = EzDbCodeGen.Core.Extensions.StringExtensions.ToSingular(ToTableName);
                    FieldName = ((PreviousFields.Contains(ToTableNameSingular)
                                         || (entity.Properties.ContainsKey(ToTableNameSingular))
                                         || (entityName == relationship.ToTableName)
                                         || (AdditionSameTableCount > 0)) 
                                            ? relationship.ToUniqueColumnName() : ToTableNameSingular).ToCsObjectName();
                    PreviousFields.Add(FieldName);
                    objectSuffix = Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix;

                    if (fkNametoSelect == relationship.ConstraintName)
                    {
                        return (FieldName + objectSuffix).Trim();
                    }
                }
                return string.Format("/* {0} */", fkNametoSelect).Trim();
            }
            catch (Exception ex)
            {
                return string.Format("/* ERROR: {0} */", string.Format("{0}: Error while figuring out the correct class name for this foriegn Key.  {1}", PROC_NAME, ex.Message));
            }
        }
        /// <summary>
        /// Used to figure out what the target object name for the end of this particular relationship
        /// </summary>
        /// <param name="thisRelationship">The this relationship.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EndAsObjectPropertyName(this IRelationship thisRelationship)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.ToObjectPropertyName('{0}')", thisRelationship.ConstraintName);
            return EndAsObjectPropertyName(thisRelationship.ConstraintName, thisRelationship.ParentEntity);
        }

        /// <summary>
        /// Used to figure out what the target object name for the end of this particular relationship
        /// </summary>
        /// <param name="thisRelationship">The this relationship.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string EndAsObjectPropertyName(this RelationshipSummary thisRelationship)
        {
            var PROC_NAME = string.Format("RelationshipExtentions.ToObjectPropertyName('{0}')", thisRelationship.ConstraintName);
            return EndAsObjectPropertyName(thisRelationship.ConstraintName, thisRelationship.Entity);
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
                nameList += (nameList.Length > 0 ? ", " : "") + rel.ConstraintName;
            }
            return nameList;
        }

        public static CoreInterfaces.IRelationshipList FindItems(this CoreInterfaces.IRelationshipList This, CoreEnums.RelationSearchField searchField, string searchFor)
        {
            var list = new RelationshipList();
            foreach (var item in This)
            {
                if ((searchField == CoreEnums.RelationSearchField.ToTableName) && (item.ToTableName == searchFor)) list.Add(item);
                else if ((searchField == CoreEnums.RelationSearchField.ToColumnName) && (item.ToColumnName == searchFor)) list.Add(item);
                else if ((searchField == CoreEnums.RelationSearchField.ToFieldName) && (item.ToPropertyName == searchFor)) list.Add(item);
                else if ((searchField == CoreEnums.RelationSearchField.FromTableName) && (item.FromTableName == searchFor)) list.Add(item);
                else if ((searchField == CoreEnums.RelationSearchField.FromFieldName) && (item.FromPropertyName == searchFor)) list.Add(item);
                else if ((searchField == CoreEnums.RelationSearchField.FromColumnName) && (item.FromColumnName == searchFor)) list.Add(item);
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
                if (ret.Database == null) ret.Database = relationship.ParentEntity?.ParentDatabase;
                if (!ret.ContainsKey(relationship.ConstraintName))
                {
                    ret.Add(relationship.ConstraintName, new RelationshipList());
                }
                ret[relationship.ConstraintName].Add(relationship);
            }
            return ret;
        }

        public static string AsString( this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.ManyToOne: return "*->1";
                case CoreEnums.RelationshipMultiplicityType.ManyToZeroOrOne: return "*->0|1";
                case CoreEnums.RelationshipMultiplicityType.OneToMany: return "1->*";
                case CoreEnums.RelationshipMultiplicityType.OneToOne: return "1->1";
                case CoreEnums.RelationshipMultiplicityType.OneToZeroOrOne: return "1->0|1";
                case CoreEnums.RelationshipMultiplicityType.Unknown: return "??";
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany: return "0|1->*";
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToOne: return "0|1->1";
            }
            return "??!";
        }


        public static bool EndsAsMany(this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.OneToMany:
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany:
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

        public static bool BeginsAsMany(this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.ManyToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.ManyToZeroOrOne: return true;
            }
            return false;
        }

        public static bool BeginsAsOne(this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.OneToMany: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToZeroOrOne: return true;
            }
            return false;
        }
        public static bool BeginsAsZeroOrOne(this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.OneToMany: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToZeroOrOne: return true;
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany: return false;
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToOne: return true;
            }
            return false;
        }

        public static bool EndsAsOne(this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.OneToMany: return false;
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany: return false;
                case CoreEnums.RelationshipMultiplicityType.ManyToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.ManyToZeroOrOne: return false;
                case CoreEnums.RelationshipMultiplicityType.OneToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToZeroOrOne: return false;
                case CoreEnums.RelationshipMultiplicityType.Unknown: return false;
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToOne: return true;
            }
            return false;
        }

        public static bool EndsAsZeroOrOne(this CoreEnums.RelationshipMultiplicityType multiplicityType)
        {
            switch (multiplicityType)
            {
                case CoreEnums.RelationshipMultiplicityType.OneToMany: return false;
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany: return false;
                case CoreEnums.RelationshipMultiplicityType.ManyToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.ManyToZeroOrOne: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToOne: return true;
                case CoreEnums.RelationshipMultiplicityType.OneToZeroOrOne: return true;
                case CoreEnums.RelationshipMultiplicityType.Unknown: return false;
                case CoreEnums.RelationshipMultiplicityType.ZeroOrOneToOne: return true;
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
                            ret.Entity = relationship.ParentEntity;
                            ret.FromTableName = relationship.FromTableName;
                            ret.ConstraintName = relationship.ConstraintName;
                            ret.ToTableName = relationship.ToTableName;
                            ret.PrimaryTableName = relationship.PrimaryTableName;
                            ret.MultiplicityType = relationship.GetRelationshipMultiplicityType();
                            ret.Type = relationship.RelationshipType;
                        }
                        else
                        {
                            var isValidMultiplicty = (
                                (
                                    ((ret.MultiplicityType == CoreEnums.RelationshipMultiplicityType.ManyToZeroOrOne) || (ret.MultiplicityType == CoreEnums.RelationshipMultiplicityType.ManyToOne)) &&
                                    ((relationship.GetRelationshipMultiplicityType() == CoreEnums.RelationshipMultiplicityType.ManyToOne) || (relationship.GetRelationshipMultiplicityType() == CoreEnums.RelationshipMultiplicityType.ManyToZeroOrOne))
                                )
                                || 
                                (
                                    ((ret.MultiplicityType == CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany) || (ret.MultiplicityType == CoreEnums.RelationshipMultiplicityType.OneToMany)) &&
                                    ((relationship.GetRelationshipMultiplicityType() == CoreEnums.RelationshipMultiplicityType.OneToMany) || (relationship.GetRelationshipMultiplicityType() == CoreEnums.RelationshipMultiplicityType.ZeroOrOneToMany)
                                        || (relationship.GetRelationshipMultiplicityType() == CoreEnums.RelationshipMultiplicityType.ZeroOrOneToOne))
                                )
                            );
                            if (ret.MultiplicityType != relationship.GetRelationshipMultiplicityType())
                            {
                                Console.WriteLine(string.Format(@"Multiplicity for FK {0} mismatched but are valid warning ({1} vs {2}): 
 FromTableName:{3}, ToTableName:{4}, PrimaryTableName:{5}",
                                relationship.ConstraintName, ret.MultiplicityType.ToString(), relationship.GetRelationshipMultiplicityType().ToString(), relationship.FromTableName,
                                            relationship.ToTableName, relationship.PrimaryTableName));
                                ret.MultiplicityTypeWarning = true;
                            }
                            if (!(
                            (ret.FromTableName == relationship.FromTableName) &&
                            (ret.ConstraintName == relationship.ConstraintName) &&
                            (ret.ToTableName == relationship.ToTableName) &&
                            (isValidMultiplicty) &&
                            (ret.PrimaryTableName == relationship.PrimaryTableName)))
                                throw new Exception(string.Format(@"Relationship List is not grouped! 
 FromTableName:{0}={1},  Name:{2}={3},  
 ToTableName:{4}={5}, MultiplicityType{6}={7},
 PrimaryTableName:{8}={9}", ret.FromTableName, relationship.FromTableName, ret.ConstraintName, relationship.ConstraintName,
                                            ret.ToTableName, relationship.ToTableName, ret.MultiplicityType.ToString(), relationship.GetRelationshipMultiplicityType().ToString(), ret.PrimaryTableName, relationship.PrimaryTableName));
                        }
                        ret.ToColumnName.Add(relationship.ToColumnName);
                        ret.ToPropertyName.Add(relationship.ToPropertyName);
                        if (relationship.ParentEntity.ParentDatabase.ContainsKey(relationship.ToTableName))
                        {
                            var ToProperty = relationship.ParentEntity.ParentDatabase[relationship.ToTableName].Properties[relationship.ToPropertyName];
                            ret.ToColumnProperties.Add(ToProperty);
                            ret.ToObjectPropertyName.Add(ToProperty.AsObjectPropertyName());

                            var FromProperty = !relationship.ParentEntity.ParentDatabase.ContainsKey(relationship.ToTableName) ? null : relationship.ParentEntity.ParentDatabase[relationship.FromTableName].Properties[relationship.FromPropertyName];
                            ret.FromColumnProperties.Add(FromProperty);
                            ret.FromObjectPropertyName.Add(FromProperty?.AsObjectPropertyName() ?? string.Empty);
                        }
                        ret.FromColumnName.Add(relationship.FromColumnName);
                        ret.FromPropertyName.Add(relationship.FromPropertyName);
                        ret.MultiplicityTypes.Add(relationship.GetRelationshipMultiplicityType());
                        ret.Types.Add(relationship.RelationshipType);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Relationship {0} error!", relationship.ConstraintName), ex);
                    }

                }
            }
            return ret;
        }
    }

    public enum ObjectNameGeneratedFrom
    {
        JoinFromColumnName,
        ToUniqueColumnName,
        JoinToColumnName,
        ForeignKeyName,
        TableName,
        SchemaAndTableName,
        SchemaAndTableNameWithUnderscore
    }
}
