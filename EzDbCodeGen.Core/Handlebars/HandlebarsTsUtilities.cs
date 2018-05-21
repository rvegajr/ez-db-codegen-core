using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using EzDbSchema;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Core
{
    public static class HandlebarsTsUtility
    {
        public static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("AsTSContructorProperty", (writer, context, parameters) => {
                var property = (Property)context;
                var prefix = parameters.AsString(0);
                if (property.Type.ToJsType(false) == "Date")
                {
                    writer.WriteSafeString(string.Format("\n{0}this.{1} = item.{1} ? new Date(item.{1}) : null;", prefix, property.Name));
                }
                else
                {
                    writer.WriteSafeString(string.Format("\n{0}this.{1} = item.{1};", prefix, property.Name));
                }
            });

            Handlebars.RegisterHelper("TSModelBaseImportDeclarations", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('TSModelBaseImportDeclarations')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Name;

                    List<string> PreviousManyToOneFields = new List<string>();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);
                    foreach (Relationship relationship in RelationshipsManyToOne)
                    {
                        if (!PreviousManyToOneFields.Contains(relationship.ToTableName))
                        {
                            writer.WriteSafeString(string.Format("\n{0}import {{ {1} }} from '../{2}';", prefix, relationship.ToTableName.ToSingular(), relationship.ToTableName.ToSingular().ToSnakeCase()));
                            PreviousManyToOneFields.Add(relationship.ToTableName);
                        }
                    }
                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);
                    foreach (Relationship relationship in RelationshipsOneToMany)
                    {
                        if (!PreviousManyToOneFields.Contains(relationship.ToTableName))
                        {
                            writer.WriteSafeString(string.Format("\n{0}import {{ {1} }} from '../{2}';", prefix, relationship.ToTableName.ToSingular(), relationship.ToTableName.ToSingular().ToSnakeCase()));
                            PreviousManyToOneFields.Add(relationship.ToTableName);
                        }
                    }
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne);
                    foreach (Relationship relationship in RelationshipsOneToOne)
                    {
                        if (!PreviousManyToOneFields.Contains(relationship.ToTableName))
                        {
                            writer.WriteSafeString(string.Format("\n{0}import {{ {1} }} from '../{2}';", prefix, relationship.ToTableName.ToSingular(), relationship.ToTableName.ToSingular().ToSnakeCase()));
                            PreviousManyToOneFields.Add(relationship.ToTableName);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("TSModelBaseRelatedProperties", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('TSModelBaseRelatedProperties')";
                try
                {
                    var prefix = parameters.AsString(0);
					var entity = (Entity)context;
                    var entityName = entity.Name;

                    List<string> PreviousManyToOneFields = new List<string>();
                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);
                    foreach (Relationship relationship in RelationshipsOneToMany.ToList())
                    {
                        writer.WriteSafeString(string.Format("\n{0}{1}?: Array<{2}> | null;", prefix, (relationship.ToTableName + "_" + relationship.ToColumnName), relationship.ToTableName.ToSingular()));
                    }

                    PreviousManyToOneFields.Clear();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);
					foreach (Relationship relationship in RelationshipsManyToOne.ToList())
                    {

						int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.ToSingular();
                        string FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}{1}?: {2};", prefix, FieldName, relationship.ToTableName.ToSingular()));
                        PreviousManyToOneFields.Add(FieldName);
                    }

                    PreviousManyToOneFields.Clear();
                    var RelationshipsOneToOne = entity.Relationships.Fetch(RelationshipType.OneToOne );
					foreach (Relationship relationship in RelationshipsOneToOne.ToList())
                    {

						int SameTableCount = RelationshipsOneToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.ToSingular();
                        string FieldName = ((PreviousManyToOneFields.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}{1}?: {2};", prefix, FieldName, relationship.ToTableName.ToSingular()));
                        PreviousManyToOneFields.Add(FieldName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("TSModelBaseConstructorRelatedDefs", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('TSModelBaseConstructorRelatedDefs')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Name;

                    List<string> PreviousFieldCheck = new List<string>();
                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);
					foreach (Relationship relationship in RelationshipsOneToMany.ToList())
                    {
                        writer.WriteSafeString(string.Format("\n{0}this.{1} = Helpers.createClassArray({2}, item.{3});",
                                                             prefix,
                                                             (relationship.ToTableName + "_" + relationship.ToColumnName),
                                                             relationship.ToTableName.ToSingular(),
                                                             relationship.ToTableName + "_" + relationship.ToColumnName
                                                            ));
                        PreviousFieldCheck.Add(relationship.ToTableName);
                    }

                    PreviousFieldCheck.Clear();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);
					foreach (Relationship relationship in RelationshipsManyToOne.ToList())
                    {

						int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.ToSingular();
                        string FieldName = ((PreviousFieldCheck.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}this.{1} = new {2}(item.{3});", prefix, FieldName, relationship.ToTableName.ToSingular(), FieldName));
                        PreviousFieldCheck.Add(FieldName);
                    }

                    PreviousFieldCheck.Clear();
                    var RelationshipsOneToOne2 = entity.Relationships.Fetch(RelationshipType.OneToOne);
					foreach (Relationship relationship in RelationshipsOneToOne2.ToList())
                    {

						int SameTableCount = RelationshipsOneToOne2.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.ToSingular();
                        string FieldName = ((PreviousFieldCheck.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}this.{1} = new {2}(item.{3});", prefix, FieldName, relationship.ToTableName.ToSingular(), FieldName));
                        PreviousFieldCheck.Add(FieldName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });


            Handlebars.RegisterHelper("TSModelInterfaceImports", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('TSModelInterfaceImports')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Name;

                    List<string> PreviousManyToOneFields = new List<string>();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);

                    writer.WriteSafeString(string.Format("\n{0}// MANY TO ZERO OR ONE", prefix));
                    foreach (Relationship relationship in RelationshipsManyToOne)
                    {
                        if (!PreviousManyToOneFields.Contains(relationship.ToTableName))
                        {
                            writer.WriteSafeString(string.Format("\n{0}import {{ I{1} }} from './i{2}';", prefix, relationship.ToTableName.ToSingular(), relationship.ToTableName.ToSingular().ToSnakeCase()));
                            PreviousManyToOneFields.Add(relationship.ToTableName);
                        }
                    }

                    writer.WriteSafeString(string.Format("\n{0}// ZERO OR ONE TO MANY", prefix));
                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);
                    foreach (Relationship relationship in RelationshipsOneToMany)
                    {
                        if (!PreviousManyToOneFields.Contains(relationship.ToTableName))
                        {
                            writer.WriteSafeString(string.Format("\n{0}import {{ I{1} }} from './i{2}';", prefix, relationship.ToTableName.ToSingular(), relationship.ToTableName.ToSingular().ToSnakeCase()));
                            PreviousManyToOneFields.Add(relationship.ToTableName);
                        }
                    }
                    writer.WriteSafeString(string.Format("\n{0}// ONE TO ZERO OR ONE", prefix));

                    var RelationshipsOneOne = entity.Relationships.Fetch(RelationshipType.OneToOne);
                    foreach (Relationship relationship in RelationshipsOneOne)
                    {
                        if (!PreviousManyToOneFields.Contains(relationship.ToTableName))
                        {
                            writer.WriteSafeString(string.Format("\n{0}import {{ I{1} }} from './i{2}';", prefix, relationship.ToTableName.ToSingular(), relationship.ToTableName.ToSingular().ToSnakeCase()));
                            PreviousManyToOneFields.Add(relationship.ToTableName);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });

            Handlebars.RegisterHelper("TSModelInterfaceRelatedProperties", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('TSModelInterfaceRelatedProperties')";
                try
                {
                    var prefix = parameters.AsString(0);
                    var entity = (IEntity)context;
                    var entityName = entity.Name;

                    var RelationshipsOneToMany = entity.Relationships.Fetch(RelationshipType.ZeroOrOneToMany);
                    List<string> PreviousFieldCheck = new List<string>();
                    PreviousFieldCheck.Clear();
                    foreach (Relationship relationship in RelationshipsOneToMany)
                    {
                        writer.WriteSafeString(string.Format("\n{0}{1}<t/>?: I{2}[] | null;", prefix, (relationship.ToTableName + "_" + relationship.ToColumnName), relationship.ToTableName.ToSingular()));
                        PreviousFieldCheck.Add(relationship.ToTableName);
                    }


                    PreviousFieldCheck.Clear();
                    var RelationshipsManyToOne = entity.Relationships.Fetch(RelationshipType.ManyToZeroOrOne);
                    foreach (Relationship relationship in RelationshipsManyToOne )
                    {

						int SameTableCount = RelationshipsManyToOne.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.ToSingular();
                        string FieldName = ((PreviousFieldCheck.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}{1}?: I{2};", prefix, FieldName, relationship.ToTableName.ToSingular()));
                        PreviousFieldCheck.Add(FieldName);
                    }

                    List<string> PreviousOneToOneFields2 = new List<string>();
                    var RelationshipsOneToOne2 = entity.Relationships.Fetch(RelationshipType.OneToOne);
                    foreach (Relationship relationship in RelationshipsOneToOne2 )
                    {

						int SameTableCount = RelationshipsOneToOne2.CountItems(RelationSearchField.ToTableName, relationship.ToTableName);
                        string ToTableNameSingular = relationship.ToTableName.ToSingular();
                        string FieldName = ((PreviousOneToOneFields2.Contains(ToTableNameSingular) || (entity.Properties.ContainsKey(ToTableNameSingular)) || (entityName == relationship.ToTableName) || (SameTableCount > 1)) ? relationship.FromColumnName : ToTableNameSingular);
                        writer.WriteSafeString(string.Format("\n{0}{1}?: I{2};", prefix, FieldName, relationship.ToTableName.ToSingular()));
                        PreviousOneToOneFields2.Add(FieldName);
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