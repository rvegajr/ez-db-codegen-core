using HandlebarsDotNet;
using HandlebarsDotNet.PathStructure;
using HandlebarsDotNet.Runtime;
using HandlebarsDotNet.Helpers;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using Moq;
using Xunit;

namespace EzDbCodeGen.HandlebarsTests
{
    public class HandlebarsSchemaHelpersTests
    {
        private readonly IHandlebars _handlebars;
        private readonly HandlebarsSchemaHelpers _schemaHelpers;

        public HandlebarsSchemaHelpersTests()
        {
            _handlebars = Handlebars.Create();
            _schemaHelpers = new HandlebarsSchemaHelpers(_handlebars);
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
            mockEntity.Setup(e => e.EntityType).Returns(isView ? "View" : "Table");
            return mockEntity.Object;
        }
    }
}
