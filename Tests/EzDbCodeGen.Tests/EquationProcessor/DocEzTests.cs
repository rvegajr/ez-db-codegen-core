using System;
using System.Collections.Generic;
using System.Text;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Objects;
using Moq;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class DocEzTests
    {
        private readonly DocEz _docEz;

        // Custom implementation of ICustomAttributes for testing
        private class CustomAttributes : Dictionary<string, object>, ICustomAttributes
        {
            public CustomAttributes() : base(StringComparer.OrdinalIgnoreCase) { }
        }

        public DocEzTests()
        {
            _docEz = new DocEz();
        }

        [Fact]
        public void GenerateDoc_PropertyXmlDoc_ReturnsCorrectFormat()
        {
            // Arrange
            var propertyMock = new Mock<IProperty>();
            propertyMock.Setup(p => p.PropertyName).Returns("CustomerId");
            
            var customAttributes = new CustomAttributes();
            customAttributes.Add("Description", "Primary key for Customer records");
            propertyMock.Setup(p => p.CustomAttributes).Returns(customAttributes);
            
            propertyMock.Setup(p => p.IsPrimaryKey).Returns(true);
            propertyMock.Setup(p => p.DataType).Returns("int");

            // Act
            string result = _docEz.GenerateDoc(propertyMock.Object, "xml");

            // Assert
            Assert.Contains("/// <summary>", result);
            Assert.Contains("Primary key for Customer records", result);
            Assert.Contains("/// </summary>", result);
            Assert.Contains("/// <remarks>Primary key</remarks>", result);
        }

        [Fact]
        public void GenerateDoc_PropertyJsDoc_ReturnsCorrectFormat()
        {
            // Arrange
            var propertyMock = new Mock<IProperty>();
            propertyMock.Setup(p => p.PropertyName).Returns("CustomerId");
            
            var customAttributes = new CustomAttributes();
            customAttributes.Add("Description", "Primary key for Customer records");
            propertyMock.Setup(p => p.CustomAttributes).Returns(customAttributes);
            
            propertyMock.Setup(p => p.IsPrimaryKey).Returns(true);
            propertyMock.Setup(p => p.DataType).Returns("int");

            // Act
            string result = _docEz.GenerateDoc(propertyMock.Object, "jsdoc");

            // Assert
            Assert.Contains("/**", result);
            Assert.Contains(" * Primary key for Customer records", result);
            Assert.Contains(" * @type {number}", result);
            Assert.Contains(" * @primaryKey", result);
            Assert.Contains(" */", result);
        }

        [Fact]
        public void GenerateDoc_RelationshipXmlDoc_ReturnsCorrectFormat()
        {
            // Arrange
            var relationshipMock = new Mock<IRelationship>();
            relationshipMock.Setup(r => r.ConstraintName).Returns("FK_Orders_Customers");
            relationshipMock.Setup(r => r.FromTableName).Returns("Orders");
            relationshipMock.Setup(r => r.ToTableName).Returns("Customers");
            relationshipMock.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ManyToOne);

            // Act
            string result = _docEz.GenerateDoc(relationshipMock.Object, "xml");

            // Assert
            Assert.Contains("/// <summary>", result);
            Assert.Contains("Relationship between Orders and Customers", result);
            Assert.Contains("/// </summary>", result);
            Assert.Contains("/// <remarks>Many-to-One relationship (FK_Orders_Customers)</remarks>", result);
        }

        [Fact]
        public void GenerateDoc_UnsupportedType_ThrowsArgumentException()
        {
            // Arrange
            var obj = new object();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _docEz.GenerateDoc(obj, "xml"));
        }
    }
}
