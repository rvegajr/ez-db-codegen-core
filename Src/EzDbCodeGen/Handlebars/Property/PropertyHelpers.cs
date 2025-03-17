namespace EzDbCodeGen.Handlebars.Property;

/// <summary>
/// Provides Handlebars helper functions for property-related code generation
/// </summary>
public static class PropertyHelpers
{
    /// <summary>
    /// Registers property-related helpers with the provided Handlebars instance
    /// </summary>
    public static void Register(IHandlebars handlebars)
    {
        RegisterPropertyAttributesHelper(handlebars);
    }

    private static void RegisterPropertyAttributesHelper(IHandlebars handlebars)
    {
        handlebars.RegisterHelper("POCOModelPropertyAttributes", (writer, context, parameters) =>
        {
            var prefix = parameters.Length > 0 ? parameters[0].ToString() : "";
            if (context.Value is not IProperty property)
            {
                throw new HandlebarsException("POCOModelPropertyAttributes helper requires an IProperty context");
            }

            var attributes = new List<string>();

            // Handle primary key and identity
            if (property.IsPrimaryKey)
            {
                var keyAttribute = "[Key";
                if (property.ParentEntity?.PrimaryKeys?.Count > 1)
                {
                    keyAttribute += $", Column(\"{property.PropertyName}\", Order={property.PrimaryKeyOrder})";
                }
                keyAttribute += "]";
                attributes.Add(keyAttribute);

                if (property.IsIdentity)
                {
                    attributes.Add("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }
            }

            // Handle foreign key
            if (property.ParentEntity?.RelationshipGroups != null)
            {
                foreach (var relationshipGroup in property.ParentEntity.RelationshipGroups.Values)
                {
                    foreach (var relationship in relationshipGroup)
                    {
                        if (relationship.MultiplicityType == EzDbSchema.Core.Enums.RelationshipMultiplicityType.ManyToOne &&
                            relationship.FromPropertyName.Contains(property.PropertyName))
                        {
                            var navigationProperty = relationship.ToTableName.TrimEnd('s');
                            attributes.Add($"[ForeignKey(\"{navigationProperty}\")]");
                        }
                    }
                }
            }

            // Write attributes
            foreach (var attribute in attributes)
            {
                writer.WriteSafeString($"\n{prefix}{attribute}");
            }
        });
    }
}
