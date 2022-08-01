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
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core
{
    internal static class HandlebarsCsUtility
    {
        internal static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("POCOModelPropertyAttributes", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelPropertyAttributes')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var property = (IProperty)context.Value;
                    var entity = property.Parent;
                    var database = entity.Parent;
                    var entityName = entity.Name;
                    if (entityName.Contains("Well"))
                    {
                        entityName += "  ";
                        entityName = entityName.Trim();
                    }
                    var decimalAttribute = "";
                    var keyAttribute = "";
                    var fkAttributes = "";
                    var identityAttribute = "";
                    var columnAttribute = "";


                    if ((property.Name.Equals(entity.Alias)) ||
                          (property.Parent.Relationships.FindItems(RelationSearchField.ToColumnName, property.Name).Count >= 1))
                    {
                        columnAttribute = string.Format("[Column(\"{0}\")]", property.Name);
                    }
                    /*else if (property.Name.Equals(entity.Alias))  //if the propery is the same as the object name, then we might need to have a column specifier incase we have to rename it
                    {
                        columnAttribute = string.Format("[Column(\"{0}\")]", property.Name);
                    }*/
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
                            keyAttribute = string.Format(@"[Key, Column(""{0}"", Order={1}){2}]", property.Name, property.KeyOrder, identityAttribute);
                            columnAttribute = "";  //Clear the column attribute since we have already added it here
                        }
                        else
                        {
                            keyAttribute = "[Key" + identityAttribute + "]";
                        }

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
                            fkAttributes += "[ForeignKey(\"" + FieldName.Replace(" ", "") + Internal.AppSettings.Instance.Configuration.Database.InverseFKTargetNameCollisionSuffix + "\")]";

                            PreviousOneToOneFields.Add(relGroupSummary.ToTableName);
                            if (fkAttributes.Length > 0) { break; };

                        }

                    }

                    if (keyAttribute.Length > 0) writer.WriteSafeString(prefix + keyAttribute + "\n");
                    if (fkAttributes.Length > 0) writer.WriteSafeString(prefix + fkAttributes + "\n");
                    if ((property.Name == "SysStartTime") 
                        || (property.Name == "SysEndTime") 
                        || (Internal.AppSettings.Instance.Configuration.IsComputedColumn(property))
                        || (property.IsComputed())
                        )
                        writer.WriteSafeString(prefix + "[DatabaseGenerated(DatabaseGeneratedOption.Computed)]\n");
                    if (decimalAttribute.Length > 0) writer.WriteSafeString(prefix + decimalAttribute + "\n");
                    if (columnAttribute.Length > 0) writer.WriteSafeString(prefix + columnAttribute + "\n");
                    writer.WriteSafeString(prefix); //Write the space header to make sure there is always space
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }

            });

            Handlebars.RegisterHelper("PropertyAsObjectName", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('PropertyAsObjectName')";
                try
                {
                    var prefix = parameters.AsString(0);
                    if (((Object)context.Value).GetType().Name == "Property")
                        writer.WriteSafeString(((IProperty)context.Value).AsObjectPropertyName());
                    else
                        throw new Exception(string.Format("Context cannot be of type {0}.", ((Object)context.Value).GetType().Name));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString(string.Format("{0}: **** ERROR RENDERING **** {1}", PROC_NAME, ex.Message));
                }

            });

            Handlebars.RegisterHelper("POCOModelFKConstructorInit", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKConstructorInit')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;
                    if (entity.Name.Contains("Relationship"))
                    {
                        entity.Name += "";
                    }

                    var PreviousOneToManyFields = new List<string>();

                    var relZeroOrOneToMany = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToMany);
                    var groupedByFKName = relZeroOrOneToMany.GroupByFKName();
                    foreach (var FKName in groupedByFKName.Keys)
                    {
                        if (FKName.Contains("FK_tbl_Tax_tbl_Parcel") || (FKName.Contains("FK_tbl_Tax_tbl_Parcel")))
                        {
                            entity.Name += "";
                        }

                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                        string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();

                        writer.WriteSafeString(string.Format(
                            "\n{0}this.{1} = new HashSet<{2}>(); //{3} 0|1->*"
                            , prefix, ToObjectFieldName.ToPlural(), ToTableName, relGroupSummary.Name));
                        PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) => {
                var entity = (IEntity)context.Value;
                var PROC_NAME = string.Format("Handlebars.RegisterHelper('POCOModelFKProperties', Entity='{0}')", entity.Name);

                try
                {
                    var prefix = parameters.AsString(0);
                    List<string> PreviousOneToManyFields = new List<string>();
                    PreviousOneToManyFields.Clear();

                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToMany);
                    var groupedByFKName = RelationshipsOneToMany.GroupByFKName();
                    foreach (var FKName in groupedByFKName.Keys)
                    {
                        if (FKName.Contains("FKNAMEHERE"))
                        {
                            entity.Name += "";
                        }
                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                        string ToObjectFieldName = relGroupSummary.AsObjectPropertyName();

                        //Check and see we have multiple declarations of thre same table,  if we do, we will need an inverse 
                        // property annotation figure out how to property find the correct object property target 
                        var inversePropertyAttribute = "";
                        if (groupedByFKName.CountItems(relGroupSummary.ToTableName) > 1)
                        {
                            var toGroupRelationshipList = entity.Parent[relGroupSummary.ToTableName].Relationships.GroupByFKName();
                            if (!toGroupRelationshipList.ContainsKey(FKName)) throw new Exception(string.Format("The inverse of FK {0} ({1}->{2})", FKName, relGroupSummary.FromTableName, relGroupSummary.ToTableName));
                            var inverseOfThisFK = toGroupRelationshipList[FKName];
                            var relGroupSummaryInverse = inverseOfThisFK.AsSummary();
                            var InversePropertyNamePotential = relGroupSummaryInverse.EndAsObjectPropertyName();
                            if (InversePropertyNamePotential.Length > 0) inversePropertyAttribute = string.Format("[InverseProperty(\"{0}\")]", InversePropertyNamePotential);
                        }

                        writer.WriteSafeString(string.Format("\n\n{0}//<summary>{1} {2}</summary>", prefix, relGroupSummary.Name, relGroupSummary.MultiplicityType.AsString()));
                        writer.WriteSafeString(string.Format("\n{0}[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Usage\", \"CA2227: CollectionPropertiesShouldBeReadOnly\")]", prefix));
                        if (inversePropertyAttribute.Length > 0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, inversePropertyAttribute));
                        writer.WriteSafeString(string.Format("\n{0}public virtual ICollection<{1}> {2} {{ get; set; }}", prefix, ToTableName, ToObjectFieldName.ToPlural()));

                        PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            Handlebars.RegisterHelper("RelationshipGroupAsSummaryField", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('RelationshipGroupAsSummaryField')";
                try
                {
                    var fieldName = parameters.AsString(0);

                    var stringModInstructions = parameters.AsString(1);
                    var contextObject = (Object)context.Value;
                    IRelationshipList relationshipList = null;
                    IDatabase database = null;
                    if (contextObject.GetType().Name == "RelationshipList")
                    {
                        relationshipList = ((IRelationshipList)context.Value);
                        database = relationshipList.FirstOrDefault().Parent.Parent;
                    }
                    else
                        throw new Exception(string.Format("Helper cannot process context type of '{0}'", contextObject.GetType().Name));
                    var summary = relationshipList.AsSummary();
                    var output = "";
                    if (fieldName == "Name") output = summary.Name;
                    if (fieldName == "FromColumnName") output = string.Join("", summary.FromColumnName);
                    if (fieldName == "FromFieldName") output = string.Join("", summary.FromFieldName);
                    if (fieldName == "FromTableName") output = summary.FromTableName;
                    if (fieldName == "FromTableAlias")
                    {
                        if (database.ContainsKey(summary.FromTableName)) output = database[summary.FromTableName].Alias;
                    }
                    if (fieldName == "ToColumnName") output = string.Join("", summary.ToColumnName);
                    if (fieldName == "ToFieldName") output = string.Join("", summary.ToFieldName);
                    if (fieldName == "ToTableName") output = summary.ToTableName;
                    if (fieldName == "ToUniqueColumnName") output = summary.ToUniqueColumnName();
                    if (fieldName == "MultiplicityType") output = summary.MultiplicityType.ToString();
                    if (fieldName == "Type") output = summary.Type;
                    if (fieldName == "Types") output = string.Join(", ", summary.FromFieldName); ;
                    if (fieldName == "MultiplicityTypeWarning") output = (summary.MultiplicityTypeWarning ? "true" : "false");
                    if (fieldName == "ToTableAlias")
                    {
                        if (database.ContainsKey(summary.ToTableName)) output = database[summary.ToTableName].Alias;
                    }
                    if (output.Length == 0)
                    {
                        throw new Exception(string.Format("Helper cannot process field name of '{0}'", fieldName));
                    }
                    writer.WriteSafeString(output.StringMod(stringModInstructions));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("POCOModelFKManyToZeroToOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOne')";
                try
                {
                    var contextObject = (Object)context.Value;
                    IEntity entity = null;
                    if (contextObject.GetType().Name == "Relationship")
                        entity = ((IRelationship)context.Value).Parent;
                    else if (contextObject.GetType().Name == "RelationshipList")
                        entity = ((IRelationshipList)context.Value).FirstOrDefault().Parent;
                    else if (contextObject.GetType().Name == "Entity")
                        entity = ((IEntity)context.Value);

                    var entityName = entity.Name;

                    var prefix = "";
                    var objectSuffix = "";
                    var fkNametoSelect = "";
                    if (parameters.Count() == 1)
                    {
                        if (string.IsNullOrWhiteSpace(parameters.AsString(0))) prefix = parameters.AsString(0);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(0))) fkNametoSelect = parameters.AsString(0);
                    }
                    else if (parameters.Count() == 2)
                    {
                        if (string.IsNullOrWhiteSpace(parameters.AsString(0))) prefix = parameters.AsString(0);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(0))) fkNametoSelect = parameters.AsString(0);
                        if (string.IsNullOrWhiteSpace(parameters.AsString(1))) prefix = parameters.AsString(1);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(1))) fkNametoSelect = parameters.AsString(1);
                    }
                    else if (parameters.Count() == 3)
                    {
                        if (string.IsNullOrWhiteSpace(parameters.AsString(0))) prefix = parameters.AsString(0);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(0))) fkNametoSelect = parameters.AsString(0);
                        if (string.IsNullOrWhiteSpace(parameters.AsString(1))) prefix = parameters.AsString(1);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(1))) fkNametoSelect = parameters.AsString(1);
                        objectSuffix = parameters.AsString(2);
                    }

                    if (entity.Name.Contains("Relationship"))
                    {
                        entity.Name += "";
                    }

                    List<string> PreviousManyToOneFields = new List<string>();

                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipMultiplicityType.ManyToZeroOrOne);
                    var groupedByFKName = RelationshipsManyToOne.GroupByFKName();
                    foreach (var FKName in groupedByFKName.Keys)
                    {
                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;

                        int SameTableCount = groupedByFKName.CountItems(RelationSearchField.ToTableName, relGroupSummary.ToTableName);
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular)
                                             || (entity.Properties.ContainsKey(ToTableNameSingular))
                                             || (entityName == relGroupSummary.ToTableName)
                                             || (SameTableCount > 1))
                                                ? relGroupSummary.ToUniqueColumnName() : ToTableNameSingular).ToCsObjectName();

                        var ForeignKeyName = "";
                        //Pick the key that exists in this entities properties
                        if (entity.Properties.ContainsKey(string.Join(", ", relGroupSummary.ToFieldName))) ForeignKeyName = string.Join(", ", relGroupSummary.ToFieldName);
                        if (entity.Properties.ContainsKey(string.Join(", ", relGroupSummary.FromFieldName))) ForeignKeyName = string.Join(", ", relGroupSummary.FromFieldName);
                        PreviousManyToOneFields.Add(relGroupSummary.ToTableName);

                        if (fkNametoSelect.Length == 0)
                        {
                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1}  *->0|1</summary>", prefix, relGroupSummary.Name));
                            if (ForeignKeyName.Length > 0) writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, ForeignKeyName));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, FieldName + objectSuffix));
                        }
                        else
                        {
                            if (fkNametoSelect == FKName)
                            {
                                writer.WriteSafeString(FieldName);
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

            Handlebars.RegisterHelper("POCOModelFKZeroOrOneToOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKZeroOrOneToOne')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var contextObject = (Object)context.Value;
                    IEntity entity = null;
                    if (contextObject.GetType().Name == "Relationship")
                        entity = ((IRelationship)context.Value).Parent;
                    else if (contextObject.GetType().Name == "RelationshipList")
                        entity = ((IRelationshipList)context.Value).FirstOrDefault().Parent;
                    else if (contextObject.GetType().Name == "Entity")
                        entity = ((IEntity)context.Value);

                    var entityName = entity.Name;
                    var fkNametoSelect = "";
                    if (parameters.Count() == 1)
                    {
                        if (string.IsNullOrWhiteSpace(parameters.AsString(0))) prefix = parameters.AsString(0);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(0))) fkNametoSelect = parameters.AsString(0);
                    }
                    else if (parameters.Count() == 2)
                    {
                        if (string.IsNullOrWhiteSpace(parameters.AsString(0))) prefix = parameters.AsString(0);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(0))) fkNametoSelect = parameters.AsString(0);
                        if (string.IsNullOrWhiteSpace(parameters.AsString(1))) prefix = parameters.AsString(1);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(1))) fkNametoSelect = parameters.AsString(1);
                    }
                    else if (parameters.Count() == 3)
                    {
                        if (string.IsNullOrWhiteSpace(parameters.AsString(0))) prefix = parameters.AsString(0);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(0))) fkNametoSelect = parameters.AsString(0);
                        if (string.IsNullOrWhiteSpace(parameters.AsString(1))) prefix = parameters.AsString(1);
                        if (entity.RelationshipGroups.ContainsKey(parameters.AsString(1))) fkNametoSelect = parameters.AsString(1);
                    }
                    if (entityName.Contains("Noun"))
                    {
                        entityName += "";
                    }
                    var PreviousOneToOneFields = new List<string>();
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne);
                    foreach (var relationshipGroupKV in RelationshipsOneToOne.GroupByFKName())
                    {
                        var relationship = relationshipGroupKV.Value.AsSummary();

                        var toGroupRelationshipList = entity.Parent[relationship.ToTableName].Relationships.GroupByFKName();
                        if (!toGroupRelationshipList.ContainsKey(relationshipGroupKV.Key)) throw new Exception(string.Format("The inverse of FK {0} ({1}->{2})", relationshipGroupKV.Key, relationship.FromTableName, relationship.ToTableName));
                        var relationshipInverse = toGroupRelationshipList[relationshipGroupKV.Key].AsSummary();

                        if (relationship.Name.StartsWith("FK_tbl_Collateral_tbl_OccupancyStatus"))
                            relationship.Name += "";

                        if (fkNametoSelect.Length == 0)
                        {
                            writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} {2}</summary>", prefix, relationship.Name, relationship.MultiplicityType.AsString()));
                            //writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableName.ToSingular(), (FieldName.Replace(" ", ""))));
                            writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, string.Join(", ", relationship.FromObjectPropertyName)));
                            writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}",
                                prefix, entity.Parent.Entities[relationship.ToTableName].Alias.ToSingular(), relationship.EndAsObjectPropertyName()));
                        }
                        else
                        {
                            if (fkNametoSelect == relationship.Name)
                            {
                                writer.WriteSafeString(relationship.EndAsObjectPropertyName());
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


            Handlebars.RegisterHelper("UnitTestsRenderExtendedEndpoints", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('UnitTestsRenderExtendedEndpoints')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context.Value;

                    foreach (var relationshipGroupKV in entity.RelationshipGroups)
                    {
                        var relationshipGroup = relationshipGroupKV.Value;
                        var relationship = relationshipGroup.AsSummary();
                        string ExpandColumnName = relationship.EndAsObjectPropertyName();
                        writer.WriteSafeString(string.Format("\n{0}using (var response = await HttpClient.GetAsync(\"http://testserver/api/{1}?%24expand={2}<t/>&%24top=10\")) ", prefix, entity.Alias, ExpandColumnName));
                        writer.WriteSafeString(string.Format("\n{0}{{ ", prefix));
                        writer.WriteSafeString(string.Format("\n{0}    var result = await response.Content.ReadAsStringAsync(); ", prefix));
                        writer.WriteSafeString(string.Format("\n{0}    Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, \"Return Get 10 or less {1} with Expand of {2}. \" + result); ", prefix, entity.Name, ExpandColumnName));
                        writer.WriteSafeString(string.Format("\n{0}}} ", prefix));
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("EntityPrimaryKeysAsParmString", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsParmString')";
                try
                {
                    writer.WriteSafeString(((IEntity)context.Value).PrimaryKeys.AsParmString().Trim());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("IsAuditiableOutput", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityBaseClass')";
                try
                {
                    var outputText = parameters.AsString(0);
                    writer.WriteSafeString(((IEntity)context.Value).IsAuditable() ? outputText : "");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("EntityPrimaryKeysAsLinqEquation", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsLinqEquation')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var prefixSetter = ((parameters.Count() > 1) ? parameters.AsString(1) : "");
                    var entityName = ((IEntity)context.Value).Name;
                    if (entityName.Contains("ZipCode"))
                    {
                        entityName += "  ";
                        entityName = entityName.Trim();
                    }
                    writer.WriteSafeString(((IEntity)context.Value).PrimaryKeys.AsLinqEquationString(prefix, " && ", "==", prefixSetter).Trim());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("EntityPrimaryKeysAsODataRouteString", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsODataRouteString')";
                try
                {
                    writer.WriteSafeString(((IEntity)context.Value).PrimaryKeys.AsODataRouteString().Trim());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("EntityPrimaryKeysAsCsvString", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsCsvString')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var useObjectPropertyName = false;  //this will attempt to resolve the property name within context of its own objects
                    if (parameters.Length > 1)
                    {
                        useObjectPropertyName = parameters.AsString(1).StartsWith("O");
                    }

                    writer.WriteSafeString(((IEntity)context.Value).PrimaryKeys.AsCsvString(prefix, useObjectPropertyName).Trim());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            Handlebars.RegisterHelper("EntityPrimaryKeysAsParmBooleanCheck", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsParmBooleanCheck')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var Op = parameters.AsString(1);
                    if (Op.Length == 0) Op = "==";

                    writer.WriteSafeString(((IEntity)context.Value).PrimaryKeys.AsParmBooleanCheck(prefix, Op).Trim());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("PropertyExists", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('PropertyExists')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var Op = parameters.AsString(1);
                    if (Op.Length == 0) Op = "==";

                    writer.WriteSafeString(((IEntity)context.Value).PrimaryKeys.AsParmBooleanCheck(prefix, Op).Trim());
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