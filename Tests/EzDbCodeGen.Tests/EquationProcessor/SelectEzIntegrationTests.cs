using System;
using System.Collections.Generic;
using System.Linq;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using HandlebarsDotNet;
using Moq;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class SelectEzIntegrationTests : IDisposable
    {
        // Using a method to create a fresh Handlebars instance for each test
        // to prevent test interference
        private IHandlebars CreateHandlebarsInstance()
        {
            var handlebars = HandlebarsDotNet.Handlebars.Create();
            handlebars.RegisterSelectEzHelper();
            return handlebars;
        }
        
        public void Dispose()
        {
            // Clean up any resources if needed
        }

        [Fact]
        public void SelectEz_SimplePropertyMatch_RendersCorrectly()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.[EntityType='Customer']\"}}{{EntityType}}{{/EachSelected}}";
            var entities = CreateMockEntities(new[] { "Customer", "Order", "Product" });
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entities);

            // Assert
            Assert.Equal("Customer", result);
        }

        [Fact]
        public void SelectEz_WildcardMatch_RendersCorrectly()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.[EntityType='Customer*']\"}}{{EntityType}},{{/EachSelected}}";
            var entities = CreateMockEntities(new[] { "Customer", "CustomerOrder", "Product" });
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entities);

            // Assert
            Assert.Contains("Customer,", result);
            Assert.Contains("CustomerOrder,", result);
            Assert.DoesNotContain("Product", result);
        }

        [Fact]
        public void SelectEz_RelationshipByName_RendersCorrectly()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.Relationships[ConstraintName='CustomerOrder']\"}}{{ConstraintName}}{{/EachSelected}}";
            var entity = CreateMockEntityWithRelationships(
                "Customer", 
                new[] { "CustomerAddress", "CustomerOrder", "CustomerPayment" });
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("CustomerOrder", result);
        }

        [Fact]
        public void SelectEz_RelationshipByWildcard_RendersCorrectly()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.Relationships[ConstraintName='Customer*']\"}}{{ConstraintName}},{{/EachSelected}}";
            var entity = CreateMockEntityWithRelationships(
                "Customer", 
                new[] { "CustomerAddress", "CustomerOrder", "CustomerPayment" });
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Contains("CustomerAddress,", result);
            Assert.Contains("CustomerOrder,", result);
            Assert.Contains("CustomerPayment,", result);
        }

        [Fact]
        public void SelectEz_EmptyResult_RendersElseBlock()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.Relationships[ConstraintName='NotFound']\"}}Found{{else}}Not Found{{/EachSelected}}";
            var entity = CreateMockEntityWithRelationships(
                "Customer", 
                new[] { "CustomerAddress", "CustomerOrder", "CustomerPayment" });
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entity);

            // Assert
            Assert.Equal("Not Found", result);
        }

        [Fact]
        public void SelectEz_MultipleConditions_RendersCorrectly()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.[EntityType='*' && Properties['Id'].IsPrimaryKey=true]\"}}{{EntityType}},{{/EachSelected}}";
            var entities = new List<IEntity>
            {
                CreateMockEntityWithProperties("Customer", new Dictionary<string, bool> { { "Id", true } }),
                CreateMockEntityWithProperties("Order", new Dictionary<string, bool> { { "Id", true } }),
                CreateMockEntityWithProperties("Product", new Dictionary<string, bool> { { "Id", false } })
            };
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entities);

            // Assert
            Assert.Contains("Customer,", result);
            Assert.Contains("Order,", result);
            Assert.DoesNotContain("Product", result);
        }

        [Fact]
        public void SelectEz_HelperReturnsArray_CanBeUsedInHandlebarsEach()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#each (SelectEz \"$.[EntityType='Customer*']\")}}{{EntityType}},{{/each}}";
            var entities = CreateMockEntities(new[] { "Customer", "CustomerOrder", "Product" });
            var compiledTemplate = handlebars.Compile(template);

            // Act
            string result = compiledTemplate(entities);

            // Assert
            Assert.Contains("Customer,", result);
            Assert.Contains("CustomerOrder,", result);
            Assert.DoesNotContain("Product", result);
        }

        [Fact]
        public void SelectEz_InvalidExpression_ThrowsHandlebarsException()
        {
            // Arrange
            var handlebars = CreateHandlebarsInstance();
            string template = "{{#EachSelected \"$.InvalidProperty\"}}{{EntityType}}{{/EachSelected}}";
            var entity = CreateMockEntity("Customer");
            var compiledTemplate = handlebars.Compile(template);

            // Act & Assert
            var exception = Assert.Throws<HandlebarsException>(() => compiledTemplate(entity));
            Assert.Contains("Error in EachSelected helper", exception.Message);
        }

        private IEnumerable<IEntity> CreateMockEntities(string[] entityTypes)
        {
            var entities = new List<IEntity>();
            
            foreach (var entityType in entityTypes)
            {
                var mockEntity = new Mock<IEntity>();
                mockEntity.Setup(e => e.EntityType).Returns(entityType);
                mockEntity.Setup(e => e.TableName).Returns(entityType);
                
                var relationshipList = new Mock<IRelationshipReferenceList>();
                relationshipList.Setup(r => r.Count).Returns(0);
                mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
                
                mockEntity.Setup(e => e.Properties).Returns(new PropertyDictionary());
                
                entities.Add(mockEntity.Object);
            }
            
            return entities;
        }

        private IEntity CreateMockEntity(string entityType)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns(entityType);
            mockEntity.Setup(e => e.TableName).Returns(entityType);
            
            var relationshipList = new Mock<IRelationshipReferenceList>();
            relationshipList.Setup(r => r.Count).Returns(0);
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            mockEntity.Setup(e => e.Properties).Returns(new PropertyDictionary());
            
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithRelationships(string entityType, string[] relationshipNames)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns(entityType);
            mockEntity.Setup(e => e.TableName).Returns(entityType);
            
            var relationships = new List<IRelationship>();
            foreach (var name in relationshipNames)
            {
                var mockRelationship = new Mock<IRelationship>();
                mockRelationship.Setup(r => r.ConstraintName).Returns(name);
                relationships.Add(mockRelationship.Object);
            }
            
            var relationshipList = new Mock<IRelationshipReferenceList>();
            relationshipList.Setup(r => r.Count).Returns(relationships.Count);
            relationshipList.Setup(r => r.GetEnumerator()).Returns(relationships.GetEnumerator());
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            mockEntity.Setup(e => e.Properties).Returns(new PropertyDictionary());
            
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithProperties(string entityType, Dictionary<string, bool> propertyNameToPrimaryKey)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns(entityType);
            mockEntity.Setup(e => e.TableName).Returns(entityType);
            
            var properties = new PropertyDictionary();
            foreach (var kvp in propertyNameToPrimaryKey)
            {
                var mockProperty = new Mock<IProperty>();
                mockProperty.Setup(p => p.PropertyName).Returns(kvp.Key);
                mockProperty.Setup(p => p.IsPrimaryKey).Returns(kvp.Value);
                properties.Add(kvp.Key, mockProperty.Object);
            }
            
            mockEntity.Setup(e => e.Properties).Returns(properties);
            
            var relationshipList = new Mock<IRelationshipReferenceList>();
            relationshipList.Setup(r => r.Count).Returns(0);
            mockEntity.Setup(e => e.Relationships).Returns(relationshipList.Object);
            
            return mockEntity.Object;
        }
    }
}
