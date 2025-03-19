using System.Collections.Generic;
using System.Linq;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using Moq;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class SelectEzTests
    {
        private readonly SelectEz _selectEz;

        public SelectEzTests()
        {
            _selectEz = new SelectEz();
        }

        [Fact]
        public void Select_SimplePropertyMatch_ReturnsMatchingItems()
        {
            // Arrange
            var entities = CreateMockEntities(new[] { "Customer", "Order", "Product" });
            
            // Act
            var result = _selectEz.Select("$.[EntityType='Customer']", entities);
            
            // Assert
            Assert.Single(result);
            Assert.Equal("Customer", ((IEntity)result.First()).EntityType);
        }

        [Fact]
        public void Select_WildcardMatch_ReturnsMatchingItems()
        {
            // Arrange
            var entities = CreateMockEntities(new[] { "Customer", "CustomerOrder", "Product" });
            
            // Act
            var result = _selectEz.Select("$.[EntityType='Customer*']", entities);
            
            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => ((IEntity)e).EntityType == "Customer");
            Assert.Contains(result, e => ((IEntity)e).EntityType == "CustomerOrder");
        }

        [Fact]
        public void Select_RelationshipByName_ReturnsMatchingItems()
        {
            // Arrange
            var entity = CreateMockEntityWithRelationships(
                "Customer", 
                new[] { "CustomerAddress", "CustomerOrder", "CustomerPayment" });
            
            // Act
            var result = _selectEz.Select("$.Relationships[ConstraintName='CustomerOrder']", entity);
            
            // Assert
            Assert.Single(result);
            Assert.Equal("CustomerOrder", ((IRelationship)result.First()).ConstraintName);
        }

        [Fact]
        public void Select_RelationshipByWildcard_ReturnsMatchingItems()
        {
            // Arrange
            var entity = CreateMockEntityWithRelationships(
                "Customer", 
                new[] { "CustomerAddress", "CustomerOrder", "CustomerPayment" });
            
            // Act
            var result = _selectEz.Select("$.Relationships[ConstraintName='Customer*']", entity);
            
            // Assert
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void Select_ParentNodeNavigation_ReturnsMatchingItems()
        {
            // Arrange
            var entities = CreateMockEntities(new[] { "Customer", "Order", "Product" });
            var context = new Dictionary<string, object>
            {
                { "Entities", entities },
                { "CurrentEntity", entities.First(e => e.EntityType == "Order") }
            };
            
            // Act
            var result = _selectEz.Select("$..[EntityType='Customer']", context["CurrentEntity"], context);
            
            // Assert
            Assert.Single(result);
            Assert.Equal("Customer", ((IEntity)result.First()).EntityType);
        }

        [Fact]
        public void Select_AbsolutePath_ReturnsMatchingItems()
        {
            // Arrange
            var entities = CreateMockEntities(new[] { "Customer", "Order", "Product" });
            var context = new Dictionary<string, object>
            {
                { "Entities", entities },
                { "CurrentEntity", entities.First(e => e.EntityType == "Order") }
            };
            
            // Act
            var result = _selectEz.Select("/Entities[EntityType='Product']", context["CurrentEntity"], context);
            
            // Assert
            Assert.Single(result);
            Assert.Equal("Product", ((IEntity)result.First()).EntityType);
        }

        [Fact]
        public void Select_MultipleConditions_ReturnsMatchingItems()
        {
            // Arrange
            var entities = new List<IEntity>
            {
                CreateMockEntityWithProperties("Customer", new Dictionary<string, bool> { { "Id", true } }),
                CreateMockEntityWithProperties("Order", new Dictionary<string, bool> { { "Id", true } }),
                CreateMockEntityWithProperties("Product", new Dictionary<string, bool> { { "Id", false } })
            };
            
            // Act
            var result = _selectEz.Select("$.[EntityType='*' && Properties['Id'].IsPrimaryKey=true]", entities);
            
            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => ((IEntity)e).EntityType == "Customer");
            Assert.Contains(result, e => ((IEntity)e).EntityType == "Order");
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
