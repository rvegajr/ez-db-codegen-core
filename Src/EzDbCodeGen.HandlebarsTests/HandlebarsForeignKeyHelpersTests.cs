using EzDbSchema.Core.Enums;

namespace EzDbCodeGen.HandlebarsTests
{
    public class HandlebarsForeignKeyHelpersTests
    {
        private readonly HandlebarsUtility _handlebarsUtility;

        public HandlebarsForeignKeyHelpersTests()
        {
            _handlebarsUtility = new HandlebarsUtility();
        }

        [Fact]
        public void POCOModelFKProperties_GeneratesCorrectProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntity();
            var template = @"{{POCOModelFKProperties '    '}}";

            // Act
            _handlebarsUtility.RegisterForeignKeyHelpers();
            var compiledTemplate = _handlebarsUtility.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual Customer CustomerId { get; set; }", result);
        }

        [Fact]
        public void POCOModelFKProperties_WithSpecificForeignKey_GeneratesOnlyMatchingProperty()
        {
            // Arrange
            var mockEntity = CreateMockEntity();
            var template = @"{{POCOModelFKProperties '    ' 'FK_Orders_Customers'}}";

            // Act
            _handlebarsUtility.RegisterForeignKeyHelpers();
            var compiledTemplate = _handlebarsUtility.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual Customer CustomerId { get; set; }", result);
        }

        [Fact]
        public void POCOModelFKProperties_WithNonExistentForeignKey_GeneratesNoProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntity();
            var template = @"{{POCOModelFKProperties '    ' 'NonExistentFK'}}";

            // Act
            _handlebarsUtility.RegisterForeignKeyHelpers();
            var compiledTemplate = _handlebarsUtility.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.DoesNotContain("public virtual", result);
        }

        private IEntity CreateMockEntity()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns("Orders");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");

            var mockRelationship = new Mock<IRelationship>();
            mockRelationship.Setup(r => r.FromTableName).Returns("dbo.Orders");
            mockRelationship.Setup(r => r.FromColumnName).Returns("CustomerId");
            mockRelationship.Setup(r => r.ToTableName).Returns("dbo.Customers");
            mockRelationship.Setup(r => r.ToColumnName).Returns("CustomerId");
            mockRelationship.Setup(r => r.ConstraintName).Returns("FK_Orders_Customers");
            mockRelationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ZeroOrOneToOne);
            mockRelationship.Setup(r => r.RelationshipType).Returns("One to One");

            var relationshipList = new RelationshipReferenceList();
            relationshipList.Add(mockRelationship.Object);

            mockEntity.Setup(e => e.Relationships).Returns(relationshipList);

            return mockEntity.Object;
        }
    }
}
