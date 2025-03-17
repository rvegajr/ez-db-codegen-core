using HandlebarsDotNet;
using Pluralize.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Enums;

namespace EzDbCodeGen.HandlebarsTests
{
    public class HandlebarsUtility
    {
        private readonly IHandlebars _handlebars;

        /// <summary>
        /// Gets the underlying Handlebars instance.
        /// </summary>
        /// <returns>The Handlebars instance used by this utility.</returns>
        public IHandlebars GetHandlebars() => _handlebars;
        private readonly Pluralizer _pluralizer;

        public HandlebarsUtility()
        {
            _handlebars = Handlebars.Create();
            _pluralizer = new Pluralizer();
            var schemaHelpers = new HandlebarsSchemaHelpers(_handlebars);
            schemaHelpers.RegisterHelpers();
        }

        public HandlebarsTemplate<object, object> Compile(string template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            return _handlebars.Compile(template);
        }

        public void RegisterStringHelpers()
        {
            // Comparison helpers
            _handlebars.RegisterHelper("eq", (writer, context, parameters) => {
                if (parameters.Length != 2)
                    throw new HandlebarsException("{{eq}} helper requires exactly 2 arguments");
                
                writer.WriteSafeString(string.Equals(parameters[0].ToString(), parameters[1].ToString()) ? "true" : "");
            });

            _handlebars.RegisterHelper("ne", (writer, context, parameters) => {
                if (parameters.Length != 2)
                    throw new HandlebarsException("{{ne}} helper requires exactly 2 arguments");
                
                writer.WriteSafeString(!string.Equals(parameters[0].ToString(), parameters[1].ToString()) ? "true" : "");
            });

            _handlebars.RegisterHelper("lt", (writer, context, parameters) => {
                if (parameters.Length != 2)
                    throw new HandlebarsException("{{lt}} helper requires exactly 2 arguments");
                
                writer.WriteSafeString(string.Compare(parameters[0].ToString(), parameters[1].ToString()) < 0 ? "true" : "");
            });

            _handlebars.RegisterHelper("gt", (writer, context, parameters) => {
                if (parameters.Length != 2)
                    throw new HandlebarsException("{{gt}} helper requires exactly 2 arguments");
                
                writer.WriteSafeString(string.Compare(parameters[0].ToString(), parameters[1].ToString()) > 0 ? "true" : "");
            });

            _handlebars.RegisterHelper("le", (writer, context, parameters) => {
                if (parameters.Length != 2)
                    throw new HandlebarsException("{{le}} helper requires exactly 2 arguments");
                
                writer.WriteSafeString(string.Compare(parameters[0].ToString(), parameters[1].ToString()) <= 0 ? "true" : "");
            });

            _handlebars.RegisterHelper("ge", (writer, context, parameters) => {
                if (parameters.Length != 2)
                    throw new HandlebarsException("{{ge}} helper requires exactly 2 arguments");
                
                writer.WriteSafeString(string.Compare(parameters[0].ToString(), parameters[1].ToString()) >= 0 ? "true" : "");
            });

            // String manipulation helpers
            _handlebars.RegisterHelper("concat", (writer, context, parameters) => {
                if (parameters.Length < 1)
                    throw new HandlebarsException("{{concat}} helper requires at least 1 argument");
                
                var result = string.Concat(parameters.Select(p => p?.ToString() ?? string.Empty));
                writer.WriteSafeString(result);
            });

            _handlebars.RegisterHelper("lower", (writer, context, parameters) => {
                if (parameters.Length != 1)
                    throw new HandlebarsException("{{lower}} helper requires exactly 1 argument");
                
                writer.WriteSafeString(parameters[0]?.ToString()?.ToLower() ?? string.Empty);
            });

            _handlebars.RegisterHelper("upper", (writer, context, parameters) => {
                if (parameters.Length != 1)
                    throw new HandlebarsException("{{upper}} helper requires exactly 1 argument");
                
                writer.WriteSafeString(parameters[0]?.ToString()?.ToUpper() ?? string.Empty);
            });

            _handlebars.RegisterHelper("pascal", (writer, context, parameters) => {
                if (parameters.Length != 1)
                    throw new HandlebarsException("{{pascal}} helper requires exactly 1 argument");
                
                var input = parameters[0]?.ToString() ?? string.Empty;
                var words = Regex.Split(input, @"[\s_-]+")
                    .Where(word => !string.IsNullOrEmpty(word))
                    .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower());
                
                writer.WriteSafeString(string.Concat(words));
            });

            _handlebars.RegisterHelper("camel", (writer, context, parameters) => {
                if (parameters.Length != 1)
                    throw new HandlebarsException("{{camel}} helper requires exactly 1 argument");
                
                var input = parameters[0]?.ToString() ?? string.Empty;
                var words = Regex.Split(input, @"[\s_-]+")
                    .Where(word => !string.IsNullOrEmpty(word))
                    .ToArray();
                
                if (words.Length == 0)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }
                
                var result = words[0].ToLower();
                for (int i = 1; i < words.Length; i++)
                {
                    result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
                
                writer.WriteSafeString(result);
            });

            _handlebars.RegisterHelper("plural", (writer, context, parameters) => {
                if (parameters.Length != 1)
                    throw new HandlebarsException("{{plural}} helper requires exactly 1 argument");
                
                var input = parameters[0]?.ToString() ?? string.Empty;
                writer.WriteSafeString(_pluralizer.Pluralize(input));
            });

            _handlebars.RegisterHelper("singular", (writer, context, parameters) => {
                if (parameters.Length != 1)
                    throw new HandlebarsException("{{singular}} helper requires exactly 1 argument");
                
                var input = parameters[0]?.ToString() ?? string.Empty;
                writer.WriteSafeString(_pluralizer.Singularize(input));
            });

            _handlebars.RegisterHelper("replace", (writer, context, parameters) => {
                if (parameters.Length != 3)
                    throw new HandlebarsException("{{replace}} helper requires exactly 3 arguments");
                
                var input = parameters[0]?.ToString() ?? string.Empty;
                var oldValue = parameters[1]?.ToString() ?? string.Empty;
                var newValue = parameters[2]?.ToString() ?? string.Empty;
                
                writer.WriteSafeString(input.Replace(oldValue, newValue));
            });

            _handlebars.RegisterHelper("substring", (writer, context, parameters) => {
                if (parameters.Length < 2 || parameters.Length > 3)
                    throw new HandlebarsException("{{substring}} helper requires 2 or 3 arguments");
                
                var input = parameters[0]?.ToString() ?? string.Empty;
                if (!int.TryParse(parameters[1]?.ToString(), out var startIndex))
                    throw new HandlebarsException("{{substring}} helper's second argument must be a valid integer");
                
                if (parameters.Length == 2)
                {
                    if (startIndex >= 0 && startIndex < input.Length)
                        writer.WriteSafeString(input.Substring(startIndex));
                    else
                        writer.WriteSafeString(string.Empty);
                }
                else
                {
                    if (!int.TryParse(parameters[2]?.ToString(), out var length))
                        throw new HandlebarsException("{{substring}} helper's third argument must be a valid integer");
                    
                    if (startIndex >= 0 && startIndex < input.Length && length > 0)
                    {
                        length = Math.Min(length, input.Length - startIndex);
                        writer.WriteSafeString(input.Substring(startIndex, length));
                    }
                    else
                    {
                        writer.WriteSafeString(string.Empty);
                    }
                }
            });
        }

        public void RegisterModelPropertyHelpers()
        {
            // Register POCOModelProperties helper
            _handlebars.RegisterHelper("POCOModelProperties", (writer, context, arguments) => {
                if (context.Value is not IEntity entity)
                    return;

                var prefix = arguments.Length > 0 ? arguments[0]?.ToString() ?? "" : "";
                var includeAnnotations = arguments.Length > 1 && arguments[1] is bool b && b;

                try
                {
                    foreach (var propertyKV in entity.Properties)
                    {
                        var property = propertyKV.Value;
                        if (includeAnnotations)
                        {
                            if (!property.IsNullable)
                            {
                                writer.WriteSafeString($"\n{prefix}[Required]");
                            }
                            if (property.MaxLength > 0)
                            {
                                writer.WriteSafeString($"\n{prefix}[MaxLength({property.MaxLength})]");
                            }
                        }
                        writer.WriteSafeString($"\n{prefix}public {GetPropertyType(property)} {property.PropertyName} {{ get; set; }}");
                    }
                }
                catch (Exception ex)
                {
                    throw new HandlebarsException($"Error in POCOModelProperties helper: {ex.Message}", ex);
                }
            });
        }

        private void GeneratePropertyAnnotations(EncodedTextWriter writer, IProperty property, string prefix)
        {
            if (!property.IsNullable)
            {
                writer.WriteSafeString($"\n{prefix}[Required]");
            }
            if (property.MaxLength > 0)
            {
                writer.WriteSafeString($"\n{prefix}[MaxLength({property.MaxLength})]");
            }
        }

        private string GetPropertyType(IProperty property)
        {
            var type = GetCSharpType(property.DataType);
            if (property.IsNullable && !type.Contains("?"))
            {
                type += "?";
            }
            return type;
        }

        private string GetCSharpType(string dbType)
        {
            return dbType.ToLower() switch
            {
                "int" => "int",
                "bigint" => "long",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",
                "decimal" => "decimal",
                "money" => "decimal",
                "float" => "float",
                "real" => "double",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "date" => "DateTime",
                "time" => "TimeSpan",
                "char" => "string",
                "nchar" => "string",
                "varchar" => "string",
                "nvarchar" => "string",
                "text" => "string",
                "ntext" => "string",
                "binary" => "byte[]",
                "varbinary" => "byte[]",
                "uniqueidentifier" => "Guid",
                _ => "object"
            };
        }

        public void RegisterForeignKeyHelpers()
        {
            // Register POCOModelFKProperties helper
            _handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) => {
                var entity = (IEntity)context.Value;
                var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                var fkNametoSelect = parameters.Length > 1 ? parameters[1]?.ToString() ?? "" : "";

                try
                {
                    var relationships = entity.Relationships;
                    var relationshipsOneToOne = relationships.Where(r => r.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne);
                    var groupedByFKName = GroupRelationshipsByForeignKeyName(relationshipsOneToOne);

                    foreach (var relationshipGroupKV in groupedByFKName)
                    {
                        var relationship = CreateRelationshipSummary(relationshipGroupKV.Value);
                        if (relationship == null) continue;

                        if (string.IsNullOrEmpty(fkNametoSelect) || relationship.ConstraintName == fkNametoSelect)
                        {
                            string tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                            string endAsObjectPropertyName = relationship.ToColumnName.Replace($"{entity.DatabaseSchema}.", "");

                            writer.WriteSafeString($"\n{prefix}public virtual {ToSingular(tableAlias)} {endAsObjectPropertyName} {{ get; set; }}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HandlebarsException($"Error in POCOModelFKProperties helper: {ex.Message}", ex);
                }
            });
        }

        private Dictionary<string, List<IRelationship>> GroupRelationshipsByForeignKeyName(IEnumerable<IRelationship> relationships)
        {
            var result = new Dictionary<string, List<IRelationship>>();
            
            foreach (var relationship in relationships)
            {
                var key = relationship.ConstraintName;
                if (!result.ContainsKey(key))
                {
                    result[key] = new List<IRelationship>();
                }
                result[key].Add(relationship);
            }
            
            return result;
        }

        private RelationshipSummary CreateRelationshipSummary(List<IRelationship> relationships)
        {
            if (relationships == null || !relationships.Any())
                return new RelationshipSummary();

            var first = relationships.First();
            return new RelationshipSummary
            {
                ConstraintName = first.ConstraintName,
                FromTableName = first.FromTableName,
                ToTableName = first.ToTableName,
                FromColumnName = first.FromColumnName,
                ToColumnName = first.ToColumnName,
                MultiplicityType = first.MultiplicityType
            };
        }

        private string ToSingular(string word)
        {
            return _pluralizer.Singularize(word);
        }
    }
}

public class RelationshipSummary
{
    public string ConstraintName { get; set; } = string.Empty;
    public string FromTableName { get; set; } = string.Empty;
    public string ToTableName { get; set; } = string.Empty;
    public string FromColumnName { get; set; } = string.Empty;
    public string ToColumnName { get; set; } = string.Empty;
    public RelationshipMultiplicityType MultiplicityType { get; set; }
}
