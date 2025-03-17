namespace EzDbCodeGen.Handlebars.Entity;

/// <summary>
/// Provides Handlebars helper functions for entity-related code generation
/// </summary>
public static class EntityHelpers
{
    /// <summary>
    /// Registers entity-related helpers with the provided Handlebars instance
    /// </summary>
    public static void Register(IHandlebars handlebars)
    {
        RegisterForeignKeyPropertiesHelper(handlebars);
    }

    private static void RegisterForeignKeyPropertiesHelper(IHandlebars handlebars)
    {
        handlebars.RegisterHelper("POCOModelFKProperties", (writer, context, parameters) =>
        {
            var prefix = parameters.Length > 0 ? parameters[0].ToString() : "";
            if (context.Value is not IEntity entity)
            {
                throw new HandlebarsException("POCOModelFKProperties helper requires an IEntity context");
            }

            foreach (var relationshipGroup in entity.RelationshipGroups.Values)
            {
                foreach (var relationship in relationshipGroup)
                {
                    if (relationship.MultiplicityType == EzDbSchema.Core.Enums.RelationshipMultiplicityType.ManyToOne)
                    {
                        var toTableName = relationship.ToTableName;
                        var propertyName = toTableName.TrimEnd('s'); // Simple singularization
                        writer.WriteSafeString($"\n{prefix}public virtual {propertyName} {propertyName} {{ get; set; }}");
                    }
                    else if (relationship.MultiplicityType == EzDbSchema.Core.Enums.RelationshipMultiplicityType.OneToMany)
                    {
                        var fromTableName = relationship.FromTableName;
                        var propertyName = fromTableName; // Collection properties are plural
                        var elementName = fromTableName.TrimEnd('s'); // Element type is singular
                        writer.WriteSafeString($"\n{prefix}public virtual ICollection<{elementName}> {propertyName} {{ get; set; }}");
                    }
                }
            }
        });
    }
}
