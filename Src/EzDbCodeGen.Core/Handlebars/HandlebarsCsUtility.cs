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
                            keyAttribute = @"[Key, Column(Order=" + property.KeyOrder + ")" + identityAttribute + "]";
                        }
                        else
                        {
                            keyAttribute = "[Key" + identityAttribute + "]";
                        }

                        List<string> PreviousOneToOneFields = new List<string>();
                        var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne);
                        foreach (var relationship in RelationshipsOneToOne)
                        {
							int SameTableCount = RelationshipsOneToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                            //Need to resolve the to table name to what the alias table name is
                            string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                            string ToTableNameSingular = ToTableName.ToSingular();
                            string FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                            if (relationship.FromTableName != relationship.PrimaryTableName)
                            {
                                fkAttributes += "[ForeignKey(\"" + relationship.ToColumnName.Replace(" ", "") + "\")]";
                            }
                            PreviousOneToOneFields.Add(relationship.ToTableName);
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
                    
                    foreach (var relationship in entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany))
                    {
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                        string FieldName = ToTableName + "_" + relationship.ToColumnName;
                        writer.WriteSafeString(string.Format(
                            "\n{0}this.{1} = new HashSet<{2}>(); //{3} 0|1->*"
                            , prefix, FieldName.Replace(" ", ""), ToTableName.ToSingular(), relationship.Name));
                        PreviousOneToManyFields.Add(relationship.ToTableName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKProperties')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    List<string> PreviousOneToManyFields = new List<string>();
                    PreviousOneToManyFields.Clear();
                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);

                    foreach (var relationship in RelationshipsOneToMany)
                    {
                        //Debugging code.. set a break point here
                        //if ((relationship.Name == "FK_ContinuousDrillingObligations_FromCDOTrigger") || (relationship.Name == "FK_ContinuousDrillingObligations_FromCDOTrigger"))
                        //    Console.Write("FK_ContinuousDrillingObligations_FromCDOTrigger");
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                        string FieldName = ToTableName + "_" + relationship.ToColumnName.Replace(" ", "");
						string InverseProperty = (RelationshipsOneToMany.CountItems(relationship.ToTableName) > 1 ? "[InverseProperty(\"" + relationship.ToColumnName.Replace(" ", "") + "\")]" : "");

                        writer.WriteSafeString(string.Format("\n\n{0}//<summary>{1}</summary>", prefix, relationship.Name)); 
                        writer.WriteSafeString(string.Format("\n{0}[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Usage\", \"CA2227: CollectionPropertiesShouldBeReadOnly\")]", prefix));
                        if (InverseProperty.Length>0) writer.WriteSafeString(string.Format("\n{0}{1}", prefix, InverseProperty)); 
                        writer.WriteSafeString(string.Format("\n{0}public virtual ICollection<{1}> {2} {{ get; set; }}", prefix, ToTableName.ToSingular(), FieldName.Replace(" ", "")));

                        PreviousOneToManyFields.Add(relationship.ToTableName);
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
					var entity = (IEntity)context;
                    var entityName = entity.Name;

                    List<string> PreviousManyToOneFields = new List<string>();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);
                    foreach (var relationship in RelationshipsManyToOne)
                    {
						int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular) 
                                             || (entity.Properties.ContainsKey(ToTableNameSingular)) 
                                             || (entityName == relationship.ToTableName) 
                                             || (SameTableCount > 1)) 
                                                ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}/// <summary>{1}  *->0|1</summary>", prefix, relationship.Name));
                        writer.WriteSafeString(string.Format("\n{0}[ForeignKey(\"{1}\")]", prefix, relationship.FromFieldName.Replace(" ", "")));
                        writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, FieldName.Replace(" ", "")));
                        PreviousManyToOneFields.Add(relationship.ToTableName);
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

                    var PreviousOneToOneFields = new List<string>();
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne); 
                    foreach (var relationship in RelationshipsOneToOne)
                    {
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;
                        int SameTableCount = RelationshipsOneToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular) 
                                             || (entity.Properties.ContainsKey(ToTableNameSingular)) 
                                             || (entityName == relationship.ToTableName) 
                                             || (SameTableCount > 1)) 
                                                ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} 1->1</summary>", prefix, relationship.Name));
                        writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, FieldName.Replace(" ", "")));
                        PreviousOneToOneFields.Add(FieldName);
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

                    var PreviousOneToOneFields = new List<string>();
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne); 
                    foreach (var relationship in RelationshipsOneToOne)
                    {
                        //Need to resolve the to table name to what the alias table name is
                        string ToTableName = entity.Parent.Entities[relationship.ToTableName].Alias;

                        int SameTableCount = RelationshipsOneToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = ToTableName.ToSingular();
                        string FieldName = ((PreviousOneToOneFields.Contains(ToTableNameSingular)
                                             || (entity.Properties.ContainsKey(ToTableNameSingular))
                                             || (entityName == relationship.ToTableName)
                                             || (SameTableCount > 1))
                                                ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}/// <summary>{1} 1->1</summary>", prefix, relationship.Name));
                        writer.WriteSafeString(string.Format("\n{0}public virtual {1} {2} {{ get; set; }}", prefix, ToTableNameSingular, FieldName.Replace(" ", "")));
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