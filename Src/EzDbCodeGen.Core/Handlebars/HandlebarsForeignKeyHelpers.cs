using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Extentions;
using EzDbSchema.Core.Enums;
using EzDbCodeGen.Core.Extensions;
using HandlebarsDotNet;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides Handlebars helpers for working with foreign key relationships.
    /// </summary>
    public static class HandlebarsForeignKeyHelpers
    {
        /// <summary>
        /// Registers all foreign key related helpers with the Handlebars instance.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register helpers with.</param>
        public static void RegisterHelpers(IHandlebars handlebars)
        {
            // Helper for generating POCO model foreign key properties
            handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) => {
                if (context.Value is not IEntity entity)
                    return;

                var procName = $"Handlebars.RegisterHelper('POCOModelFKProperties', Entity='{entity.TableName}')";

                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() ?? "" : "";

                    var relationships = entity.Relationships;
                    if (relationships == null || !relationships.Any())
                        return;

                    var oneToOneRelationships = relationships.Where(r => r.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne).ToList();
                    
                    // Instead of grouping by ForeignKeyName, we'll just process each relationship
                    foreach (var relationship in oneToOneRelationships)
                    {
                        if (relationship is null) 
                            continue;

                        // Skip if we're looking for a specific FK name and this isn't it
                        if (!string.IsNullOrEmpty(fkNameToSelect) && !string.Equals(relationship.ConstraintName, fkNameToSelect))
                            continue;

                        string tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                        string endAsObjectPropertyName = relationship.ToColumnName.Replace($"{entity.DatabaseSchema}.", "");

                        // Use EzDbSchema.Core.Extentions explicitly
                        string singularTableName = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(tableAlias);
                        writer.WriteSafeString($"\n{prefix}public virtual {singularTableName} {endAsObjectPropertyName} {{ get; set; }}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for generating POCO model many-to-zero-or-one relationships
            handlebars.RegisterHelper("POCOModelFKManyToZeroToOne", (writer, context, parameters) => {
                var procName = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOne')";
                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    
                    IEntity entity = context.Value switch
                    {
                        IRelationship rel => rel.ParentEntity,
                        IEntity ent => ent,
                        _ => throw new ArgumentException($"{procName}: Unable to get entity from context")
                    };

                    var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() ?? "" : "";

                    // Process parameters
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i]?.ToString() ?? "";
                        if (i == 0)
                        {
                            prefix = param;
                        }
                        else if (entity.RelationshipGroups != null && entity.RelationshipGroups.ContainsKey(param))
                        {
                            fkNameToSelect = param;
                        }
                    }

                    // Track fields that have been processed
                    var processedFields = new HashSet<string>();

                    // Get relationships
                    var relationships = entity.Relationships;
                    if (relationships == null || !relationships.Any())
                        return;
                        
                    var oneToOneRelationships = relationships.Where(r => r.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne).ToList();
                    
                    // Process each relationship individually instead of grouping
                    foreach (var rel in oneToOneRelationships)
                    {
                        if (rel is null) 
                            continue;

                        // Skip if we're looking for a specific FK name and this isn't it
                        if (!string.IsNullOrEmpty(fkNameToSelect) && !string.Equals(rel.ConstraintName, fkNameToSelect))
                            continue;

                        var toTableName = rel.ToTableName;
                        var fromPropertyName = rel.FromPropertyName;

                        if (!string.IsNullOrEmpty(fromPropertyName) && processedFields.Contains(fromPropertyName))
                            continue;

                        if (!string.IsNullOrEmpty(fromPropertyName))
                            processedFields.Add(fromPropertyName);

                        // Extract table name without schema
                        string tableAlias = toTableName;
                        if (toTableName.Contains("."))
                        {
                            tableAlias = toTableName.Split('.').Last();
                        }
                        
                        string propertyName = fromPropertyName;

                        // Use EzDbSchema.Core.Extentions explicitly
                        string singularTableName = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(tableAlias);
                        writer.WriteSafeString($"\n{prefix}public virtual {singularTableName} {propertyName} {{ get; set; }}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for generating POCO model one-to-many relationships
            handlebars.RegisterHelper("POCOModelFKOneToMany", (writer, context, parameters) => {
                var procName = "Handlebars.RegisterHelper('POCOModelFKOneToMany')";
                try
                {
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    
                    IEntity entity = context.Value switch
                    {
                        IRelationship rel => rel.ParentEntity,
                        IEntity ent => ent,
                        _ => throw new ArgumentException($"{procName}: Unable to get entity from context")
                    };

                    var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() ?? "" : "";

                    // Process parameters
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i]?.ToString() ?? "";
                        if (i == 0)
                        {
                            prefix = param;
                        }
                        else if (entity.RelationshipGroups != null && entity.RelationshipGroups.ContainsKey(param))
                        {
                            fkNameToSelect = param;
                        }
                    }

                    // Track fields that have been processed
                    var processedFields = new HashSet<string>();

                    // Get relationships
                    var relationships = entity.Relationships;
                    if (relationships == null || !relationships.Any())
                        return;
                        
                    var oneToManyRelationships = relationships.Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToMany).ToList();
                    
                    // Process each relationship individually instead of grouping
                    foreach (var rel in oneToManyRelationships)
                    {
                        if (rel is null) 
                            continue;

                        // Skip if we're looking for a specific FK name and this isn't it
                        if (!string.IsNullOrEmpty(fkNameToSelect) && !string.Equals(rel.ConstraintName, fkNameToSelect))
                            continue;

                        var fromTableName = rel.FromTableName;
                        var toPropertyName = rel.ToPropertyName;

                        if (!string.IsNullOrEmpty(toPropertyName) && processedFields.Contains(toPropertyName))
                            continue;

                        if (!string.IsNullOrEmpty(toPropertyName))
                            processedFields.Add(toPropertyName);

                        // Extract table name without schema
                        string tableAlias = fromTableName;
                        if (fromTableName.Contains("."))
                        {
                            tableAlias = fromTableName.Split('.').Last();
                        }
                        
                        string propertyName = toPropertyName;

                        // Use EzDbSchema.Core.Extentions explicitly for both ToSingular and ToPlural
                        string singularName = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(tableAlias);
                        string pluralName = EzDbSchema.Core.Extentions.StringExtensions.ToPlural(propertyName);
                        writer.WriteSafeString($"\n{prefix}public virtual ICollection<{singularName}> {pluralName} {{ get; set; }} = new List<{singularName}>();");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });

            // Helper for checking if an entity has foreign keys
            handlebars.RegisterHelper("HasForeignKeys", (writer, options, context, parameters) => {
                IEntity entity = context.Value switch
                {
                    IEntity ent => ent,
                    _ => null
                };

                if (entity is null)
                    return;

                var relationships = entity.Relationships;
                bool hasForeignKeys = relationships != null && relationships.Any();

                if (hasForeignKeys)
                {
                    options.Template(writer, context);
                }
                else
                {
                    // Use options.Inverse directly without any null check
                    // The Handlebars library ensures it's never null in this context
                    options.Inverse(writer, context);
                }
            });

            // Helper for getting all foreign key properties
            handlebars.RegisterHelper("GetForeignKeyProperties", (writer, options, context, parameters) => {
                IEntity entity = context.Value switch
                {
                    IRelationship rel => rel.ParentEntity,
                    IEntity ent => ent,
                    _ => null
                };

                if (entity is null)
                    return;

                var procName = $"Handlebars.RegisterHelper('GetForeignKeyProperties', Entity='{entity.TableName}')";

                try
                {
                    var relationships = entity.Relationships;
                    if (relationships == null || !relationships.Any())
                        return;

                    foreach (var relationship in relationships)
                    {
                        options.Template(writer, relationship);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{procName}: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
