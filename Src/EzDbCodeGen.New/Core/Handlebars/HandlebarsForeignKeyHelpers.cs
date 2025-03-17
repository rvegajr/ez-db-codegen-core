using System.Diagnostics;

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
                    var oneToOneRelationships = relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne);
                    var groupedByFKName = oneToOneRelationships.GroupByFKName();

                    foreach (var relationshipGroupKV in groupedByFKName)
                    {
                        var relationship = relationshipGroupKV.Value.AsSummary();
                        if (relationship == null) continue;

                        if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ForeignKeyName == fkNameToSelect)
                        {
                            string tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                            string endAsObjectPropertyName = relationship.ToColumnName.Replace($"{entity.DatabaseSchema}.", "");

                            writer.WriteSafeString($"\n{prefix}public virtual {tableAlias.ToSingular()} {endAsObjectPropertyName} {{ get; set; }}");
                        }
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

                    var fkNameToSelect = "";

                    // Process parameters
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i]?.ToString() ?? "";
                        if (i == 0)
                        {
                            prefix = param;
                        }
                        else if (entity.RelationshipGroups.ContainsKey(param))
                        {
                            fkNameToSelect = param;
                        }
                    }

                    // Track fields that have been processed
                    var processedFields = new HashSet<string>();

                    // Get relationships
                    var relationships = entity.Relationships;
                    var oneToOneRelationships = relationships.Fetch(RelationshipMultiplicityType.ZeroOrOneToOne);
                    var groupedByFKName = oneToOneRelationships.GroupByFKName();
                    
                    foreach (var relationshipGroupKV in groupedByFKName)
                    {
                        var rel = relationshipGroupKV.Value.AsSummary();
                        if (rel == null) continue;

                        if (!string.IsNullOrEmpty(fkNameToSelect) && rel.ForeignKeyName != fkNameToSelect)
                            continue;

                        var toTableName = rel.ToTableName;
                        var fromPropertyName = rel.FromPropertyName;

                        if (!string.IsNullOrEmpty(fromPropertyName) && processedFields.Contains(fromPropertyName))
                            continue;

                        if (!string.IsNullOrEmpty(fromPropertyName))
                            processedFields.Add(fromPropertyName);

                        string tableAlias = toTableName.ExtractTableName();
                        string propertyName = fromPropertyName.AsFormattedName();

                        writer.WriteSafeString($"\n{prefix}public virtual {tableAlias.ToSingular()} {propertyName} {{ get; set; }}");
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

                    var fkNameToSelect = "";

                    // Process parameters
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i]?.ToString() ?? "";
                        if (i == 0)
                        {
                            prefix = param;
                        }
                        else if (entity.RelationshipGroups.ContainsKey(param))
                        {
                            fkNameToSelect = param;
                        }
                    }

                    // Track fields that have been processed
                    var processedFields = new HashSet<string>();

                    // Get relationships
                    var relationships = entity.Relationships;
                    var oneToManyRelationships = relationships.Fetch(RelationshipMultiplicityType.OneToMany);
                    var groupedByFKName = oneToManyRelationships.GroupByFKName();
                    
                    foreach (var relationshipGroupKV in groupedByFKName)
                    {
                        var rel = relationshipGroupKV.Value.AsSummary();
                        if (rel == null) continue;

                        if (!string.IsNullOrEmpty(fkNameToSelect) && rel.ForeignKeyName != fkNameToSelect)
                            continue;

                        var fromTableName = rel.FromTableName;
                        var toPropertyName = rel.ToPropertyName;

                        if (!string.IsNullOrEmpty(toPropertyName) && processedFields.Contains(toPropertyName))
                            continue;

                        if (!string.IsNullOrEmpty(toPropertyName))
                            processedFields.Add(toPropertyName);

                        string tableAlias = fromTableName.ExtractTableName();
                        string propertyName = toPropertyName.AsFormattedName();

                        writer.WriteSafeString($"\n{prefix}public virtual ICollection<{tableAlias.ToSingular()}> {propertyName.ToPlural()} {{ get; set; }} = new List<{tableAlias.ToSingular()}>();");
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
                    IRelationship rel => rel.ParentEntity,
                    IEntity ent => ent,
                    _ => null
                };

                if (entity == null)
                    return;

                var relationships = entity.Relationships;
                bool hasForeignKeys = relationships.Any();

                if (hasForeignKeys)
                {
                    options.Template(writer, context);
                }
                else if (options.Inverse != null)
                {
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

                if (entity == null)
                    return;

                var relationships = entity.Relationships;
                var fkProperties = new List<IProperty>();

                foreach (var relationship in relationships)
                {
                    if (relationship.FromEntity == entity && relationship.FromProperty != null)
                    {
                        fkProperties.Add(relationship.FromProperty);
                    }
                }

                foreach (var fkProperty in fkProperties)
                {
                    options.Template(writer, fkProperty);
                }
            });
        }
    }
}
