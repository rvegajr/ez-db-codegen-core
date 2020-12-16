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

namespace EzDbCodeGen.Core
{
    public static class HandlebarsCsV0Utility
    {
        public static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("POCOModelPropertyAttributesV0", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelPropertyAttributesV0')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var property = (IProperty)context.Value;
                    var entity = property.Parent;
                    var entityName = entity.Schema + "." + entity.Name;
                    var decimalAttribute = "";
                    var keyAttribute = "";
                    var fkAttributes = "";
                    var identityAttribute = "";

                    if ((entityName.Contains("dbo.AreaTargetFormations")) && (property.Name.Contains("AreaTypeId")))
                        entityName = (entityName + " ").Trim();
                    if (entityName.Contains("dbo.Scenarios")) //&& (property.Name.Contains("")))
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
                            if (((relGroupSummary.ToColumnName.Count > 1) && (!relGroupSummary.FromTableName.Equals(relGroupSummary.ToTableName))) || (relGroupSummary.MultiplicityType == RelationshipMultiplicityType.OneToOne) || (relGroupSummary.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne))
                            {
                                var ColumnOrder = "";
                                if (relGroupSummary.ToColumnName.Count > 1)
                                {
                                    ColumnOrder = string.Format(", Column(Order = {0})", relGroupSummary.ToColumnProperties.Where(p => p.Name.Equals(property.Name)).Select(p => p.KeyOrder).FirstOrDefault());
                                    fkAttributes += string.Format("[ForeignKey(\"{0}\"){1}]", (FieldName.Replace(" ", "") + Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix).Trim(), ColumnOrder);
                                }
                            }
                            if (fkAttributes.Length > 0) break;

                        }
                    }

                    if (keyAttribute.Length > 0) writer.WriteSafeString(keyAttribute);
                    if (fkAttributes.Length > 0) writer.WriteSafeString(fkAttributes);
                    if ((property.Name == "SysStartTime") || (property.Name == "SysEndTime")) writer.WriteSafeString("[DatabaseGenerated(DatabaseGeneratedOption.Computed)]\n");
                    if (decimalAttribute.Length > 0) writer.WriteSafeString(decimalAttribute);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
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
                            string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                            //string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();
                            string ToObjectFieldName = string.Format("{0}_{1}", relGroupSummary.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", ""), relGroupSummary.ToColumnName.First());

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
                    var entity = (IEntity)context.Value;
                    List<string> PreviousOneToManyFields = new List<string>();
                    PreviousOneToManyFields.Clear();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToMany).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();
                            string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                            //string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();
                            string ToObjectFieldName = string.Format("{0}_{1}", relGroupSummary.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", ""), relGroupSummary.ToColumnName.First());


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
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;

                    List<string> PreviousManyToOneFields = new List<string>();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.ManyToZeroOrOne).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();

                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                            string ToTableNameSingular = relGroupSummary.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", "").ToSingular();
                            string _FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular)
                                                 || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                 || (entityName == relGroupSummary.ToTableName)
                                                 || (SameTableCount > 1))
                                                    ? string.Join(", ", relGroupSummary.FromColumnName ) : ToTableNameSingular);

                            string FieldName = entity.GenerateObjectName(fkName, ObjectNameGeneratedFrom.JoinFromColumnName);

                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1}  *->0|1</summary>", prefix, fkName));
                            writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, relGroupSummary.FromFieldName));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relGroupSummary.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
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
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;

                    var PreviousOneToOneFields = new List<string>();

                    var FKToUse = entity.Relationships.Fetch(RelationshipMultiplicityType.OneToOne).Select(s => s.Name).ToList();
                    foreach (var fkNameKV in entity.RelationshipGroups)
                    {
                        var fkName = fkNameKV.Key;
                        if (FKToUse.Contains(fkName))
                        {
                            var relationship = fkNameKV.Value;
                            var relGroupSummary = relationship.AsSummary();

                            int SameTableCount = 0;
                            foreach (var rg in entity.RelationshipGroups.Values)
                                if (rg.AsSummary().ToTableName.Equals(relGroupSummary.ToTableName)) SameTableCount++;
                            string ToTableNameSingular = relGroupSummary.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", "").ToSingular();
                            string FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular)
                                                 || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                 || (entityName == relGroupSummary.ToTableName)
                                                 || (SameTableCount > 1))
                                                    ?  string.Format(",", relGroupSummary.FromColumnName) : ToTableNameSingular);
                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} 1->1</summary>", prefix, relGroupSummary.Name));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relGroupSummary.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
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
                    var entity = (IEntity)context.Value;
                    var entityName = entity.Schema + "." + entity.Name;


                    if (entityName.Equals("dbo.Areas"))
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
                            string ToTableNameSingular = relationship.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", "").ToSingular();
                            string _FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular)
                                                 || (entity.Properties.ContainsKey(ToTableNameSingular))
                                                 || (entityName == relationship.ToTableName)
                                                 || (SameTableCount > 1))
                                                    ? relationship.ToUniqueColumnName() : ToTableNameSingular);
                            string FieldName = entity.GenerateObjectName(fkName, ObjectNameGeneratedFrom.ToUniqueColumnName);
                            string ForeignKey = ((entityName == relationship.ToTableName) ? relationship.FromFieldName[relationship.FromFieldName.Count-1] : string.Join(", ", relationship.FromFieldName));

                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} {2}</summary>", prefix, relationship.Name, relationship.MultiplicityType.AsString()));
                            if (relationship.FromTableName.Equals(relationship.ToTableName))
                            {
                                writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, relationship.FromFieldName[relationship.FromFieldName.Count-1]));
                            }
                            else
                            {
                                writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, string.Join(", ", relationship.FromFieldName)));
                            }
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, relationship.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema + ".", "").ToSingular(), FieldName));
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
                        string ToTableNameSingular = relationship.ToTableName.Replace(Config.Configuration.Instance.Database.DefaultSchema+".", "").ToSingular();
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