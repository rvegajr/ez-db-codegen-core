namespace EzDbCodeGen.UnitTests.Handlebars;

public class HandlebarsUtilityTests
{
    [Fact]
    public void RegisterHelpers_ShouldRegisterBasicHelpers()
    {
        // Arrange
        var handlebars = HandlebarsDotNet.Handlebars.Create();

        // Act
        HandlebarsUtility.RegisterHelpers(handlebars);

        // Assert
        // Verify that basic helpers are registered by using them
        var template = handlebars.Compile("{{#if value}}true{{else}}false{{/if}}");
        var result = template(new { value = true });
        Assert.Equal("true", result);
    }

    [Fact]
    public void POCOModelPropertyAttributes_ShouldGeneratePropertyAttributes()
    {
        // Arrange
        var handlebars = HandlebarsDotNet.Handlebars.Create();
        HandlebarsUtility.RegisterHelpers(handlebars);

        var property = new Property
        {
            PropertyName = "TestProperty",
            ColumnName = "TestProperty",
            DataType = "int",
            IsIdentity = true,
            IsPrimaryKey = true,
            PrimaryKeyOrder = 1,
            ParentEntity = new Entity { TableName = "TestTable" }
        };

        var template = handlebars.Compile("{{POCOModelPropertyAttributes '    '}}");

        // Act
        var result = template(property);

        // Assert
        Assert.Contains("[Key", result);
        Assert.Contains("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]", result);
    }

    [Fact]
    public void POCOModelPropertyAttributes_ShouldGenerateForeignKeyAttributes()
    {
        // Arrange
        var handlebars = HandlebarsDotNet.Handlebars.Create();
        HandlebarsUtility.RegisterHelpers(handlebars);

        var entity = new Entity
        {
            TableName = "Orders",
            TableAlias = "Order"
        };

        var relationship = new Relationship
        {
            FromTableName = "Orders",
            ToTableName = "Customers",
            FromPropertyName = "CustomerId",
            ToPropertyName = "Id",
            ParentEntity = entity,
            MultiplicityType = EzDbSchema.Core.Enums.RelationshipMultiplicityType.ManyToOne
        };

        entity.RelationshipGroups = new RelationshipGroups
        {
            { "FK_Orders_Customers", new RelationshipList { relationship } }
        };

        var property = new Property
        {
            PropertyName = "CustomerId",
            ColumnName = "CustomerId",
            DataType = "int",
            ParentEntity = entity
        };

        var template = handlebars.Compile("{{POCOModelPropertyAttributes '    '}}");

        // Act
        var result = template(property);

        // Assert
        Assert.Contains("[ForeignKey(\"Customer\")]", result);
    }

    [Fact]
    public void POCOModelFKProperties_ShouldGenerateForeignKeyProperties()
    {
        // Arrange
        var handlebars = HandlebarsDotNet.Handlebars.Create();
        HandlebarsUtility.RegisterHelpers(handlebars);

        var entity = new Entity
        {
            TableName = "Orders",
            TableAlias = "Order"
        };

        var relationship = new Relationship
        {
            FromTableName = "Orders",
            ToTableName = "Customers",
            FromPropertyName = "CustomerId",
            ToPropertyName = "Id",
            ParentEntity = entity,
            MultiplicityType = EzDbSchema.Core.Enums.RelationshipMultiplicityType.ManyToOne
        };

        entity.RelationshipGroups = new RelationshipGroups
        {
            { "FK_Orders_Customers", new RelationshipList { relationship } }
        };

        var template = handlebars.Compile("{{POCOModelFKProperties '    '}}");

        // Act
        var result = template(entity);

        // Assert
        Assert.Contains("public virtual Customer Customer { get; set; }", result);
    }

    [Fact]
    public void POCOModelFKProperties_ShouldGenerateCollectionProperties()
    {
        // Arrange
        var handlebars = HandlebarsDotNet.Handlebars.Create();
        HandlebarsUtility.RegisterHelpers(handlebars);

        var entity = new Entity
        {
            TableName = "Customers",
            TableAlias = "Customer"
        };

        var relationship = new Relationship
        {
            FromTableName = "Orders",
            ToTableName = "Customers",
            FromPropertyName = "CustomerId",
            ToPropertyName = "Id",
            ParentEntity = entity,
            MultiplicityType = EzDbSchema.Core.Enums.RelationshipMultiplicityType.OneToMany
        };

        entity.RelationshipGroups = new RelationshipGroups
        {
            { "FK_Orders_Customers", new RelationshipList { relationship } }
        };

        var template = handlebars.Compile("{{POCOModelFKProperties '    '}}");

        // Act
        var result = template(entity);

        // Assert
        Assert.Contains("public virtual ICollection<Order> Orders { get; set; }", result);
    }
}
