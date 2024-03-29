﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbCodeGen.Core.Extentions.Objects;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Classes;
using EzDbSchema.Core.Objects;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EzDbSchema.Core.Enums;
using System.Runtime.CompilerServices;
using System.Diagnostics;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core
{
    internal class HandlebarsBlockHelperHandler {
        public void HandlebarsBlockHelperIfCond(TextWriter writer, HelperOptions options, dynamic context, params object[] arguments) {

        }
    }
    internal static class HandlebarsUtility
    {
        public static void RegisterHelpers()
        {
            Handlebars.RegisterHelper("Debugger", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    var prefix = parameters.AsString(0);
                    writer.WriteSafeString(prefix);
                }
            });

            Handlebars.RegisterHelper("ContextAsJson", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('ContextAsJson')";
                IRelationship relationship;
                IRelationshipList relationshipList;
                IEntity entity;
                RelationshipSummary relationshipListSummary;
                var json = "";
                var entityStringContains = parameters.AsString(0);
                try
                {
                    var ErrorList = new List<string>();
                    var contextObject = (Object)context;

                    if (contextObject.GetType().Name == "Relationship")
                    {
                        relationship = ((IRelationship)context.Value);
                        entity = relationship.Parent;
                        json = JsonConvert.SerializeObject((IRelationship)context.Value, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                                {PreserveReferencesHandling = PreserveReferencesHandling.All});
                    }
                    else if (contextObject.GetType().Name == "Entity")
                    {
                        entity = ((IEntity)context.Value);
                        json = JsonConvert.SerializeObject(entity, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                        { PreserveReferencesHandling = PreserveReferencesHandling.All });
                    }
                    else if (contextObject.GetType().Name == "RelationshipList")
                    {
                        relationshipList = ((IRelationshipList)context.Value);
                        relationshipListSummary = relationshipList.AsSummary();
                        entity = relationshipListSummary.Entity;
                        json = JsonConvert.SerializeObject((IRelationshipList)context.Value, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                                { PreserveReferencesHandling = PreserveReferencesHandling.All});
                    }
                    else
                    {
                        entity = new Entity();
                        entity.Name += "";
                    }
                    if (entity.Name.Contains(entityStringContains))
                    {
                        entity.Name += "";
                    } else
                    {
                        json = ""; //clear it out so we do not write it
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(PROC_NAME + "- Error! " + ex.Message);
                    writer.WriteSafeString("**** ERROR RENDERING " + PROC_NAME + ".  " + ex.Message);
                }
            });
            Handlebars.RegisterHelper("Prefix", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    var prefix = parameters.AsString(0);
                    writer.WriteSafeString(prefix);
                }
            });
            Handlebars.RegisterHelper("ExtractTableName", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString((new SchemaObjectName(parameters[0].ToSafeString())).TableName);
                }
            });
            Handlebars.RegisterHelper("ExtractSchemaName", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString((new SchemaObjectName(parameters[0].ToSafeString())).SchemaName);
                }
            });
            Handlebars.RegisterHelper("ToSingular", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToSingular());
                }
            });
            Handlebars.RegisterHelper("Comma", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    if (parameters[0].AsInt(0) > 0)
                    {
                        writer.WriteSafeString(",");
                    }
                }
            });

            Handlebars.RegisterHelper("ToPlural", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToPlural());
                }
            });
            Handlebars.RegisterHelper("ToNetType", (writer, context, parameters) => {
                var property = (IProperty)context.Value;
                var isNullable = ((parameters.Count() >= 2) && (parameters[1].AsBoolean() != false)) || (property.IsNullableResolved());
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToNetType(isNullable));
                }
            });

            Handlebars.RegisterHelper("ToCodeFriendly", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToCodeFriendly());
                }
            });
            Handlebars.RegisterHelper("PropertyNameSuffix", (writer, context, parameters) => {
                //Used to append text if the property already exists 
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToCodeFriendly());
                }
            });
            Handlebars.RegisterHelper("ToJsType", (writer, context, parameters) => {
				var property = (IProperty)context.Value;
                var isNullable = ((parameters.Count() >= 2) && (parameters[1].AsBoolean() != false)) || (property.IsNullableResolved());
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToJsType(isNullable));
                }
            });
            Handlebars.RegisterHelper("AsFormattedName", (writer, context, parameters) => {
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().AsFormattedName());
                }
            });
            Handlebars.RegisterHelper("ToSnakeCase", (writer, context, parameters) => {
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToSnakeCase());
                }
            });

            Handlebars.RegisterHelper("ToSingularSnakeCase", (writer, context, parameters) => {
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToSingular().ToSnakeCase());
                }
            });

            Handlebars.RegisterHelper("ToTitleCaseSafeFileName", (writer, context, parameters) => {
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToTitleCase().ToSafeFileName());
                }
            });

            Handlebars.RegisterHelper("ToCsObjectName", (writer, context, parameters) => {
                if (parameters.Count() >= 1)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToCsObjectName());
                }
            });

            Handlebars.RegisterHelper("StringFormat", (writer, context, parameters) => {
                if (parameters.Count() != 2)
                {
                    writer.WriteSafeString("Warning: StringFormat needs to have 2 parameters, [0]=StringToActOn, [1]='lower,upper,snake,title,pascal,trim,plural,single,nettype,jstype,table,schema' ");
                }
                else
                {
                    var strToFormat = parameters[0].ToSafeString();
                    var arrActions = parameters[1].ToSafeString().Split(',');
                    foreach(var _action in arrActions)
                    {
                        var action = _action.ToLower().Trim();
                        if (action.Equals("lower"))
                            strToFormat = strToFormat.ToLower();
                        else if (action.Equals("upper"))
                            strToFormat = strToFormat.ToUpper();
                        else if (action.Equals("snake"))
                            strToFormat = strToFormat.ToSnakeCase();
                        else if (action.Equals("title"))
                            strToFormat = strToFormat.ToTitleCase();
                        else if (action.Equals("pascal"))
                            strToFormat = strToFormat.ToPascalCase();
                        else if (action.Equals("trim"))
                            strToFormat = strToFormat.Trim();
                        else if (action.Equals("plural"))
                            strToFormat = strToFormat.ToPlural();
                        else if (action.Equals("single"))
                            strToFormat = strToFormat.ToSingular();
                        else if (action.Equals("nettype"))
                            strToFormat = strToFormat.ToNetType();
                        else if (action.Equals("jstype"))
                            strToFormat = strToFormat.ToJsType();
                        else if (action.Equals("table"))
                            strToFormat = (new SchemaObjectName(strToFormat.ToSafeString())).TableName;
                        else if (action.Equals("schema"))
                            strToFormat = (new SchemaObjectName(strToFormat.ToSafeString())).SchemaName;
                        else if (action.Equals("sentence"))
                            strToFormat = strToFormat.ToSentenceCase();
                    }
                    writer.WriteSafeString(strToFormat);
                }
            });

            Handlebars.RegisterHelper("EntityCustomAttribute", (writer, context, parameters) => {
                var entity = (IEntity)context.Value;
                var entityName = entity.Name;
                var customAttributeToFind = parameters[0].ToSafeString();
                if ((parameters.Count() >= 1) && (entity.CustomAttributes.ContainsKey(customAttributeToFind)))
                {
                    writer.WriteSafeString(  entity.CustomAttributes[customAttributeToFind].ToSafeString().ToUpper().Replace("US_", ""));
                }
            });

            //Much thanks to https://stackoverflow.com/questions/30933956/handlebars-net-if-comparision
            Handlebars.RegisterHelper("IfPropertyExists", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('IfPropertyExists')";
                if (arguments.Length != 1)
                {
                    writer.Write(PROC_NAME + ":Wrong number of arguments");
                    return;
                }
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                {
                    writer.Write(PROC_NAME + ":args[0] undefined");
                    return;
                }
                var PropertyToSearch = arguments.AsString(0);
				var entity = (IEntity)context.Value;
                var entityName = entity.Name;

                if (entity.Properties.ContainsKey(PropertyToSearch))
                {
                    options.Template(writer, (object)context.Value);
                }
                else
                {
                    options.Inverse(writer, (object)context.Value);
                }


            });

            Handlebars.RegisterHelper("isRelationshipCount", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('isRelationshipCount')";
                var ErrorList = new List<string>();
                if (arguments.Length != 2) ErrorList.Add("Wrong number of arguments. ");
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[0] is undefined. ");
                if (arguments[1] == null || arguments[1].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[1] is undefined. ");

                var val2 = arguments.AsString(1).AsInt(0);
				var entity = (IEntity)context.Value;
                var entityName = entity.Name;

                var val1 = entity.Relationships.Count();
                switch (arguments.AsString(0))
                {
                    case ">":
                        if (val1 > val2)
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    case "=":
                    case "==":
                        if (val1.Equals(val2))
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    case "<":
                        if (val1 < val2)
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    case "!=":
                    case "<>":
                        if (!val1.Equals(val2))
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    default:
                       ErrorList.Add(string.Format("Operand of {0} is unknown.", arguments.AsString(0)));
                       break;
                }
                if (ErrorList.Count>0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())) );
                    
            });

            Handlebars.RegisterHelper("isRelationshipTypeOf", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('isRelationshipTypeOf')";
                var ErrorList = new List<string>();
                if (arguments.Length != 1) ErrorList.Add("Wrong number of arguments. ");
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[0] is undefined and should be one of OneToMany, ZeroOrOneToMany, ZeroOrOneToManyOnly, ManyToOne, ManyToZeroOrOne, ManyToZeroOrOneOnly, OneToOne, OneToZeroOrOne, OneToZeroOrOneOnly, ZeroOrOneToOne, ZeroOrOneToOneOnly");

                var relationshipType = arguments.AsString(0);
                var multiplicityType = RelationshipMultiplicityType.Unknown;
                var contextObject = (Object)context.Value;
                if (contextObject.GetType().Name=="Relationship")
                    multiplicityType = ((IRelationship)context.Value).MultiplicityType;
                else if (contextObject.GetType().Name == "RelationshipList")
                    multiplicityType = ((IRelationshipList)context.Value).AsSummary().MultiplicityType;

                var isType = false;
                if (relationshipType.ToLower().Equals("onetoone")) isType = (multiplicityType == RelationshipMultiplicityType.OneToOne);
                if (relationshipType.ToLower().Equals("onetomany")) isType = (multiplicityType == RelationshipMultiplicityType.OneToMany);
                if (relationshipType.ToLower().Equals("zerooronetomany")) isType = ((multiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany) ||
                                                                                    (multiplicityType == RelationshipMultiplicityType.OneToMany));
                if (relationshipType.ToLower().Equals("zerooronetomanyonly")) isType = ((multiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany));
                if (relationshipType.ToLower().Equals("manytoone")) isType = (multiplicityType == RelationshipMultiplicityType.ManyToOne);
                if (relationshipType.ToLower().Equals("manytozeroorone")) isType = ((multiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne) ||
                                                                                    (multiplicityType == RelationshipMultiplicityType.ManyToOne));
                if (relationshipType.ToLower().Equals("manytozerooroneonly")) isType = (multiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne);
                if (relationshipType.ToLower().Equals("zerooronetoone")) isType = ((multiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne) ||
                                                                                       (multiplicityType == RelationshipMultiplicityType.OneToOne));
                if (relationshipType.ToLower().Equals("zerooronetooneonly")) isType = (multiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne);
                if (relationshipType.ToLower().Equals("onetozeroorone")) isType = ((multiplicityType == RelationshipMultiplicityType.OneToZeroOrOne) ||
                                                                                   (multiplicityType == RelationshipMultiplicityType.OneToOne));
                if (relationshipType.ToLower().Equals("onetozerooroneonly")) isType = (multiplicityType == RelationshipMultiplicityType.OneToZeroOrOne);

                if (isType)
                    options.Template(writer, (object)context.Value);
                else
                    options.Inverse(writer, (object)context.Value);

                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));
            });


            //entity.Parent.Entities[relGroupSummary.ToTableName].Alias
            Handlebars.RegisterHelper("ToTargetEntityAlias", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('ToTargetEntityAlias')";
                var ErrorList = new List<string>();
                var contextObject = (Object)context.Value;

                if (contextObject.GetType().Name == "Relationship")
                {
                    var relationship = ((IRelationship)context.Value);
                    writer.WriteSafeString(relationship.Parent.Parent.Entities[relationship.ToTableName].Alias);

                }
                else if (contextObject.GetType().Name == "RelationshipList")
                {
                    var relationshipListSummary = ((IRelationshipList)context.Value).AsSummary();
                    writer.WriteSafeString(relationshipListSummary.Entity.Parent.Entities[relationshipListSummary.ToTableName].Alias);
                }
                else
                {
                    ErrorList.Add(string.Format("Cannot handle class of type", contextObject.GetType().Name));
                }
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));
            });

            Handlebars.RegisterHelper("ToUniqueColumnName", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('ToUniqueColumnName')";
                var ErrorList = new List<string>();
                var isPlural = ((parameters.Count() >= 1) && (parameters[0].ToString().ToLower().Equals("plural")));
                if (parameters.Count() >= 1)
                {
                    var contextObject = (Object)context.Value;
                    if (contextObject.GetType().Name == "Relationship")
                    {
                        var relationship = ((IRelationship)context.Value);
                        if (isPlural)
                            writer.WriteSafeString(relationship.ToUniqueColumnName().ToPlural());
                        else
                            writer.WriteSafeString(relationship.ToUniqueColumnName());
                    }
                    else if (contextObject.GetType().Name == "RelationshipList")
                    {
                        var relationshipListSummary = ((IRelationshipList)context.Value).AsSummary();
                        if (isPlural)
                            writer.WriteSafeString(relationshipListSummary.ToUniqueColumnName().ToPlural());
                        else
                            writer.WriteSafeString(relationshipListSummary.ToUniqueColumnName());
                    } else
                    {
                        ErrorList.Add(string.Format("Cannot handle class of type", contextObject.GetType().Name));
                    }
                }
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));
            });

            Handlebars.RegisterHelper("ifPropertyCustomAttributeCond", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('ifPropertyCustomAttributeCond')";
                var property = (IProperty)context.Value;
                var CustomAttributeName = arguments.AsString(0);

                var ErrorList = new List<string>();
                if (arguments.Length != 3) ErrorList.Add("Wrong number of arguments. ");
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[0] is undefined. ");
                if (arguments[1] == null || arguments[1].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[1] is undefined. ");
                if (arguments[2] == null || arguments[1].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[2] is undefined. ");
                if (!property.CustomAttributes.ContainsKey(CustomAttributeName))
                    ErrorList.Add(string.Format("Custom Attribute of {0) cannot be found.", CustomAttributeName));


                var val1 = property.CustomAttributes[CustomAttributeName].ToSafeString();
                var val2 = arguments.AsString(2);

                switch (arguments.AsString(1))
                {
                    case ">":
                        if (val1.Length > val2.Length)
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    case "=":
                    case "==":
                        if (val1.Equals(val2))
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    case "<":
                        if (val1.Length < val2.Length)
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    case "!=":
                    case "<>":
                        if (!val1.Equals(val2))
                            options.Template(writer, (object)context.Value);
                        else
                            options.Inverse(writer, (object)context.Value);
                        break;
                    default:
                        ErrorList.Add(string.Format("Operand of {0} is unknown.", arguments.AsString(1)));
                        break;
                }
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));

            });


            Handlebars.RegisterHelper("isNotInList", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('isInList')";
                var CustomAttributeName = arguments.AsString(0);

                var ErrorList = new List<string>();
                if (arguments.Length < 2) ErrorList.Add("Wrong number of arguments. ");
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[0] is undefined. ");
                if (arguments[1] == null || arguments[1].GetType().Name == "UndefinedBindingResult")
                    ErrorList.Add("args[1] is undefined. ");


                var val1 = arguments.AsString(0);
                var listToCheck = new List<string>();
                var i = 0;
                foreach (var s in arguments)
                {
                    if (i > 0) listToCheck.Add(s.ToSafeString());
                    i++;
                }
                if (listToCheck.Contains(val1))
                    options.Inverse(writer, (object)context.Value);
                else
                    options.Template(writer, (object)context.Value);
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));

            });
            Handlebars.RegisterHelper("ifNot", (writer, options, context, arguments) =>
            {
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                {
                    writer.Write("ifNot:arguments[0] undefined");
                    return;
                }
                if (!arguments[0].AsBoolean())
                {
                    options.Template(writer, (object)context.Value);
                }
                else
                {
                    options.Inverse(writer, (object)context.Value);
                }

            });

            Handlebars.RegisterHelper("ifCond", (writer, options, context, arguments) =>
            {
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                {
                    writer.Write("ifCond:arguments[0] undefined");
                    return;
                }

                if (arguments.Length == 3)
                {
                    if (arguments[1] == null || arguments[1].GetType().Name == "UndefinedBindingResult")
                    {
                        writer.Write("ifCond:arguments[1] undefined");
                        return;
                    }
                    if (arguments[2] == null || arguments[2].GetType().Name == "UndefinedBindingResult")
                    {
                        writer.Write("ifCond:arguments[2] undefined");
                        return;
                    }
                    if (arguments[0].GetType().Name == "String")
                    {
                        var val1 = arguments[0].ToString();
                        var val2 = arguments[2].ToString();

                        switch (arguments[1].ToString())
                        {  // '>', '=', '==', '<', '!=', '<>'
                            case ">":
                                if (val1.Length > val2.Length)
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                            case "=":
                            case "==":
                                if (val1 == val2)
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                            case "<":
                                if (val1.Length < val2.Length)
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                            case "!=":
                            case "<>":
                                if (val1 != val2)
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                        }
                    }
                    else
                    {
                        var val1 = float.Parse(arguments[0].ToString());
                        var val2 = float.Parse(arguments[2].ToString());

                        switch (arguments[1].ToString())
                        {
                            case ">":
                                if (val1 > val2)
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                            case "=":
                            case "==":
                                if (val1.Equals(val2))
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                            case "<":
                                if (val1 < val2)
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                            case "!=":
                            case "<>":
                                if (!val1.Equals(val2))
                                {
                                    options.Template(writer, (object)context.Value);
                                }
                                else
                                {
                                    options.Inverse(writer, (object)context.Value);
                                }
                                break;
                        }
                    }
                }
                else if (arguments.Length == 1)
                {
                    if (arguments[0].AsBoolean())
                    {
                        options.Template(writer, (object)context.Value);
                    }
                    else
                    {
                        options.Inverse(writer, (object)context.Value);
                    }
                }
                else
                {
                    writer.Write("ifCond: Wrong number of arguments :(");
                }
            });

            Handlebars.RegisterHelper("ifIsAuditableProperty", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('ifIsAuditableProperty')";
                var ErrorList = new List<string>();
                try
                {
                    var p = (IProperty)context.Value;
                    var entityName = p.Parent.Name;

                    if ((p.Parent.IsAuditable()) && (p.Parent.isAuditablePropertyName(p.Name)))
                        options.Inverse(writer, (object)context.Value);
                    else
                        options.Template(writer, (object)context.Value);
                }
                catch (Exception ex)
                {
                    ErrorList.Add(ex.StackTrace.ToString());
                }
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));


            });

            Handlebars.RegisterHelper("ifIsAuditable", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('ifIsAuditable')";
                var ErrorList = new List<string>();
                try
                {
                    var e = (IEntity)context.Value;
                    if (e.IsAuditable())
                        options.Template(writer, (object)context.Value);
                    else
                        options.Inverse(writer, (object)context.Value);
                }
                catch (Exception ex)
                {
                    ErrorList.Add(ex.StackTrace.ToString());
                }
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));
            });

            Handlebars.RegisterHelper("ifIsNotIgnored", (writer, options, context, arguments) =>
            {
                var ErrorList = new List<string>();
                var PROC_NAME = "Handlebars.RegisterHelper('ifIsNotIgnored')";
                IEntity entity;
                IProperty property;

                try
                {
                    var IsIgnored = false;
                    var contextObject = (Object)context;

                    if (contextObject.GetType().Name == "Property")
                    {
                        property = ((IProperty)context.Value);
                        if ((property.CustomAttributes != null) && (property.CustomAttributes.ContainsKey("IsIgnored"))) IsIgnored = property.CustomAttributes["IsIgnored"].AsBoolean();
                    }
                    else if (contextObject.GetType().Name == "Entity")
                    {
                        entity = ((IEntity)context.Value);
                        if ((entity.CustomAttributes != null) && entity.CustomAttributes.ContainsKey("IsIgnored")) IsIgnored = entity.CustomAttributes["IsIgnored"].AsBoolean();
                    } 
                    else
                    {
                        ErrorList.Add(string.Format("Could not render object with context of {0}", contextObject.GetType().Name));
                    }

                    if (!IsIgnored)
                        options.Template(writer, (object)context.Value);
                    else
                        options.Inverse(writer, (object)context.Value);
                }
                catch (Exception ex)
                {
                    ErrorList.Add(ex.StackTrace.ToString());
                }
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));
            });
        }
    }
}