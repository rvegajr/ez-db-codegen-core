namespace EzDbCodeGen.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void ToCodeFriendly_WithSpecialCharacters_ReturnsCleanString()
        {
            // Arrange
            var input = "Hello World!@#";
            
            // Act
            var result = input.ToCodeFriendly();
            
            // Assert
            Assert.Equal("Hello_World___", result);
        }
        
        [Fact]
        public void ToPascalCase_WithMixedCaseAndSpecialChars_ReturnsPascalCase()
        {
            // Arrange
            var input = "hello-world_example";
            
            // Act
            var result = input.ToPascalCase();
            
            // Assert
            Assert.Equal("HelloWorldExample", result);
        }
        
        [Fact]
        public void ToCamelCase_WithMixedCaseAndSpecialChars_ReturnsCamelCase()
        {
            // Arrange
            var input = "Hello-World_Example";
            
            // Act
            var result = input.ToCamelCase();
            
            // Assert
            Assert.Equal("helloWorldExample", result);
        }
        
        [Fact]
        public void ToSnakeCase_WithPascalCase_ReturnsSnakeCase()
        {
            // Arrange
            var input = "HelloWorldExample";
            
            // Act
            var result = input.ToSnakeCase();
            
            // Assert
            Assert.Equal("hello_world_example", result);
        }
        
        [Fact]
        public void ExtractTableName_WithSchemaAndTable_ReturnsTableName()
        {
            // Arrange
            var input = "dbo.Customer";
            
            // Act
            var result = input.ExtractTableName();
            
            // Assert
            Assert.Equal("Customer", result);
        }
        
        [Fact]
        public void ExtractSchemaName_WithSchemaAndTable_ReturnsSchemaName()
        {
            // Arrange
            var input = "dbo.Customer";
            
            // Act
            var result = input.ExtractSchemaName();
            
            // Assert
            Assert.Equal("dbo", result);
        }
        
        [Theory]
        [InlineData("int", "int")]
        [InlineData("varchar", "string")]
        [InlineData("datetime", "DateTime")]
        [InlineData("bit", "bool")]
        [InlineData("uniqueidentifier", "Guid")]
        [InlineData("unknown", "object")]
        public void ToNetType_WithDatabaseType_ReturnsNetType(string dbType, string expectedNetType)
        {
            // Act
            var result = dbType.ToNetType();
            
            // Assert
            Assert.Equal(expectedNetType, result);
        }
        
        [Fact]
        public void AsFormattedName_WithIdSuffix_RemovesIdSuffix()
        {
            // Arrange
            var input = "CustomerId";
            
            // Act
            var result = input.AsFormattedName();
            
            // Assert
            Assert.Equal("Customer", result);
        }
    }
}
