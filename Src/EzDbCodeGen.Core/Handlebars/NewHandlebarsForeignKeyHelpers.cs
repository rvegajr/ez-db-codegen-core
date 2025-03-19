using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Extentions;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Handlebars
{
    public class NewHandlebarsForeignKeyHelpers
    {
        private readonly IHandlebars _handlebars;

        public NewHandlebarsForeignKeyHelpers(IHandlebars handlebars)
        {
            _handlebars = handlebars ?? throw new ArgumentNullException(nameof(handlebars));
        }

        public void RegisterHelpers()
        {
            _handlebars.RegisterHelper("GetForeignKeyProperties", (writer, context, parameters) => {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IEntity entity)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? string.Empty : string.Empty;
                var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() : string.Empty;

                if (entity.Relationships == null || !entity.Relationships.Any())
                {
                    return;
                }

                var relationships = entity.Relationships.Where(r => 
                    r.MultiplicityType == RelationshipMultiplicityType.ManyToOne || 
                    r.MultiplicityType == RelationshipMultiplicityType.ManyToZeroOrOne).ToList();

                foreach (var relationship in relationships)
                {
                    if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ConstraintName == fkNameToSelect)
                    {
                        var tableAlias = relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", "");
                        var propertyName = relationship.FromPropertyName;
                        var singularTableName = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(tableAlias);
                        writer.WriteSafeString($"\n{prefix}public virtual {singularTableName} {propertyName} {{ get; set; }}");
                    }
                }
            });

            _handlebars.RegisterHelper("GetOneToOneReferences", (writer, context, parameters) => {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IEntity entity)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? string.Empty : string.Empty;
                var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() : string.Empty;
                
                if (entity.Relationships == null || !entity.Relationships.Any())
                {
                    return;
                }
                
                var relationships = entity.Relationships.Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToOne).ToList();

                foreach (var relationship in relationships)
                {
                    if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ConstraintName == fkNameToSelect)
                    {
                        var tableAlias = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(relationship.ToTableName.Replace($"{entity.DatabaseSchema}.", ""));
                        var propertyName = relationship.ToPropertyName;
                        writer.WriteSafeString($"\n{prefix}public virtual {tableAlias} {propertyName} {{ get; set; }}");
                    }
                }
            });

            _handlebars.RegisterHelper("GetForeignKeyCollectionsForEntity", (writer, context, parameters) => {
                if (context.Value is null)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                if (context.Value is not IEntity entity)
                {
                    writer.WriteSafeString(string.Empty);
                    return;
                }

                var prefix = parameters.Length > 0 ? parameters[0]?.ToString() ?? string.Empty : string.Empty;
                var fkNameToSelect = parameters.Length > 1 ? parameters[1]?.ToString() : string.Empty;
                
                if (entity.Relationships == null || !entity.Relationships.Any())
                {
                    return;
                }
                
                var relationships = entity.Relationships.Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToMany).ToList();

                foreach (var relationship in relationships)
                {
                    if (string.IsNullOrEmpty(fkNameToSelect) || relationship.ConstraintName == fkNameToSelect)
                    {
                        var tableAlias = relationship.FromTableName.Replace($"{entity.DatabaseSchema}.", "");
                        var singularName = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(tableAlias);
                        var pluralName = EzDbSchema.Core.Extentions.StringExtensions.ToPlural(relationship.ToPropertyName);
                        writer.WriteSafeString($"\n{prefix}public virtual ICollection<{singularName}> {pluralName} {{ get; set; }} = new List<{singularName}>();");
                    }
                }
            });
        }
    }
}
