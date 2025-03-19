using System;
using System.Collections.Generic;
using System.Dynamic;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Extentions;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class IfCondEzTests
    {
        private readonly IfCondEz _ifCondEz;

        public IfCondEzTests()
        {
            _ifCondEz = new IfCondEz();
        }

        [Fact]
        public void Evaluate_SimpleEquality_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("Table");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType='Table'", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_SimpleEquality_ReturnsFalse()
        {
            // Arrange
            var entity = CreateMockEntity("Table");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType='View'", entity);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_ComplexCondition_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("View", 0);
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType='View' && $.Relationships.Count=0", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ComplexCondition_ReturnsFalse()
        {
            // Arrange
            var entity = CreateMockEntity("View", 2);
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType='View' && $.Relationships.Count=0", entity);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_NestedPropertyAccess_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntityWithNestedProperties("Table", true);
            
            // Act
            var result = _ifCondEz.Evaluate("$.Properties['Id'].IsPrimaryKey=true", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_MultipleConditions_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntityWithNestedProperties("Table", true);
            entity.Properties.Add("Name", CreateMockProperty("Name", "varchar", 50));
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType='Table' && $.Properties.Count > 1 && $.Properties['Id'].IsPrimaryKey=true", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_OrCondition_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("View");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType='View' || $.EntityType='Table'", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ComplexOrAndCondition_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntityWithNestedProperties("View", false);
            entity.Properties.Add("Name", CreateMockProperty("Name", "varchar", 50));
            
            // Act
            var result = _ifCondEz.Evaluate("($.EntityType='View' || $.EntityType='Report') && ($.Properties.Count > 1 || $.Properties['Id'].IsPrimaryKey=true)", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_NotOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("Table");
            
            // Act
            var result = _ifCondEz.Evaluate("!($.EntityType='View')", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_GreaterThanOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("Table", 5);
            
            // Act
            var result = _ifCondEz.Evaluate("$.Relationships.Count > 3", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_LessThanOperator_ReturnsFalse()
        {
            // Arrange
            var entity = CreateMockEntity("Table", 5);
            
            // Act
            var result = _ifCondEz.Evaluate("$.Relationships.Count < 3", entity);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Evaluate_GreaterThanOrEqualOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("Table", 3);
            
            // Act
            var result = _ifCondEz.Evaluate("$.Relationships.Count >= 3", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_LessThanOrEqualOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("Table", 3);
            
            // Act
            var result = _ifCondEz.Evaluate("$.Relationships.Count <= 3", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_NotEqualOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("Table");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType!='View'", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ContainsOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("UserTable");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType contains 'Table'", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_StartsWithOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("TableUser");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType startsWith 'Table'", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_EndsWithOperator_ReturnsTrue()
        {
            // Arrange
            var entity = CreateMockEntity("UserTable");
            
            // Act
            var result = _ifCondEz.Evaluate("$.EntityType endsWith 'Table'", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_ComplexNestedCondition_ReturnsTrue()
        {
            // Arrange
            var entity = CreateComplexEntity();
            
            // Act
            var result = _ifCondEz.Evaluate("$.Properties['Id'].IsPrimaryKey=true && ($.Relationships.Count > 0 || ($.EntityType='Table' && $.Properties.Count >= 2))", entity);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Evaluate_InvalidExpression_ThrowsException()
        {
            // Arrange
            var entity = CreateMockEntity("Table");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _ifCondEz.Evaluate("$.EntityType=", entity));
        }

        [Fact]
        public void Evaluate_NullEntity_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _ifCondEz.Evaluate("$.EntityType='Table'", null));
        }

        [Fact]
        public void Evaluate_EmptyExpression_ThrowsException()
        {
            // Arrange
            var entity = CreateMockEntity("Table");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _ifCondEz.Evaluate("", entity));
        }

        private IEntity CreateMockEntity(string entityType, int relationshipCount = 0)
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.EntityType).Returns(entityType);
            
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
