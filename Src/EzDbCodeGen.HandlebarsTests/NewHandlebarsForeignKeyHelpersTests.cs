using HandlebarsDotNet;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Enums;
using Moq;
using Xunit;
using EzDbCodeGen.Core.Handlebars;

namespace EzDbCodeGen.HandlebarsTests
{
    public class NewHandlebarsForeignKeyHelpersTests
    {
        private readonly IHandlebars _handlebars;
        private readonly NewHandlebarsForeignKeyHelpers _foreignKeyHelpers;

        public NewHandlebarsForeignKeyHelpersTests()
        {
            _handlebars = Handlebars.Create();
            _foreignKeyHelpers = new NewHandlebarsForeignKeyHelpers(_handlebars);
            _foreignKeyHelpers.RegisterHelpers();
        }

        [Fact]
        public void POCOModelFKProperties_GeneratesCorrectProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntityWithOneToOne();
            var template = @"{{POCOModelFKProperties '    '}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual Customer Customer { get; set; }", result);
        }

        [Fact]
        public void POCOModelFKProperties_WithSpecificForeignKey_GeneratesOnlyMatchingProperty()
        {
            // Arrange
            var mockEntity = CreateMockEntityWithOneToOne();
            var template = @"{{POCOModelFKProperties '    ' 'FK_Orders_Customers'}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual Customer Customer { get; set; }", result);
        }

        [Fact]
        public void POCOModelFKManyToZeroToOne_GeneratesCorrectProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntityWithManyToZeroOrOne();
            var template = @"{{POCOModelFKManyToZeroToOne '    '}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual Employee Manager { get; set; }", result);
        }

        [Fact]
        public void POCOModelFKManyToOne_GeneratesCorrectProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntityWithManyToOne();
            var template = @"{{POCOModelFKManyToOne '    '}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual Category Category { get; set; }", result);
        }

        [Fact]
        public void POCOModelCollectionProperties_GeneratesCorrectProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntityWithOneToMany();
            var template = @"{{POCOModelCollectionProperties '    '}}";

            // Act
            var compiledTemplate = _handlebars.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();", result);
        }

        private IEntity CreateMockEntityWithOneToOne()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns("Order");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");

            var relationship = new Mock<IRelationship>();
            relationship.Setup(r => r.ConstraintName).Returns("FK_Orders_Customers");
            relationship.Setup(r => r.ToTableName).Returns("dbo.Customer");
            relationship.Setup(r => r.ToPropertyName).Returns("Customer");
            relationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.OneToOne);

            mockEntity.Setup(e => e.Relationships).Returns(new List<IRelationship> { relationship.Object });
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithManyToZeroOrOne()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns("Employee");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");

            var relationship = new Mock<IRelationship>();
            relationship.Setup(r => r.ConstraintName).Returns("FK_Employees_Managers");
            relationship.Setup(r => r.ToTableName).Returns("dbo.Employee");
            relationship.Setup(r => r.ToPropertyName).Returns("Manager");
            relationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ManyToZeroOrOne);

            mockEntity.Setup(e => e.Relationships).Returns(new List<IRelationship> { relationship.Object });
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithManyToOne()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns("Product");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");

            var relationship = new Mock<IRelationship>();
            relationship.Setup(r => r.ConstraintName).Returns("FK_Products_Categories");
            relationship.Setup(r => r.ToTableName).Returns("dbo.Category");
            relationship.Setup(r => r.ToPropertyName).Returns("Category");
            relationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.ManyToOne);

            mockEntity.Setup(e => e.Relationships).Returns(new List<IRelationship> { relationship.Object });
            return mockEntity.Object;
        }

        private IEntity CreateMockEntityWithOneToMany()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns("Customer");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");

            var relationship = new Mock<IRelationship>();
            relationship.Setup(r => r.ConstraintName).Returns("FK_Orders_Customers");
            relationship.Setup(r => r.FromTableName).Returns("dbo.Order");
            relationship.Setup(r => r.FromPropertyName).Returns("Orders");
            relationship.Setup(r => r.MultiplicityType).Returns(RelationshipMultiplicityType.OneToMany);

            mockEntity.Setup(e => e.Relationships).Returns(new List<IRelationship> { relationship.Object });
            return mockEntity.Object;
        }
    }
}
