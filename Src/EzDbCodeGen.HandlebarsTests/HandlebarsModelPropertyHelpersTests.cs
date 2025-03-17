namespace EzDbCodeGen.HandlebarsTests
{
    public class HandlebarsModelPropertyHelpersTests
    {
        private readonly HandlebarsUtility _handlebarsUtility;

        public HandlebarsModelPropertyHelpersTests()
        {
            _handlebarsUtility = new HandlebarsUtility();
        }

        [Fact]
        public void POCOModelProperties_GeneratesCorrectProperties()
        {
            // Arrange
            var mockEntity = CreateMockEntity();
            var template = @"{{POCOModelProperties '    '}}";            

            // Act
            _handlebarsUtility.RegisterModelPropertyHelpers();
            var compiledTemplate = _handlebarsUtility.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("public int Id { get; set; }", result);
            Assert.Contains("public string? Name { get; set; }", result);
        }

        [Fact]
        public void POCOModelProperties_WithAnnotations_GeneratesAttributes()
        {
            // Arrange
            var mockEntity = CreateMockEntity();
            var template = @"{{POCOModelProperties '    ' true}}";

            // Act
            _handlebarsUtility.RegisterModelPropertyHelpers();
            var compiledTemplate = _handlebarsUtility.Compile(template);
            var result = compiledTemplate(mockEntity);

            // Assert
            Assert.Contains("[Required]", result);
            Assert.Contains("[MaxLength(100)]", result);
        }

        private IEntity CreateMockEntity()
        {
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.TableName).Returns("TestEntity");
            mockEntity.Setup(e => e.DatabaseSchema).Returns("dbo");

            var mockProperty1 = new Mock<IProperty>();
            mockProperty1.Setup(p => p.PropertyName).Returns("Id");
            mockProperty1.Setup(p => p.DataType).Returns("int");
            mockProperty1.Setup(p => p.IsNullable).Returns(false);

            var mockProperty2 = new Mock<IProperty>();
            mockProperty2.Setup(p => p.PropertyName).Returns("Name");
            mockProperty2.Setup(p => p.DataType).Returns("nvarchar");
            mockProperty2.Setup(p => p.IsNullable).Returns(true);
            mockProperty2.Setup(p => p.MaxLength).Returns(100);

            var properties = new Dictionary<string, IProperty>
            {
                { "Id", mockProperty1.Object },
                { "Name", mockProperty2.Object }
            };
            var mockPropertyDictionary = new Mock<IPropertyDictionary>();
            mockPropertyDictionary.Setup(d => d.GetEnumerator()).Returns(properties.GetEnumerator());
            mockEntity.Setup(e => e.Properties).Returns(mockPropertyDictionary.Object);

            return mockEntity.Object;
        }
    }
}
