using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Objects;
using Moq;
using Xunit;
using EzDbCodeGen.Core.Handlebars;

namespace EzDbCodeGen.HandlebarsTests
{
    public class NewHandlebarsSchemaHelpersTests
    {
        private readonly IHandlebars _handlebars;
        private readonly NewHandlebarsSchemaHelpers _schemaHelpers;

        public NewHandlebarsSchemaHelpersTests()
        {
            _handlebars = Handlebars.Create();
            _schemaHelpers = new NewHandlebarsSchemaHelpers(_handlebars);
            _schemaHelpers.RegisterHelpers();
        }

        [Fact]
        public void FilterBySchema_ReturnsCorrectEntities()
        {
            // Arrange
            var mockDatabase = CreateMockDatabase();
            var template = @"{{#filterBySchema 'dbo'}}{{#each this}}{{TableName}}{{#unless @last}},{{/unless}}{{/each}}{{/filterBySchema}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockDatabase);

            // Assert
            Assert.Contains("Customer,Order", result);
            Assert.DoesNotContain("Product", result);
        }

        [Fact]
        public void GroupBySchema_GroupsEntitiesCorrectly()
        {
            // Arrange
            var mockDatabase = CreateMockDatabase();
            var template = @"{{#groupBySchema}}{{schema}}:{{#each entities}}{{TableName}}{{#unless @last}},{{/unless}}{{/each}}|{{/groupBySchema}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockDatabase);

            // Assert
            Assert.Contains("dbo:Customer,Order|", result);
            Assert.Contains("sales:Product|", result);
        }

        [Fact]
        public void GetSchemas_ListsAllSchemas()
        {
            // Arrange
            var mockDatabase = CreateMockDatabase();
            var template = @"{{#getSchemas}}{{this}}{{#unless @last}},{{/unless}}{{/getSchemas}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockDatabase);

            // Assert
            Assert.Equal("dbo,sales", result);
        }

        [Fact]
        public void SchemaInfo_ProvidesCorrectMetadata()
        {
            // Arrange
            var mockDatabase = CreateMockDatabase();
            var template = @"{{#schemaInfo 'dbo'}}Tables:{{TableCount}},HasViews:{{HasViews}}{{/schemaInfo}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockDatabase);

            // Assert
            Assert.Contains("Tables:2", result);
            Assert.Contains("HasViews:false", result);
        }

        [Fact]
        public void HasForeignKeys_DetectsCorrectly()
        {
            // Arrange
            var entity = CreateMockEntityWithRelationships("Customer", "dbo", false);
            var template = @"{{hasForeignKeys}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(entity);

            // Assert
            Assert.Equal("true", result);
        }

        [Fact]
        public void GetForeignKeyProperties_ListsCorrectly()
        {
            // Arrange
            var entity = CreateMockEntityWithRelationships("Order", "dbo", false);
            var template = @"{{getForeignKeyProperties}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(entity);

            // Assert
            Assert.Contains("CustomerId:Customer;", result);
        }

        [Fact]
        public void GetUniqueConstraints_ListsCorrectly()
        {
            // Arrange
            var entity = CreateMockEntityWithUniqueProperties("Customer", "dbo", false);
            var template = @"{{getUniqueConstraints}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(entity);

            // Assert
            Assert.Contains("Email;", result);
        }

        private IDatabase CreateMockDatabase()
        {
            var mockDatabase = new Mock<IDatabase>();
            var entities = new List<IEntity>
            {
                CreateMockEntity("Customer", "dbo", false),
                CreateMockEntity("Order", "dbo", false),
                CreateMockEntity("Product", "sales", false)
            };

            mockDatabase.Setup(d => d.Values).Returns(entities);
            return mockDatabase.Object;
        }

        private IEntity CreateMockEntity(string name, string schema, bool isView)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns(name);
            mockEntity.Setup(e => e.DatabaseSchema).Returns(schema);
            mockEntity.Setup(e => e.IsView).Returns(isView);
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithRelationships(string name, string schema, bool isView)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns(name);
            mockEntity.Setup(e => e.DatabaseSchema).Returns(schema);
            mockEntity.Setup(e => e.IsView).Returns(isView);

            var relationships = new List<IRelationship>();
            if (name == "Order")
            {
                var relationship = new Mock<IRelationship>();
                relationship.Setup(r => r.FromPropertyName).Returns("CustomerId");
                relationship.Setup(r => r.ToTableName).Returns("Customer");
                relationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ManyToOne);
                relationships.Add(relationship.Object);
            }

            mockEntity.Setup(e => e.Relationships).Returns(relationships);
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithUniqueProperties(string name, string schema, bool isView)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns(name);
            mockEntity.Setup(e => e.DatabaseSchema).Returns(schema);
            mockEntity.Setup(e => e.IsView).Returns(isView);

            var properties = new List<IProperty>();
            var emailProperty = new Mock<IProperty>();
            emailProperty.Setup(p => p.Name).Returns("Email");
            emailProperty.Setup(p => p.IsUnique).Returns(true);
            properties.Add(emailProperty.Object);

            mockEntity.Setup(e => e.Properties).Returns(properties);
            return mockEntity.Object;
        }
    }
}
