using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EzDbSchema.Core.Interfaces;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides documentation generation capabilities for Handlebars templates
    /// </summary>
    public class DocEz
    {
        /// <summary>
        /// Generates documentation for a schema object in the specified format
        /// </summary>
        /// <param name="obj">The object to document (IProperty, IRelationship, etc.)</param>
        /// <param name="format">The documentation format (xml, jsdoc)</param>
        /// <returns>Formatted documentation string</returns>
        public string GenerateDoc(object obj, string format)
        {
            if (obj is IProperty property)
            {
                return GeneratePropertyDoc(property, format);
            }
            else if (obj is IRelationship relationship)
            {
                return GenerateRelationshipDoc(relationship, format);
            }
            else if (obj is IEntity entity)
            {
                return GenerateEntityDoc(entity, format);
            }
            else
            {
                throw new ArgumentException($"Unsupported object type for documentation: {obj.GetType().Name}");
            }
        }

        private string GeneratePropertyDoc(IProperty property, string format)
        {
            StringBuilder sb = new StringBuilder();
            string description = property.CustomAttributes != null && property.CustomAttributes.ContainsKey("Description") 
                ? property.CustomAttributes["Description"]?.ToString() 
                : $"The {property.PropertyName} property";

            switch (format.ToLowerInvariant())
            {
                case "xml":
                    sb.AppendLine("/// <summary>");
                    sb.AppendLine($"/// {description}");
                    sb.AppendLine("/// </summary>");
                    
                    // Add remarks for special properties
                    List<string> remarks = new List<string>();
                    if (property.IsPrimaryKey) remarks.Add("Primary key");
                    if (property.IsIdentity) remarks.Add("Auto-incrementing");
                    if (property.IsRequired) remarks.Add("Required");
                    if (property.RequiresEncryption) remarks.Add("Requires encryption");
                    if (property.RequiresMasking) remarks.Add("Requires masking");
                    if (property.IsSensitive) remarks.Add("Contains sensitive data");
                    
                    if (remarks.Count > 0)
                    {
                        sb.AppendLine($"/// <remarks>{string.Join(", ", remarks)}</remarks>");
                    }
                    break;

                case "jsdoc":
                    sb.AppendLine("/**");
                    sb.AppendLine($" * {description}");
                    
                    // Add type annotation
                    string jsType = ConvertSqlTypeToJsType(property.DataType);
                    sb.AppendLine($" * @type {{{jsType}}}");
                    
                    // Add tags for special properties
                    if (property.IsPrimaryKey) sb.AppendLine(" * @primaryKey");
                    if (property.IsIdentity) sb.AppendLine(" * @autoIncrement");
                    if (property.IsRequired) sb.AppendLine(" * @required");
                    if (property.MaxLength > 0) sb.AppendLine($" * @maxLength {property.MaxLength}");
                    if (property.RequiresEncryption) sb.AppendLine(" * @encrypted");
                    if (property.RequiresMasking) sb.AppendLine(" * @masked");
                    if (property.IsSensitive) sb.AppendLine(" * @sensitive");
                    
                    sb.AppendLine(" */");
                    break;

                default:
                    throw new ArgumentException($"Unsupported documentation format: {format}");
            }

            return sb.ToString();
        }

        private string GenerateRelationshipDoc(IRelationship relationship, string format)
        {
            StringBuilder sb = new StringBuilder();
            string description = $"Relationship between {relationship.FromTableName} and {relationship.ToTableName}";

            switch (format.ToLowerInvariant())
            {
                case "xml":
                    sb.AppendLine("/// <summary>");
                    sb.AppendLine($"/// {description}");
                    sb.AppendLine("/// </summary>");
                    sb.AppendLine($"/// <remarks>{relationship.MultiplicityType.ToString().Replace("To", "-to-")} relationship ({relationship.ConstraintName})</remarks>");
                    break;

                case "jsdoc":
                    sb.AppendLine("/**");
                    sb.AppendLine($" * {description}");
                    sb.AppendLine($" * @relationship {relationship.MultiplicityType.ToString().Replace("To", "-to-")}");
                    sb.AppendLine($" * @constraintName {relationship.ConstraintName}");
                    sb.AppendLine($" * @fromTable {relationship.FromTableName}");
                    sb.AppendLine($" * @toTable {relationship.ToTableName}");
                    if (relationship.IsOptional) sb.AppendLine(" * @optional");
                    if (relationship.CascadeDelete) sb.AppendLine(" * @cascadeDelete");
                    sb.AppendLine(" */");
                    break;

                default:
                    throw new ArgumentException($"Unsupported documentation format: {format}");
            }

            return sb.ToString();
        }

        private string GenerateEntityDoc(IEntity entity, string format)
        {
            StringBuilder sb = new StringBuilder();
            string description = entity.CustomAttributes != null && entity.CustomAttributes.ContainsKey("Description") 
                ? entity.CustomAttributes["Description"]?.ToString() 
                : $"The {entity.TableName} entity";

            switch (format.ToLowerInvariant())
            {
                case "xml":
                    sb.AppendLine("/// <summary>");
                    sb.AppendLine($"/// {description}");
                    sb.AppendLine("/// </summary>");
                    
                    List<string> remarks = new List<string>();
                    remarks.Add($"Entity type: {entity.EntityType}");
                    if (entity.HasPrimaryKeys()) remarks.Add("Has primary keys");
                    if (entity.HasForeignKeyConstraints) remarks.Add("Has foreign key constraints");
                    if (entity.RequiresAuthorization) remarks.Add("Requires authorization");
                    if (entity.HasRowLevelSecurity) remarks.Add("Has row-level security");
                    if (entity.IsAuditable()) remarks.Add("Is auditable");
                    
                    if (remarks.Count > 0)
                    {
                        sb.AppendLine($"/// <remarks>{string.Join(", ", remarks)}</remarks>");
                    }
                    break;

                case "jsdoc":
                    sb.AppendLine("/**");
                    sb.AppendLine($" * {description}");
                    sb.AppendLine($" * @entity {entity.EntityType}");
                    sb.AppendLine($" * @table {entity.TableName}");
                    sb.AppendLine($" * @schema {entity.DatabaseSchema}");
                    
                    if (entity.HasPrimaryKeys()) sb.AppendLine(" * @hasPrimaryKeys");
                    if (entity.HasForeignKeyConstraints) sb.AppendLine(" * @hasForeignKeys");
                    if (entity.RequiresAuthorization) sb.AppendLine(" * @requiresAuthorization");
                    if (entity.HasRowLevelSecurity) sb.AppendLine(" * @hasRowLevelSecurity");
                    if (entity.IsAuditable()) sb.AppendLine(" * @isAuditable");
                    
                    sb.AppendLine(" */");
                    break;

                default:
                    throw new ArgumentException($"Unsupported documentation format: {format}");
            }

            return sb.ToString();
        }

        private string ConvertSqlTypeToJsType(string sqlType)
        {
            if (string.IsNullOrEmpty(sqlType))
                return "any";

            // Extract base type without size/precision
            string baseType = sqlType.Split('(')[0].ToLowerInvariant();

            switch (baseType)
            {
                case "int":
                case "bigint":
                case "smallint":
                case "tinyint":
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                case "float":
                case "real":
                    return "number";
                
                case "bit":
                    return "boolean";
                
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "date":
                case "datetimeoffset":
                    return "Date";
                
                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                case "text":
                case "ntext":
                case "xml":
                case "uniqueidentifier":
                case "time":
                    return "string";
                
                case "binary":
                case "varbinary":
                case "image":
                case "rowversion":
                case "timestamp":
                    return "Array<number>";
                
                default:
                    return "any";
            }
        }

        /// <summary>
        /// Registers the DocEz helper with Handlebars
        /// </summary>
        /// <param name="context">The Handlebars context</param>
        public static void RegisterHelper(IHandlebars context)
        {
            var docEz = new DocEz();

            context.RegisterHelper("DocEz", (writer, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: DocEz requires at least 2 arguments");
                    return;
                }

                object obj = arguments[0];
                string format = arguments[1]?.ToString() ?? "xml";

                if (obj == null)
                {
                    writer.WriteSafeString("Error: DocEz first argument cannot be null");
                    return;
                }

                try
                {
                    string result = docEz.GenerateDoc(obj, format);
                    writer.WriteSafeString(result);
                }
                catch (ArgumentException ex)
                {
                    writer.WriteSafeString($"Error: {ex.Message}");
                }
            });

            // Register a shorter alias
            context.RegisterHelper("doc", (writer, context, arguments) =>
            {
                if (arguments.Length < 2)
                {
                    writer.WriteSafeString("Error: doc requires at least 2 arguments");
                    return;
                }

                object obj = arguments[0];
                string format = arguments[1]?.ToString() ?? "xml";

                if (obj == null)
                {
                    writer.WriteSafeString("Error: doc first argument cannot be null");
                    return;
                }

                try
                {
                    string result = docEz.GenerateDoc(obj, format);
                    writer.WriteSafeString(result);
                }
                catch (ArgumentException ex)
                {
                    writer.WriteSafeString($"Error: {ex.Message}");
                }
            });
        }
    }
}
