using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbCodeGen.Core.Extentions.Objects;
using EzDbCodeGen.Core.Extentions.Strings;

namespace EzDbCodeGen.Core
{
    public class HandlebarsBlockHelperHandler {
        public void HandlebarsBlockHelperIfCond(TextWriter writer, HelperOptions options, dynamic context, params object[] arguments) {

        }
    }
    public static class HandlebarsUtility
    {
        public static void RegisterHelpers()
        {
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
                    writer.WriteSafeString((new SchemaObjectName(parameters[0].ToSafeString())).ObjectName);
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
            Handlebars.RegisterHelper("ToPlural", (writer, context, parameters) => {
                if (parameters.Count() > 0)
                {
                    writer.WriteSafeString(parameters[0].ToSafeString().ToPlural());
                }
            });
            Handlebars.RegisterHelper("ToNetType", (writer, context, parameters) => {
                var property = (IProperty)context;
                var isNullable = ((parameters.Count() >= 2) && (parameters[1].AsBoolean() != false)) || (property.IsNullable);
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
				var property = (IProperty)context;
                var isNullable = ((parameters.Count() >= 2) && (parameters[1].AsBoolean() != false)) || (property.IsNullable);
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

            Handlebars.RegisterHelper("EntityCustomAttribute", (writer, context, parameters) => {
                var entity = (IEntity)context;
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
				var entity = (IEntity)context;
                var entityName = entity.Name;

                if (entity.Properties.ContainsKey(PropertyToSearch))
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
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
				var entity = (IEntity)context;
                var entityName = entity.Name;

                var val1 = entity.Relationships.Count();
                switch (arguments.AsString(0))
                {
                    case ">":
                        if (val1 > val2)
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    case "=":
                    case "==":
                        if (val1.Equals(val2))
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    case "<":
                        if (val1 < val2)
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    case "!=":
                    case "<>":
                        if (!val1.Equals(val2))
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    default:
                       ErrorList.Add(string.Format("Operand of {0} is unknown.", arguments.AsString(0)));
                       break;
                }
                if (ErrorList.Count>0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())) );
                    
            });

            Handlebars.RegisterHelper("ifPropertyCustomAttributeCond", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('ifPropertyCustomAttributeCond')";
                var property = (IProperty)context;
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
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    case "=":
                    case "==":
                        if (val1.Equals(val2))
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    case "<":
                        if (val1.Length < val2.Length)
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
                        break;
                    case "!=":
                    case "<>":
                        if (!val1.Equals(val2))
                            options.Template(writer, (object)context);
                        else
                            options.Inverse(writer, (object)context);
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
                    options.Inverse(writer, (object)context);
                else
                    options.Template(writer, (object)context);
                if (ErrorList.Count > 0) writer.Write(string.Format("{0} Errors: {1}", PROC_NAME, string.Join("", ErrorList.ToList())));

            });

            Handlebars.RegisterHelper("ifCond", (TextWriter writer, HelperOptions options, dynamic context, object[] arguments) =>
            {
                if (arguments.Length != 3)
                {
                    writer.Write("ifCond:Wrong number of arguments");
                    return;
                }
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                {
                    writer.Write("ifCond:arguments[0] undefined");
                    return;
                }
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
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                        case "=":
                        case "==":
                            if (val1 == val2)
                            {
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                        case "<":
                            if (val1.Length < val2.Length)
                            {
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                        case "!=":
                        case "<>":
                            if (val1 != val2)
                            {
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
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
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                        case "=":
                        case "==":
                            if (val1.Equals(val2))
                            {
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                        case "<":
                            if (val1 < val2)
                            {
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                        case "!=":
                        case "<>":
                            if (!val1.Equals(val2))
                            {
                                options.Template(writer, (object)context);
                            }
                            else
                            {
                                options.Inverse(writer, (object)context);
                            }
                            break;
                    }
                }            
            });

            Handlebars.RegisterHelper("ifIsAuditableProperty", (writer, options, context, arguments) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('ifIsAuditableProperty')";
                var ErrorList = new List<string>();
                try
                {
                    var p = (IProperty)context;
                    if ((p.Parent.IsAuditable()) && (p.Parent.isAuditablePropertyName(p.Name)))
                        options.Inverse(writer, (object)context);
                    else
                        options.Template(writer, (object)context);
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
                    var e = (IEntity)context;
                    if (e.IsAuditable())
                        options.Template(writer, (object)context);
                    else
                        options.Inverse(writer, (object)context);
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