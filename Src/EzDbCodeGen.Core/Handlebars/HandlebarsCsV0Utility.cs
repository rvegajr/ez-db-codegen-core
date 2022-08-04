using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using EzDbCodeGen.Core.Extentions.Objects;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core;
using System.Runtime.CompilerServices;
using EzDbSchema.Core.Objects;
using System.Text;
using System.Diagnostics;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core
{
    internal static class HandlebarsCsV0Utility
    {
        public static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("POCOModelPropertyAttributesV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelPropertyAttributesV0')";
                var entityName = "";
                try
                {
                    var prefix = parameters.AsString(0);
                    var property = (IProperty)context.Value;
                    var entity = property.Parent;
                    entityName = entity.Schema + "." + entity.Name;
                    var decimalAttribute = "";
                    var keyAttribute = "";
                    var fkAttributes = "";
                    var identityAttribute = "";
                    var columnAttribute = "";

                    if ((entityName.Contains("dbo.Wells")) && (property.Name.Contains("Shape")))
                        entityName = (entityName + " ").Trim();
                    if (entityName.Contains("AdHocTankBatteryArea")) //&& (property.Name.Contains("")))
                        entityName = (entityName + " ").Trim();
                    if (property.Type == "decimal")
                    {
                        decimalAttribute = $"\n{prefix}[DecimalPrecision({property.Precision}, {property.Scale})]";
                    }
                    if ((!property.IsIdentity) && (entity.Type == "TABLE"))
                    {
                        identityAttribute = ", DatabaseGenerated(DatabaseGeneratedOption.None)";
                    }
                    if (property.IsKey)
                    {
                        if (entity.PrimaryKeys.Count > 1)
                        {
                            keyAttribute = $"\n{prefix}[Key, Column(Order={property.KeyOrder}){identityAttribute}]";
                        }
                        else
                        {
                            keyAttribute = $"\n{prefix}[Key{identityAttribute}]";
                        }
                    }
                    if (property.RelatedTo.Count > 0)
                    {
                        foreach (var FKKeyValue in property.RelatedTo.GroupByFKName())
                        {
                            if (FKKeyValue.Key.Equals("FK_FracFleets_FracFleets"))
                                entityName = (entityName + " ").Trim();
                            var relGroupSummary = entity.RelationshipGroups[FKKeyValue.Key].AsSummary();
                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;

                            var FieldName = entity.GenerateObjectName(FKKeyValue.Key, ObjectNameGeneratedFrom.JoinFromColumnName);
                            //We only will write the ForeignKey if we compound FKs or multiple columns with references to the same table
                            //if ((SameTableCount>1) || (relGroupSummary.ToColumnName.Count>1))
                            var isOneToOneRelation = (relGroupSummary.MultiplicityType == RelationshipMultiplicityType.OneToOne) || (relGroupSummary.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne);
                            var isCompositeKey = ((relGroupSummary.ToColumnName.Count > 1) && (!relGroupSummary.FromTableName.Equals(relGroupSummary.ToTableName)));
                            if (isOneToOneRelation || isCompositeKey)
                            {
                                var ColumnOrder = "";
                                if (relGroupSummary.ToColumnName.Count > 1)
                                {
                                    ColumnOrder = string.Format(", Column(Order = {0})", relGroupSummary.ToColumnProperties.Where(p => p.Name.Equals(property.Name)).Select(p => p.KeyOrder).FirstOrDefault());
                                }
                                if (isOneToOneRelation) fkAttributes += string.Format("\n{0}[ForeignKey(\"{1}\"){2}]", prefix, (FieldName.Replace(" ", "") + Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix).Trim(), ColumnOrder);
                            }
                            if (fkAttributes.Length > 0) break;

                        }
                    }
                    var configEntityList = Internal.AppSettings.Instance.Configuration.FindMatchingConfigEntities(entityName).ToList().FirstOrDefault();
                    if (configEntityList != null)
                    {
                        var FieldOverride = configEntityList.Overrides.Fields.Find(f => f.FieldName.ToLower().Equals(property.Name.ToLower()));
                        if ((FieldOverride != null) && (!string.IsNullOrEmpty(FieldOverride.ColumnAttributeTypeName)))
                            columnAttribute = string.Format("[Column(TypeName=\"{0}\")]", FieldOverride.ColumnAttributeTypeName);
                    }

                    if (keyAttribute.Length > 0) writer.WriteSafeString(keyAttribute);
                    if (fkAttributes.Length > 0) writer.WriteSafeString(fkAttributes);
                    if ((property.Name == "SysStartTime") || (property.Name == "SysEndTime")) writer.WriteSafeString("[DatabaseGenerated(DatabaseGeneratedOption.Computed)]\n");
                    if (decimalAttribute.Length > 0) writer.WriteSafeString(decimalAttribute);
                    if (columnAttribute.Length > 0) writer.WriteSafeString(columnAttribute);
                    writer.WriteSafeString($"\n{prefix}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }

            });

            Handlebars.RegisterHelper("POCOModelFKConstructorInitV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKConstructorInitV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    var PreviousOneToManyFields = new List<string>();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToMany).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();
                            string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias.ToSingular();
                            //string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();
                            string ToObjectFieldName = string.Format("{0}_{1}", relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", ""), relGroupSummary.ToColumnName.First());

                            writer.WriteSafeString(string.Format(
                                "\n{0}this.{1} = new HashSet<{2}>(); //{3} 0|1->*"
                                , prefix, ToObjectFieldName, ToTableName, relGroupSummary.Name));
                            PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKPropertiesV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKPropertiesV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    if (entity.Name.Contains("ProjectSubTypes"))
                        entity.Name = entity.Name + "";
                    List<string> PreviousOneToManyFields = new List<string>();
                    PreviousOneToManyFields.Clear();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToMany).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (fkName.Equals("FK_AreaTargetFormations_AreaTypes")) {
                            fkName = (fkName + " ").Trim();
                        }
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();
                            var targetTableExists = entity.Parent.Entities.ContainsKey(relGroupSummary.ToTableName);
                            string ToTableName = targetTableExists ? entity.Parent.Entities[relGroupSummary.ToTableName].Alias.ToSingular() : relGroupSummary.ToTableName;
                            //string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();
                            string ToObjectFieldName = string.Format("{0}_{1}", relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", ""), relGroupSummary.ToColumnName.First());

                            //Check and see we have multiple declarations of the same table,  if we do, we will need an inverse 
                            // property annotation figure out how to property find the correct object property target 
                            var inversePropertyAttribute = "";
                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                            if ((targetTableExists) && (SameTableCount > 1))
                            {
                                var toGroupRelationshipList = entity.Parent[relGroupSummary.ToTableName].Relationships.GroupByFKName();
                                if (!toGroupRelationshipList.ContainsKey(fkName)) throw new Exception(string.Format("The inverse of FK {0} ({1}->{2})", fkName, relGroupSummary.FromTableName, relGroupSummary.ToTableName));
                                var inverseOfThisFK = toGroupRelationshipList[fkName];
                                var relGroupSummaryInverse = inverseOfThisFK.AsSummary();
                                var InversePropertyNamePotential = relGroupSummaryInverse.EndAsObjectPropertyName();
                                if (InversePropertyNamePotential.Length > 0) inversePropertyAttribute = string.Format("[InverseProperty(\"{0}\")]", InversePropertyNamePotential);
                            }

                            if (!targetTableExists) writer.WriteSafeString($"\n\n{prefix}/* NOTE: Target Table {ToTableName} was set to ignore in config file");
                            writer.WriteSafeString(string.Format("\n{0}//<summary>{1} {2}</summary>", prefix, relGroupSummary.Name, relGroupSummary.MultiplicityType.AsString()));
                            writer.WriteSafeString(string.Format("\n{0}[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Usage\", \"CA2227: CollectionPropertiesShouldBeReadOnly\")]", prefix));
                            if (inversePropertyAttribute.Length > 0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, inversePropertyAttribute));
                            writer.WriteSafeString(string.Format("\n{0}public virtual ICollection<{1}> {2} {{ get; set; }}", prefix, ToTableName, ToObjectFieldName));
                            if (!targetTableExists) writer.WriteSafeString($"\n{prefix}*/");
                            writer.WriteSafeString($"\n{prefix}");
                            PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("POCOModelFKManyToZeroToOneV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOneV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;
                    if (entityName.Equals("dbo.ProjectSubTypes"))
                        entityName = entityName + "";
                    List<string> PreviousManyToOneFields = new List<string>();

                    var FKToUse = new List<string>();
                    FKToUse.AddRange(entity.Relationships.Fetch(RelationshipMultiplicityType.ManyToZeroOrOne).Select(s => s.Name).ToList());
                    FKToUse.AddRange(entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne).Select(s => s.Name).ToList());
                    FKToUse = FKToUse.Distinct().ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if ((fkName.Equals("FK_AreaTargetFormations_AreaTypes")) || (fkName.Equals("FK_AreaTargetFormations___TargetFormations1")))
                            fkName = fkName + "";
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();
                            if (relGroupSummary.MultiplicityType.Equals(RelationshipMultiplicityType.OneToOne))
                            {
                                //Ignore one to one as it will be handled through POCOModelFKOneToOneV0 
                            } else
                            {
                                int SameTableCount = 0;
                                foreach (var rg in entity.RelationshipGroups.Values)
                                    if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                                string ToTableNameSingular = relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular();
                                string _FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular)
                                                     || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                     || (entityName == relGroupSummary.ToTableName)
                                                     || (SameTableCount > 1))
                                                        ? string.Join(", ", relGroupSummary.FromColumnName) : ToTableNameSingular);

                                string FieldName = entity.GenerateObjectName(fkName, ObjectNameGeneratedFrom.JoinFromColumnName);
                                var oldPrefix = prefix;
                                if (relGroupSummary.FromFieldName.Count>1)
                                {
                                    prefix += "// ";
                                    writer.WriteSafeString(string.Format("\n{0} WARNING: Legacy Code generator cannot handle composite Foriegn Keys... commenting out", prefix, fkName));
                                }
                                writer.WriteSafeString(string.Format("\n{0}/// <summary>{1}  *->0|1</summary>", prefix, fkName));
                                writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, string.Join(", ", relGroupSummary.FromFieldName)));
                                writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
                                writer.WriteSafeString("\n");
                                PreviousManyToOneFields.Add(FieldName);
                                prefix = oldPrefix;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKOneToOneV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKOneToOneV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;
                    if (entityName.Equals("dbo.ProjectSubTypes"))
                        entityName = entityName + "";

                    var PreviousOneToOneFields = new List<string>();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.OneToOne).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if ((fkName.Equals("FK_TankBatteries_AssetTeams")) || (fkName.Equals("FK_TankBatteries_AssetTeams")))
                            fkName = fkName + "";
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();

                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                            string ToTableNameSingular = relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular();
                            string FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular)
                                                 || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                 || (entityName == relGroupSummary.ToTableName)
                                                 || (SameTableCount > 1))
                                                    ?  string.Join(",", relGroupSummary.FromColumnName) : ToTableNameSingular);
                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} 1->1</summary>", prefix, relGroupSummary.Name));
                            writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, string.Join(", ", relGroupSummary.FromObjectPropertyName)));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
                            writer.WriteSafeString("\n");
                            PreviousOneToOneFields.Add(FieldName);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            

            Handlebars.RegisterHelper("POCOModelFKZeroOrOneToOneV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKZeroOrOneToOneV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;


                    if (entityName.Equals("dbo.ProjectSubTypes"))
                    {
                        entityName = (entityName + " ").Trim();
                    }
                    var PreviousOneToOneFields = new List<string>();
                    //Search for all foreign keys that contain a Zero or one to one... 
                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne).Select(s => s.Name).ToList();
                    foreach (var rg in entity.RelationshipGroups.Values)
                        if (rg.AsSummary().ToColumnName.Count > 1) FKToUse.Add(rg.AsSummary().Name);

                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (fkName.Contains("FK_AreaTargetFormations_AreaTypes"))
                        {
                            entityName = (entityName + " ").Trim();
                        }
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value.AsSummary();
                            int SameTableCount = 0;
                            foreach(var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relationship.ToTableName)) SameTableCount++;
                            string ToTableNameSingular = relationship.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular();
                            string _FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular)
                                                 || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                 || (entityName == relationship.ToTableName)
                                                 || (SameTableCount > 1))
                                                    ? relationship.ToUniqueColumnName() : ToTableNameSingular);
                            string FieldName = entity.GenerateObjectName(fkName, ObjectNameGeneratedFrom.ToUniqueColumnName);
                            string ForeignKey = ((entityName == relationship.ToTableName) ? relationship.FromFieldName[relationship.FromFieldName.Count-1] : string.Join(", ", relationship.FromFieldName));

                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} {2}</summary>", prefix, relationship.Name, relationship.MultiplicityType.AsString()));

                            /* New work based on using EF docs to figure out fk attributes - https://www.entityframeworktutorial.net/code-first/foreignkey-dataannotations-attribute-in-code-first.aspx
                             * Specifically - [ForeignKey] on the navigation property in the dependent entity */
                            var fkList = new List<string>();
                            var WriteFKAttribute = false;
                            var IsRequired = false;
                            var AttributeText = new StringBuilder();
                            foreach (Relationship fk in fkNameKV.Value)
                            {
                                //If this is a one to one relationship,  then we will apply the ForiegnKey Attribute to Primary Key of this object
                                if ((fk.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne) || (fk.MultiplicityType == RelationshipMultiplicityType.OneToOne))
                                {

                                    if (fk.ToTableName.Equals(fk.FromTableName))
                                    {
                                        fk.FromFieldName += "";
                                    }
                                    //We want to grab the field that does not equal the current table, but in those case swhere the reference points to itself,  then we need to assume the From End of the Relationship data
                                    var TargetFieldName = (entityName.Equals(fk.FromTableName)) && (!fk.ToTableName.Equals(fk.FromTableName)) ? fk.ToFieldName : fk.FromFieldName;
                                    var SourceFieldName = (entityName.Equals(fk.FromTableName)) && (!fk.ToTableName.Equals(fk.FromTableName)) ? fk.FromFieldName : fk.ToFieldName;
                                    var TargetEntityPrimaryKeys = (entityName.Equals(fk.FromTableName) && (!fk.ToTableName.Equals(fk.FromTableName))) ? fk.ToEntity.PrimaryKeys : fk.FromEntity.PrimaryKeys;
                                    var FKFieldExistsOnTargetTablePKs = TargetEntityPrimaryKeys.Any(c => c.Name.Equals(TargetEntityPrimaryKeys));
                                    if (!IsRequired)
                                        IsRequired = ((entityName.Equals(fk.FromTableName)) && (!fk.ToTableName.Equals(fk.FromTableName))) ? !((Property)fk.FromProperty).IsNullableResolved() : !((Property)fk.ToProperty).IsNullableResolved();
                                    //Because this field exists in the target entity primary keys, then convvention should automatically know to resolve the foriegn key, but if 
                                    // it DOES NOT exist,  then we will need to write the key attribute.. however
                                    /// if this is a one to one relationship to the same table, we will apply the ForiegnKey atttribute to the primary key to the object
                                    if (!FKFieldExistsOnTargetTablePKs) WriteFKAttribute = true;
                                    fkList.Add(SourceFieldName);
                                }
                                //Determine wich end of the relationship to use, since we need the target, choose the end that does not equal this entities table name (schame.table)
                            }
                            if (!relationship.MultiplicityType.EndsAsOne())
                            {
                                writer.WriteSafeString(string.Format("\n{0}////WARNING:  There are multiple relationship groups with the same name but different multiplicities.. skipping this one...", prefix));
                            }
                            else
                            {
                                if (WriteFKAttribute) AttributeText.Append(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, string.Join(", ", fkList.ToList())));
                                if (IsRequired) AttributeText.Append("[Required]");
                                if (AttributeText.Length > 0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, AttributeText.ToString()));
                                writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relationship.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
                            }
                            writer.WriteSafeString($"\n{prefix}");
                            PreviousOneToOneFields.Add(FieldName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            Handlebars.RegisterHelper("UnitTestsRenderExtendedEndpointsV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('UnitTestsRenderExtendedEndpointsV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;

                    var PreviousManyToOneFields = new List<string>();
                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ManyToZeroOrOne).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                        }
                    }


                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipMultiplicityType.ManyToZeroOrOne);
                    foreach (IRelationship relationship in RelationshipsManyToOne)
                    {
                        int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema+".", "").ToSingular();
                        string ExpandColumnName = ((PreviousManyToOneFields.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}using (var response = await HttpClient.GetAsync(\"http://testserver/api/{1}?%24expand={2}<t/>&%24top=10\")) ", prefix, entityName, ExpandColumnName));
                        writer.WriteSafeString(string.Format("\n{0}{{ ", prefix));
                        writer.WriteSafeString(string.Format("\n{0}    var result = await response.Content.ReadAsStringAsync(); ", prefix));
                        writer.WriteSafeString(string.Format("\n{0}    Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, \"Return Get 10 or less {1} with Expand of {2}. \" + result); ", prefix, entityName, ExpandColumnName));
                        writer.WriteSafeString(string.Format("\n{0}}} ", prefix));
                        PreviousManyToOneFields.Add(ExpandColumnName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
        }
    }
}