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

namespace EzDbCodeGen.Core
{
    public static class HandlebarsCsUtility
    {
        public static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("POCOModelPropertyAttributes", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelPropertyAttributes')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var property = (IProperty)context;
                    var entity = property.Parent;
                    var database = entity.Parent;
                    var entityName = entity.Name;
                    if (entityName.Contains("tbl_DocumentLocation"))
                    {
                        entityName += "";
                    }
                    var decimalAttribute = "";
                    var keyAttribute = "";
                    var fkAttributes = "";
                    var identityAttribute = "";
                    var columnAttribute = "";

                    if (property.Name.Equals(entity.Alias)) {
                        columnAttribute = string.Format("[Column(\"{0}\")]", property.Name);
                    }
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
                        var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne).FindItems(RelationSearchField.FromFieldName, property.Name);
                        var groupedByFKName = RelationshipsOneToOne.GroupByFKName();
                        foreach (var FKName in groupedByFKName.Keys)
                        {
                            string FieldName = "";
                            var relationshipList = groupedByFKName[FKName];
                            var relGroupSummary = relationshipList.AsSummary();
                            var toGroupRelationshipList = entity.Parent[relGroupSummary.ToTableName].Relationships.GroupByFKName();
                            var CountOfThisEntityInTargetRelationships = toGroupRelationshipList.CountItems(RelationSearchField.ToTableName, relGroupSummary.FromTableName);
                            /*
                            if (CountOfThisEntityInTargetRelationships == 1)
                            {
                                FieldName = entity.Parent[relGroupSummary.FromTableName].Alias + Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix;
                            }
                            else if (CountOfThisEntityInTargetRelationships > 1)
                            {
                                FieldName = string.Join(",", relGroupSummary.ToColumnName) + Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix;
                            }
                            */
                            //$$$

                            int SameTableCount = RelationshipsOneToOne.CountItems(RelationSearchField.ToTableName, relGroupSummary.ToTableName);
                            //Need to resolve the to table name to what the alias table name is
                            string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                            string ToTableNameSingular = ToTableName.ToSingular();
                            FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular) 
                                        || (entity.Properties.ContainsKey(ToTableNameSingular)) 
                                        || (entityName == relGroupSummary.ToTableName) 
                                        || (SameTableCount > 1) || (CountOfThisEntityInTargetRelationships > 1)) ? string.Join(",", relGroupSummary.FromColumnName) : ToTableNameSingular);

                            fkAttributes += "[ForeignKey(\"" + FieldName.Replace(" ", "") + "\")]";

                            PreviousOneToOneFields.Add(relGroupSummary.ToTableName);
                            if (fkAttributes.Length > 0) { break; };

                        }

                    }
                    if (keyAttribute.Length > 0) writer.WriteSafeString(prefix + keyAttribute + "\n");
                    if (fkAttributes.Length > 0) writer.WriteSafeString(prefix + fkAttributes + "\n");
                    if ((property.Name == "SysStartTime") || (property.Name == "SysEndTime")) writer.WriteSafeString(prefix + "[DatabaseGenerated(DatabaseGeneratedOption.Computed)]\n");
                    if (decimalAttribute.Length > 0) writer.WriteSafeString(prefix + decimalAttribute + "\n");
                    if (columnAttribute.Length > 0) writer.WriteSafeString(prefix + columnAttribute + "\n");
                    writer.WriteSafeString(prefix); //Write the space header to make sure there is always space
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }

            });

            Handlebars.RegisterHelper("POCOModelFKConstructorInit", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKConstructorInit')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
  

                    var PreviousOneToManyFields = new List<string>();

                    var groupedByFKName = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany).GroupByFKName();
                    foreach(var FKName in groupedByFKName.Keys)
                    {
                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                        writer.WriteSafeString(string.Format(
                            "\n{0}this.{1} = new HashSet<{2}>(); //{3} 0|1->*"
                            , prefix, relGroupSummary.ToUniqueColumnName().ToPlural(), ToTableName, relGroupSummary.Name));
                        PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) => {
                var entity = (IEntity)context;
                var PROC_NAME = string.Format("Handlebars.RegisterHelper('POCOModelFKProperties', Entity='{0}')", entity.Name);
               
                try
                {
                    var prefix = parameters.AsString(0);
                    List<string> PreviousOneToManyFields = new List<string>();
                    PreviousOneToManyFields.Clear();

                    if (entity.Name.Contains("tbl_Parcel"))
                    {
                        entity.Name += "";
                    }

                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);
                    var groupedByFKName = RelationshipsOneToMany.GroupByFKName();
                    foreach (var FKName in groupedByFKName.Keys)
                    {
                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;

                        //Check and see we have multiple declarations of thre same table,  if we do, we will need an inverse 
                        // property annotation figure out how to property find the correct object property target 
                        var inversePropertyAttribute = "";
                        if (RelationshipsOneToMany.CountItems(relGroupSummary.ToTableName) > 1) {
                            var InversePropertyName = "";
                            var toGroupRelationshipList = entity.Parent[relGroupSummary.ToTableName].Relationships.GroupByFKName();
                            var CountOfThisEntityInTargetRelationships = toGroupRelationshipList.CountItems(RelationSearchField.ToTableName, relGroupSummary.FromTableName);
                            if (CountOfThisEntityInTargetRelationships == 1)
                            {
                                InversePropertyName = entity.Parent[relGroupSummary.FromTableName].Alias + Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix;
                            }
                            else if (CountOfThisEntityInTargetRelationships > 1)
                            {
                                InversePropertyName = string.Join(",", relGroupSummary.ToColumnName) + Config.Configuration.Instance.Database.InverseFKTargetNameCollisionSuffix;
                            }
                            if (InversePropertyName.Length > 0)
                            {
                                inversePropertyAttribute = string.Format("[InverseProperty(\"{0}\")]", InversePropertyName);
                            }
                        }
                        writer.WriteSafeString(string.Format("\n\n{0}//<summary>{1}</summary>", prefix, relGroupSummary.Name));
                        writer.WriteSafeString(string.Format("\n{0}[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Usage\", \"CA2227: CollectionPropertiesShouldBeReadOnly\")]", prefix));
                        if (inversePropertyAttribute.Length > 0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, inversePropertyAttribute));
                        writer.WriteSafeString(string.Format("\n{0}public virtual ICollection<{1}> {2} {{ get; set; }}", prefix, ToTableName, relGroupSummary.ToUniqueColumnName().ToPlural()));

                        PreviousOneToManyFields.Add(relGroupSummary.ToTableName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("POCOModelFKManyToZeroToOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOne')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var objectSuffix = parameters.AsString(1);
                    var entity = (IEntity)context;
                    var entityName = entity.Name;


                    if (entity.Name.Contains("Noun"))
                    {
                        entity.Name += "";
                    }

                    List<string> PreviousManyToOneFields = new List<string>();

                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);
                    var groupedByFKName = RelationshipsManyToOne.GroupByFKName();
                    foreach (var FKName in groupedByFKName.Keys)
                    {
                        var relationshipList = groupedByFKName[FKName];
                        var relGroupSummary = relationshipList.AsSummary();
                        string ToTableName = entity.Parent.Entities[relGroupSummary.ToTableName].Alias;
                        
                        int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relGroupSummary.ToTableName);
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular)
                                             || (entity.Properties.ContainsKey(ToTableNameSingular))
                                             || (entityName == relGroupSummary.ToTableName)
                                             || (SameTableCount > 1))
                                                ? relGroupSummary.ToUniqueColumnName() : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}/// <summary>{1}  *->0|1</summary>", prefix, relGroupSummary.Name));
                        var ForeignKeyName = "";
                        //Pick the key that exists in this entities properties
                        if (entity.Properties.ContainsKey(string.Join(", ", relGroupSummary.ToFieldName))) ForeignKeyName = string.Join(", ", relGroupSummary.ToFieldName);
                        if (entity.Properties.ContainsKey(string.Join(", ", relGroupSummary.FromFieldName))) ForeignKeyName = string.Join(", ", relGroupSummary.FromFieldName);
                        if (ForeignKeyName.Length>0) writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, ForeignKeyName));
                        writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, FieldName + objectSuffix));
                        PreviousManyToOneFields.Add(relGroupSummary.ToTableName);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKOneToOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKOneToOne')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Name;
                    if (entityName.Contains("Collateral"))
                    {
                        entityName += "";
                    }
                    var PreviousOneToOneFields = new List<string>();
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne); 
                    foreach (var relationship in RelationshipsOneToOne)
                    {
                        if (relationship.Name.StartsWith("FK_tbl_DocumentLocationHistory_tbl_DocumentLocation"))
                        {
                            relationship.Name += "";
                        }
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                        int SameTableCount = RelationshipsOneToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular) 
                                             || (entity.Properties.ContainsKey(ToTableNameSingular)) 
                                             || (entityName == relationship.ToTableName) 
                                             || (SameTableCount > 1)) 
                                                ? relationship.ToUniqueColumnName() : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} 1->1</summary>", prefix, relationship.Name));
                        writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, (FieldName.Replace(" ", ""))));
                        //writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, (FieldName.Replace(" ", "") + Config.Configuration.Instance.Database.PropertyObjectNameCollisionSuffix)));
                        PreviousOneToOneFields.Add(FieldName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("UnitTestsRenderExtendedEndpoints", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('UnitTestsRenderExtendedEndpoints')";
                try
                {
                    var prefix = parameters.AsString(0);
					var entity = (IEntity)context;
                    var entityName = entity.Name;

                    var PreviousManyToOneFields = new List<string>();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne); 
                    foreach (var relationship in RelationshipsManyToOne)
                    {
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                        int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string ExpandColumnName = ((PreviousManyToOneFields.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}using (var response = await HttpClient.GetAsync(\"http://testserver/api/{1}?%24expand={2}<t/>&%24top=10\")) ", prefix, entityName, ExpandColumnName.Replace(" ", "")));
                        writer.WriteSafeString(string.Format("\n{0}{{ ", prefix));
                        writer.WriteSafeString(string.Format("\n{0}    var result = await response.Content.ReadAsStringAsync(); ", prefix));
                        writer.WriteSafeString(string.Format("\n{0}    Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, \"Return Get 10 or less {1} with Expand of {2}. \" + result); ", prefix, entityName, ExpandColumnName.Replace(" ", "")));
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

            Handlebars.RegisterHelper("EntityPrimaryKeysAsParmString", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsParmString')";
                try
                {
					writer.WriteSafeString(((IEntity)context).PrimaryKeys.AsParmString().Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("IsAuditiableOutput", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityBaseClass')";
                try
                {
                    var outputText = parameters.AsString(0);
					writer.WriteSafeString( ((IEntity)context).IsAuditable() ? outputText : ""  );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("EntityPrimaryKeysAsLinqEquation", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsLinqEquation')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var prefixSetter = ((parameters.Count()>1) ? parameters.AsString(1) : ""); 
					writer.WriteSafeString(((IEntity)context).PrimaryKeys.AsLinqEquationString(prefix, " && ", "==", prefixSetter).Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("EntityPrimaryKeysAsODataRouteString", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsODataRouteString')";
                try
                {
					writer.WriteSafeString(((IEntity)context).PrimaryKeys.AsODataRouteString().Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("EntityPrimaryKeysAsCsvString", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('EntityPrimaryKeysAsCsvString')";
                try
                {
                    var prefix = parameters.AsString(0);
					writer.WriteSafeString(((IEntity)context).PrimaryKeys.AsCsvString(prefix).Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
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

					writer.WriteSafeString(((IEntity)context).PrimaryKeys.AsParmBooleanCheck(prefix, Op).Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
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

					writer.WriteSafeString(((IEntity)context).PrimaryKeys.AsParmBooleanCheck(prefix, Op).Trim());
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