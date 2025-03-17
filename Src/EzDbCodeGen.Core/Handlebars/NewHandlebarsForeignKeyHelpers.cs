using HandlebarsDotNet;
using System.Diagnostics;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using EzDbCodeGen.Core.Extensions;

namespace EzDbCodeGen.Core.Handlebars
{
    public class NewHandlebarsForeignKeyHelpers
    {
        private readonly IHandlebars _handlebars;

        public NewHandlebarsForeignKeyHelpers(IHandlebars handlebars)
        {
            _handlebars = handlebars;
        }

        public void RegisterHelpers()
        {
            // Generate POCO model properties for foreign keys
            _handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKProperties')";
                try
                {
                    var entity = (IEntity)context.Value;
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() : null;

                    var oneToOneRelationships = entity.Relationships?
                        .Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToOne)
                        .ToList() ?? new List<IRelationship>();

                    foreach (var relationship in oneToOneRelationships)
                    {
                        if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ConstraintName == fkNameToSelect)
                        {
                            var tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                            var propertyName = relationship.ToPropertyName;

                            writer.WriteSafeString($"\n{prefix}public virtual {tableAlias.ToSingular()} {propertyName} {{ get; set; }}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Generate POCO model properties for many-to-zero-or-one relationships
            _handlebars.RegisterHelper("POCOModelFKManyToZeroToOne", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToZeroToOne')";
                try
                {
                    var entity = (IEntity)context.Value;
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() : null;

                    var manyToZeroOrOneRelationships = entity.Relationships?
                        .Where(r => r.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne)
                        .ToList() ?? new List<IRelationship>();

                    foreach (var relationship in manyToZeroOrOneRelationships)
                    {
                        if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ConstraintName == fkNameToSelect)
                        {
                            var tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                            var propertyName = relationship.ToPropertyName;

                            writer.WriteSafeString($"\n{prefix}public virtual {tableAlias.ToSingular()} {propertyName} {{ get; set; }}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Generate POCO model properties for many-to-one relationships
            _handlebars.RegisterHelper("POCOModelFKManyToOne", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelFKManyToOne')";
                try
                {
                    var entity = (IEntity)context.Value;
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";
                    var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() : null;

                    var manyToOneRelationships = entity.Relationships?
                        .Where(r => r.MultiplicityType == RelationshipMultiplicityType.ManyToOne)
                        .ToList() ?? new List<IRelationship>();

                    foreach (var relationship in manyToOneRelationships)
                    {
                        if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ConstraintName == fkNameToSelect)
                        {
                            var tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                            var propertyName = relationship.ToPropertyName;

                            writer.WriteSafeString($"\n{prefix}public virtual {tableAlias.ToSingular()} {propertyName} {{ get; set; }}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{PROC_NAME}: {ex.Message}");
                    throw;
                }
            });

            // Generate POCO model properties for collection navigation properties
            _handlebars.RegisterHelper("POCOModelCollectionProperties", (writer, context, parameters) =>
            {
                var PROC_NAME = "Handlebars.RegisterHelper('POCOModelCollectionProperties')";
                try
                {
                    var entity = (IEntity)context.Value;
                    var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? "" : "";

                    var collectionRelationships = entity.Relationships?
                        .Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToMany ||
                                  r.MultiplicityType == RelationshipMultiplicityType.OneToMany)
                        .ToList() ?? new List<IRelationship>();

                    foreach (var relationship in collectionRelationships)
                    {
                        var tableAlias = relationship.FromTableName?.Replace($"{entity.DatabaseSchema}.", "") ?? string.Empty;
                        var propertyName = relationship.FromPropertyName;

                        writer.WriteSafeString($"\n{prefix}public virtual ICollection<{tableAlias}> {propertyName} {{ get; set; }} = new HashSet<{tableAlias}>();");
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
