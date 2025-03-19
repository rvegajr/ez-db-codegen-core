using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using HandlebarsDotNet;
using Moq;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class IfCondEzIntegrationTests : IDisposable
    {
        // Using a method to create a fresh Handlebars instance for each test
        // to prevent test interference
        private IHandlebars CreateHandlebarsInstance()
        {
            var handlebars = HandlebarsDotNet.Handlebars.Create();
            handlebars.RegisterIfCondEzHelper();
            return handlebars;
        }
        
        public void Dispose()
        {
            // Clean up any resources if needed
        }

        [Fact]
        public void IfCondEz_SimpleCondition_RendersCorrectly()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#IfCondEz \"$.EntityType='Table'\"}}Is Table{{else}}Not Table{{/IfCondEz}}";
            var entity = CreateMockEntity("Table");
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("Is Table", result);
        }

        [Fact]
        public void IfCondEz_ComplexCondition_RendersCorrectly()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#IfCondEz \"$.EntityType='View' && $.Relationships.Count=0\"}}Is View with no relationships{{else}}Not a view or has relationships{{/IfCondEz}}";
            var entity = CreateMockEntity("View", 0);
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("Is View with no relationships", result);
        }

        [Fact]
        public void IfCondEz_NestedPropertyAccess_RendersCorrectly()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#IfCondEz \"$.Properties['Id'].IsPrimaryKey=true\"}}Has primary key{{else}}No primary key{{/IfCondEz}}";
            var entity = CreateMockEntityWithNestedProperties("Table", true);
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("Has primary key", result);
        }

        [Fact]
        public void IfCondEz_ComplexLogicalExpression_RendersCorrectly()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#IfCondEz \"($.EntityType='View' || $.EntityType='Table') && ($.Properties.Count > 0)\"}}Valid entity{{else}}Invalid entity{{/IfCondEz}}";
            var entity = CreateMockEntityWithNestedProperties("View", false);
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("Valid entity", result);
        }

        [Fact]
        public void IfCondEz_StringOperations_RendersCorrectly()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#IfCondEz \"$.EntityType contains 'Table'\"}}Contains Table{{else}}Does not contain Table{{/IfCondEz}}";
            var entity = CreateMockEntity("UserTable");
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("Contains Table", result);
        }

        [Fact]
        public void IfCondEz_InvalidExpression_ThrowsHandlebarsException()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#IfCondEz \"$.EntityType=\"}}Invalid{{/IfCondEz}}";
            var entity = CreateMockEntity("Table");
            var compiledTemplate = handlebars.Compile(template);

            // Act & Assert
            var exception = Assert.Throws<HandlebarsException>(() => compiledTemplate(entity));
            Assert.Contains("Invalid expression", exception.Message);
        }

        [Fact]
        public void IfCondEz_RealWorldExample_ViewWithNoRelationships()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = @"
{{#IfCondEz ""$.EntityType='View' && $.Relationships.Count=0""}}
    // This is a view with no relationships
    public partial class {{TableName}}View 
    {
        // View properties only
    }
{{else}}
    // This is a regular entity or a view with relationships
    public partial class {{TableName}}
    {
        // Regular entity implementation
    }
{{/IfCondEz}}";
            
            // Create a mock entity with EntityType="View" and Relationships.Count=0
            var mockEntity = new Mock<IEntity>(MockBehavior.Strict);
            mockEntity.Setup(e => e.EntityType).Returns("View");
            mockEntity.Setup(e => e.TableName).Returns("CustomerSummary");
            
            var relationshipList = new Mock<IRelationshipReferenceList>(MockBehavior.Strict);
            relationshipList.Setup(r => r.Count).Returns(0);
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            var properties = new PropertyDictionary();
            mockEntity.Setup(e => e.Properties).Returns(properties);
            
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(mockEntity.Object);
            Console.WriteLine(result); // Debug output

            // Assert
            Assert.Contains("// This is a view with no relationships", result);
            Assert.Contains("public partial class CustomerSummaryView", result);
        }

        [Fact]
        public void IfCondEz_RealWorldExample_ComplexCondition()
        {
            // Arrange - Create a fresh instance for this test
            var handlebars = CreateHandlebarsInstance();
            string template = @"
{{#IfCondEz ""$.Properties['Id'].IsPrimaryKey=true && ($.Relationships.Count > 0 || $.EntityType='Table')""}}
    // Entity with primary key that is either a table or has relationships
    public partial class {{TableName}}Entity : IEntity
    {
        // Full entity implementation
    }
{{else}}
    // Simple data transfer object
    public partial class {{TableName}}Dto
    {
        // DTO implementation
    }
{{/IfCondEz}}";
            
            var entity = CreateComplexEntity();
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Contains("// Entity with primary key that is either a table or has relationships", result);
            Assert.Contains("public partial class TestEntity : IEntity", result);
        }

        private IEntity CreateMockEntity(string entityType, int relationshipCount = 0)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns(entityType);
            mockEntity.Setup(e => e.TableName).Returns(entityType == "CustomerSummary" ? "CustomerSummary" : "Test");
            
            var relationships = new List<IRelationship>();
            for (int i = 0; i < relationshipCount; i++)
            {
                relationships.Add(new Mock<IRelationship>().Object);
            }
            
            var relationshipList = new Mock<IRelationshipReferenceList>();
            relationshipList.Setup(r => r.Count).Returns(relationshipCount);
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            mockEntity.Setup(e => e.Properties).Returns(new PropertyDictionary());
            
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithNestedProperties(string entityType, bool isPrimaryKey)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns(entityType);
            mockEntity.Setup(e => e.TableName).Returns("Test");
            
            var properties = new PropertyDictionary();
            properties.Add("Id", CreateMockProperty("Id", "int", 0, isPrimaryKey));
            
            mockEntity.Setup(e => e.Properties).Returns(properties);
            
            var relationshipList = new Mock<IRelationshipReferenceList>();
            relationshipList.Setup(r => r.Count).Returns(0);
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            return mockEntity.Object;
        }

        private IProperty CreateMockProperty(string name, string dataType, int maxLength, bool isPrimaryKey = false)
        {
            var mockProperty = new Mock<IProperty>();
            mockProperty.Setup(p => p.PropertyName).Returns(name);
            mockProperty.Setup(p => p.DataType).Returns(dataType);
            mockProperty.Setup(p => p.MaxLength).Returns(maxLength);
            mockProperty.Setup(p => p.IsPrimaryKey).Returns(isPrimaryKey);
            return mockProperty.Object;
        }

        private IEntity CreateComplexEntity()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns("Table");
            mockEntity.Setup(e => e.TableName).Returns("Test");
            
            var properties = new PropertyDictionary();
            properties.Add("Id", CreateMockProperty("Id", "int", 0, true));
            properties.Add("Name", CreateMockProperty("Name", "varchar", 50));
            
            mockEntity.Setup(e => e.Properties).Returns(properties);
            
            var relationshipList = new Mock<IRelationshipReferenceList>();
            relationshipList.Setup(r => r.Count).Returns(0);
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            return mockEntity.Object;
        }
    }
}
