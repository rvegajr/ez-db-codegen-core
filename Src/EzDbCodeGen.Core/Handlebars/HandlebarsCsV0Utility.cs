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
                    var property = (IProperty)context;
                    var entity = property.Parent;
                    entityName = entity.Schema + "." + entity.Name;
                    var decimalAttribute = "";
                    var keyAttribute = "";
                    var fkAttributes = "";
                    var identityAttribute = "";

                    if ((entityName.Contains("dbo.WellStickSurveys")) && (property.Name.Contains("AreaTypeId")))
                        entityName = (entityName + " ").Trim();
                    if (entityName.Equals("dbo.Scenarios")) //&& (property.Name.Contains("")))
                        entityName = (entityName + " ").Trim();
                    if (property.Type == "decimal")
                    {
                        decimalAttribute = "[DecimalPrecision(" + property.Precision + ", " + property.Scale + ")]";
                    }
                    if ((!property.IsIdentity) && (entity.Type == "TABLE"))
                    {
                        identityAttribute = ", DatabaseGenerated(DatabaseGeneratedOption.None)";
                    }
                    if (property.IsKey)
                    {
                        if (entity.PrimaryKeys.Count > 1)
                        {
                            keyAttribute = @"[Key, Column(Order=" + property.KeyOrder + ")" + identityAttribute + "]";
                        }
                        else
                        {
                            keyAttribute = "[Key" + identityAttribute + "]";
                        }
                        foreach (var fkRelatedTo in property.RelatedTo)
                        {
                            var oneToOneCount = property.RelatedTo.Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToOne).Count();
                            var toProperty = ((EzDbSchema.Core.Objects.Relationship)fkRelatedTo).ToProperty;
                            if ((fkRelatedTo.MultiplicityType == RelationshipMultiplicityType.OneToOne) && (toProperty.IsKey) && (oneToOneCount==1))
                            {
                                var toFieldName = (fkRelatedTo.ToColumnName.Replace(" ", "") + Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix).Trim();
                                if (toFieldName.Equals(entity.Alias)) toFieldName = fkRelatedTo.EndAsObjectPropertyName();
                                fkAttributes += "[ForeignKey(\"" + toFieldName + "\")]";
                            }
                        }
                    }
                    /*
                    List<string> PreviousOneToOneFields = new List<string>();
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne).FindItems(RelationSearchField.FromFieldName, property.Name);
                    var groupedByFKName = RelationshipsOneToOne.GroupByFKName();
                    foreach (var FKName in groupedByFKName.Keys)
                    {
                        string FieldName = "";
                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        var toGroupRelationshipList = entity.Parent[relGroupSummary.ToTableName].Relationships.GroupByFKName();
                        var CountOfThisEntityInTargetRelationships = toGroupRelationshipList.CountItems(RelationSearchField.ToTableName, relGroupSummary.FromTableName);

                        int SameTableCount = groupedByFKName.CountItems(RelationSearchField.ToTableName, relGroupSummary.ToTableName);
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                        string ToTableNameSingular = ToTableName.ToSingular();
                        FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular)
                                    || (entity.Properties.ContainsKey(ToTableNameSingular))
                                    || (entityName == relGroupSummary.ToTableName)
                                    || (SameTableCount > 1) || (CountOfThisEntityInTargetRelationships > 1)) ? string.Join(",", relGroupSummary.FromColumnName) : ToTableNameSingular);

                        //One to one relationships will always point to a virtual item of that class it is related to,  so it needs to have the InverseFKTargetNameCollisionSuffix as the 
                        // virtual target item will
                        fkAttributes += "[ForeignKey(\"" + (FieldName.Replace(" ", "") + Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix).Trim() + "\")]";

                        PreviousOneToOneFields.Add(relGroupSummary.ToTableName);
                        if (fkAttributes.Length > 0) { break; };

                    }
                    */

                    var OutputAttributeString = new StringBuilder();
                    if (keyAttribute.Length > 0) OutputAttributeString.Append(keyAttribute);
                    if (fkAttributes.Length > 0) OutputAttributeString.Append(fkAttributes);
                    if (property.Get("Computed", false)) OutputAttributeString.Append("[DatabaseGenerated(DatabaseGeneratedOption.Computed)]");
                    if (property.Get("NotMapped", false)) OutputAttributeString.Append("[NotMapped]");
                    if (decimalAttribute.Length > 0) OutputAttributeString.Append(decimalAttribute);
                    if (OutputAttributeString.Length > 0) writer.WriteSafeString(prefix + OutputAttributeString.ToString() + @"" + prefix);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message + " in entity " + entityName);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }

            });

            Handlebars.RegisterHelper("POCOModelFKConstructorInitV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKConstructorInitV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
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
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKPropertiesV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKPropertiesV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    List<string> PreviousOneToManyFields = new List<string>();
                    PreviousOneToManyFields.Clear();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToMany).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (fkName.Equals("FK_Wells_PIGAreaId_DELETE")) {
                            fkName = (fkName + " ").Trim();
                        }
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();
                            string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias.ToSingular();
                            //string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();
                            string ToObjectFieldName = string.Format("{0}_{1}", relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", ""), relGroupSummary.ToColumnName.First());

                            //Check and see we have multiple declarations of the same table,  if we do, we will need an inverse 
                            // property annotation figure out how to property find the correct object property target 
                            var inversePropertyAttribute = "";
                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                            if (SameTableCount > 1)
                            {
                                var toGroupRelationshipList = entity.Parent[relGroupSummary.ToTableName].Relationships.GroupByFKName();
                                if (!toGroupRelationshipList.ContainsKey(fkName)) throw new Exception(string.Format("The inverse of FK {0} ({1}->{2})", fkName, relGroupSummary.FromTableName, relGroupSummary.ToTableName));
                                var inverseOfThisFK = toGroupRelationshipList[fkName];
                                var relGroupSummaryInverse = inverseOfThisFK.AsSummary();
                                var InversePropertyNamePotential = relGroupSummaryInverse.EndAsObjectPropertyName();
                                if (InversePropertyNamePotential.Length > 0) inversePropertyAttribute = string.Format("[InverseProperty(\"{0}\")]", InversePropertyNamePotential);
                            }

                            writer.WriteSafeString(string.Format("\n\n{0}//<summary>{1} {2}</summary>", prefix, relGroupSummary.Name, relGroupSummary.MultiplicityType.AsString()));
                            writer.WriteSafeString(string.Format("\n{0}[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Usage\", \"CA2227: CollectionPropertiesShouldBeReadOnly\")]", prefix));
                            if (inversePropertyAttribute.Length > 0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, inversePropertyAttribute));
                            writer.WriteSafeString(string.Format("\n{0}public virtual ICollection<{1}> {2} {{ get; set; }}", prefix, ToTableName, ToObjectFieldName));

                            PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("POCOModelFKManyToZeroToOneV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOneV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Schema + "." + entity.Name;

                    List<string> PreviousManyToOneFields = new List<string>();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ManyToZeroOrOne).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if ((fkName.Equals("FK_AreaTargetFormations_TargetFormations")) || (fkName.Equals("FK_AreaTargetFormations_TargetFormations1")))
                            fkName = fkName + "";
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();

                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                            string ToTableNameSingular = relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular();
                            string _FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular)
                                                 || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                 || (entityName == relGroupSummary.ToTableName)
                                                 || (SameTableCount > 1))
                                                    ? string.Join(", ", relGroupSummary.FromColumnName ) : ToTableNameSingular);

                            string FieldName = entity.GenerateObjectName(fkName, ObjectNameGeneratedFrom.JoinFromColumnName);

                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1}  *->0|1</summary>", prefix, fkName));
                            writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, relGroupSummary.FromFieldName));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
                            PreviousManyToOneFields.Add(FieldName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKOneToOneV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKOneToOneV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Schema + "." + entity.Name;

                    var PreviousOneToOneFields = new List<string>();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.OneToOne).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if ((fkName.Equals("FK_Wells_WellStickSurveys")) || (fkName.Equals("FK_Wells_WellStickSurveys")))
                            fkName += fkName + "";
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
                                                    ?  string.Format(",", relGroupSummary.FromColumnName) : ToTableNameSingular);
                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} 1->1</summary>", prefix, relGroupSummary.Name));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relGroupSummary.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
                            PreviousOneToOneFields.Add(FieldName);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            

            Handlebars.RegisterHelper("POCOModelFKZeroOrOneToOneV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKZeroOrOneToOneV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Schema + "." + entity.Name;


                    if (entityName.Equals("dbo.ForecastHorizontal"))
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
                        if (fkName.Contains("FK_Wells_WellStickSurveys"))
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
                            var FKAttribute = "";
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
                                        IsRequired = ((entityName.Equals(fk.FromTableName)) && (!fk.ToTableName.Equals(fk.FromTableName))) ? !((Property)fk.FromProperty).IsNullable : !((Property)fk.ToProperty).IsNullable;
                                    //Because this field exists in the target entity primary keys, then convvention should automatically know to resolve the foriegn key, but if 
                                    // it DOES NOT exist,  then we will need to write the key attribute.. however
                                    /// if this is a one to one relationship to the same table, we will apply the ForiegnKey atttribute to the primary key to the object
                                    if (!FKFieldExistsOnTargetTablePKs) WriteFKAttribute = true;
                                    fkList.Add(SourceFieldName);
                                }
                                //Determine wich end of the relationship to use, since we need the target, choose the end that does not equal this entities table name (schame.table)
                            }
                            if (WriteFKAttribute) AttributeText.Append(string.Format("[ForeignKey(\"{0}\")]", string.Join(", ", fkList.ToList())));
                            if (IsRequired) AttributeText.Append("[Required]");
                            if (AttributeText.Length>0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, AttributeText.ToString()));
                            /*
                            if (relationship.FromTableName.Equals(relationship.ToTableName))
                            {
                                writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, relationship.FromFieldName[relationship.FromFieldName.Count-1]));
                            }
                            else
                            {
                                writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, string.Join(", ", relationship.FromFieldName)));
                            }
                            */
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relationship.ToTableName.Replace(Internal.AppSettings.Instance.Configuration.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
                            PreviousOneToOneFields.Add(FieldName);

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            Handlebars.RegisterHelper("UnitTestsRenderExtendedEndpointsV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('UnitTestsRenderExtendedEndpointsV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
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
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
        }
    }
}