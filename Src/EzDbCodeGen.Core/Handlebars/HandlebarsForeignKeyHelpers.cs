using HandlebarsDotNet;
using System.Diagnostics;
using EzDbSchema.Core.Extentions;

namespace EzDbCodeGen.Core.Handlebars
{
    internal static class HandlebarsForeignKeyHelpers
    {
        internal static void RegisterHelpers(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) => {
                var entity = (IEntity)context.Value;
                var PROC_NAME = $"Handlebars.RegisterHelper('POCOModelFKProperties', Entity='{entity.TableName}')";

                try
                {
                    var prefix = parameters[0]?.ToString() ?? "";
                    var fkNametoSelect = "";
                    if (parameters.Length > 1)
                    {
                        fkNametoSelect = parameters[1]?.ToString() ?? "";
                    }

                    var relationships = entity.Relationships;
                    var RelationshipsOneToOne = relationships.Fetch(EzDbSchema.Core.Enums.RelationshipMultiplicityType.ZeroOrOneToOne);
                    var groupedByFKName = EzDbCodeGen.Core.Extentions.RelationshipExtentions.GroupByFKName(RelationshipsOneToOne);

                    foreach (var relationshipGroupKV in groupedByFKName)
                    {
                        var relationship = EzDbCodeGen.Core.Extentions.RelationshipExtentions.AsSummary(relationshipGroupKV.Value);
                        if (relationship == null) continue;

                        if (string.IsNullOrEmpty(fkNametoSelect) || relationship.ForeignKeyName == fkNametoSelect)
                        {
                            string tableAlias = relationship.ToTableName?.ToString()?.Replace($"{entity.DatabaseSchema}.", "") ?? string.Empty;
                            string endAsObjectPropertyName = relationship.ToColumnName?.ToString()?.Replace($"{entity.DatabaseSchema}.", "") ?? string.Empty;

                            writer.WriteSafeString($"\n{prefix}public virtual {EzDbSchema.Core.Extentions.StringExtensions.ToSingular(tableAlias)} {endAsObjectPropertyName} {{ get; set; }}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            handlebars.RegisterHelper("POCOModelFKManyToZeroToOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOne')";
                try
                {
                    var prefix = parameters[0]?.ToString() ?? "";
                    var entity = context.Value switch
                    {
                        CoreInterfaces.IRelationship rel => rel.ParentEntity,
                        CoreInterfaces.IEntity ent => ent,
                        _ => throw new ArgumentException($"{PROC_NAME}: Unable to get entity from context")
                    };

                    var entityName = entity.TableName;
                    var fkNametoSelect = "";

                    if (entity is CoreInterfaces.IEntity coreEntity)
                    {
                        var relationshipGroups = coreEntity.RelationshipGroups;

                        // Process parameters
                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            var param = parameters[i]?.ToString() ?? "";
                            if (string.IsNullOrWhiteSpace(param))
                            {
                                prefix = param;
                            }
                            else if (relationshipGroups.ContainsKey(param))
                            {
                                fkNametoSelect = param;
                            }
                        }

                        // Track fields that have been processed
                        var PreviousOneToOneFields = new List<string>();

                        // Get relationships
                        var relationships = coreEntity.Relationships;
                        var RelationshipsOneToOne = relationships.Fetch(EzDbSchema.Core.Enums.RelationshipMultiplicityType.ZeroOrOneToOne);
                        var groupedByFKName = EzDbCodeGen.Core.Extentions.RelationshipExtentions.GroupByFKName(RelationshipsOneToOne);
                        foreach (var relationshipGroupKV in groupedByFKName)
                        {
                            var rel = EzDbCodeGen.Core.Extentions.RelationshipExtentions.AsSummary(relationshipGroupKV.Value);
                            if (rel == null) continue;

                            var toTableName = rel.ToTableName;
                            var fromPropertyName = string.Join("", rel.FromPropertyName?.ToArray() ?? Array.Empty<string>());

                            if (!string.IsNullOrEmpty(fromPropertyName) && PreviousOneToOneFields.Contains(fromPropertyName))
                            {
                                continue;
                            }

                            PreviousOneToOneFields.Add(fromPropertyName);

                            if (!string.IsNullOrEmpty(fkNametoSelect) && relationshipGroupKV.Key != fkNametoSelect)
                            {
                                continue;
                            }

                            writer.Write($"public virtual {toTableName} {fromPropertyName} {{ get; set; }}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            handlebars.RegisterHelper("POCOModelFKZeroOrOneToOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKZeroOrOneToOne')";
                try
                {
                    var prefix = parameters[0]?.ToString() ?? "";
                    var contextObject = context.Value;
                    var entity = contextObject switch
                    {
                        CoreInterfaces.IRelationship rel => rel.ParentEntity,
                        CoreInterfaces.IEntity ent => ent,
                        _ => throw new ArgumentException($"{PROC_NAME}: Unable to get entity from context")
                    };

                    var entityName = entity.TableName;
                    var fkNametoSelect = "";

                    if (entity is CoreInterfaces.IEntity coreEntity)
                    {
                        var relationshipGroups = coreEntity.RelationshipGroups;

                        // Process parameters
                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            var param = parameters[i]?.ToString() ?? "";
                            if (string.IsNullOrWhiteSpace(param))
                            {
                                prefix = param;
                            }
                            else if (relationshipGroups.ContainsKey(param))
                            {
                                fkNametoSelect = param;
                            }
                        }

                        // Track fields that have been processed
                        var PreviousOneToOneFields = new List<string>();

                        // Get relationships
                        var relationships = coreEntity.Relationships;
                        var RelationshipsOneToOne = relationships.Fetch(EzDbSchema.Core.Enums.RelationshipMultiplicityType.ZeroOrOneToOne);
                        var groupedByFKName = EzDbCodeGen.Core.Extentions.RelationshipExtentions.GroupByFKName(RelationshipsOneToOne);
                        foreach (var relationshipGroupKV in groupedByFKName)
                        {
                            var rel = EzDbCodeGen.Core.Extentions.RelationshipExtentions.AsSummary(relationshipGroupKV.Value);
                            if (rel == null) continue;

                            var toTableName = rel.ToTableName;
                            var fromPropertyName = string.Join("", rel.FromPropertyName?.ToArray() ?? Array.Empty<string>());

                            if (!string.IsNullOrEmpty(fromPropertyName) && PreviousOneToOneFields.Contains(fromPropertyName))
                            {
                                continue;
                            }

                            PreviousOneToOneFields.Add(fromPropertyName);

                            if (!string.IsNullOrEmpty(fkNametoSelect) && relationshipGroupKV.Key != fkNametoSelect)
                            {
                                continue;
                            }

                            writer.Write($"public virtual {toTableName} {fromPropertyName} {{ get; set; }}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            handlebars.RegisterHelper("POCOModelFKManyToZeroOrOne", (writer, context, parameters) => {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroOrOne')";
                try
                {
                    var prefix = parameters[0]?.ToString() ?? "";
                    var entity = context.Value switch
                    {
                        CoreInterfaces.IRelationship rel => rel.ParentEntity,
                        CoreInterfaces.IEntity ent => ent,
                        _ => throw new ArgumentException($"{PROC_NAME}: Unable to get entity from context")
                    };

                    var entityName = entity.TableName;
                    var fkNametoSelect = "";

                    if (entity is CoreInterfaces.IEntity coreEntity)
                    {
                        var relationshipGroups = coreEntity.RelationshipGroups;

                        // Process parameters
                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            var param = parameters[i]?.ToString() ?? "";
                            if (string.IsNullOrWhiteSpace(param))
                            {
                                prefix = param;
                            }
                            else if (relationshipGroups.ContainsKey(param))
                            {
                                fkNametoSelect = param;
                            }
                        }

                        // Track fields that have been processed
                        var PreviousOneToOneFields = new List<string>();

                        // Get relationships
                        var relationships = coreEntity.Relationships;
                        var relationshipsManyToZeroOrOne = relationships.Fetch(EzDbSchema.Core.Enums.RelationshipMultiplicityType.ManyToZeroOrOne);
                        var groupedByFKName = EzDbCodeGen.Core.Extentions.RelationshipExtentions.GroupByFKName(relationshipsManyToZeroOrOne);

                        // Get the list of tables to exclude
                        var excludeTables = new List<string>();
                        if (parameters.Length > 1)
                        {
                            var excludeParams = parameters[1]?.ToString()?.Split(',') ?? Array.Empty<string>();
                            excludeTables.AddRange(excludeParams.Select(p => p.Trim()));
                        }

                        foreach (var relationshipGroupKV in groupedByFKName)
                        {
                            var rel = EzDbCodeGen.Core.Extentions.RelationshipExtentions.AsSummary(relationshipGroupKV.Value);
                            if (rel == null) continue;

                            var toTableName = rel.ToTableName;
                            var fromPropertyName = string.Join("", rel.FromPropertyName?.ToArray() ?? Array.Empty<string>());

                            if (!string.IsNullOrEmpty(fromPropertyName) && PreviousOneToOneFields.Contains(fromPropertyName))
                            {
                                continue;
                            }

                            PreviousOneToOneFields.Add(fromPropertyName);

                            if (!string.IsNullOrEmpty(fkNametoSelect) && relationshipGroupKV.Key != fkNametoSelect)
                            {
                                continue;
                            }

                            if (excludeTables.Contains(toTableName))
                            {
                                continue;
                            }

                            writer.Write($"public virtual {toTableName} {fromPropertyName} {{ get; set; }}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });
        }
    }
}
