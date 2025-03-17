using System;
using System.IO;
using System.Text;
using HandlebarsDotNet;
using EzDbCodeGen.Core.Handlebars;
using Xunit;

namespace EzDbCodeGen.Tests
{
    public class HandlebarsUtilityTests
    {
        [Fact]
        public void RegisterHelpers_RegistersAllHelpers()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            
            // Act
            HandlebarsUtility.RegisterHelpers(handlebars);
            
            // Assert - If no exception is thrown, the test passes
            // This is a basic test to ensure the registration process completes without errors
        }
        
        [Fact]
        public void StringFormatHelpers_ToPascalCase_FormatsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ToPascalCase value}}");
            var data = new { value = "hello_world" };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Equal("HelloWorld", result);
        }
        
        [Fact]
        public void StringFormatHelpers_ToCamelCase_FormatsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ToCamelCase value}}");
            var data = new { value = "hello_world" };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Equal("helloWorld", result);
        }
        
        [Fact]
        public void StringFormatHelpers_ToSnakeCase_FormatsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ToSnakeCase value}}");
            var data = new { value = "HelloWorld" };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Equal("hello_world", result);
        }
        
        [Fact]
        public void StringFormatHelpers_ExtractTableName_ExtractsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ExtractTableName value}}");
            var data = new { value = "dbo.Customer" };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Equal("Customer", result);
        }
        
        [Fact]
        public void StringFormatHelpers_ExtractSchemaName_ExtractsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ExtractSchemaName value}}");
            var data = new { value = "dbo.Customer" };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Equal("dbo", result);
        }
        
        [Fact]
        public void ComparisonHelpers_IfCond_EvaluatesCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{#IfCond value1 '==' value2}}Equal{{else}}Not Equal{{/IfCond}}");
            
            // Act & Assert
            var data1 = new { value1 = 5, value2 = 5 };
            var result1 = template(data1);
            Assert.Equal("Equal", result1);
            
            var data2 = new { value1 = 5, value2 = 10 };
            var result2 = template(data2);
            Assert.Equal("Not Equal", result2);
        }
        
        [Fact]
        public void ComparisonHelpers_Switch_EvaluatesCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile(
                "{{#Switch value}}" +
                "{{#Case 'A'}}Value is A{{/Case}}" +
                "{{#Case 'B'}}Value is B{{/Case}}" +
                "{{#Default}}Value is something else{{/Default}}" +
                "{{/Switch}}");
            
            // Act & Assert
            var dataA = new { value = "A" };
            var resultA = template(dataA);
            Assert.Equal("Value is A", resultA);
            
            var dataB = new { value = "B" };
            var resultB = template(dataB);
            Assert.Equal("Value is B", resultB);
            
            var dataC = new { value = "C" };
            var resultC = template(dataC);
            Assert.Equal("Value is something else", resultC);
        }
        
        [Fact]
        public void TypeConversionHelpers_ToNetType_ConvertsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ToNetType value}}");
            
            // Act & Assert
            var data1 = new { value = "varchar" };
            var result1 = template(data1);
            Assert.Equal("string", result1);
            
            var data2 = new { value = "int" };
            var result2 = template(data2);
            Assert.Equal("int", result2);
        }
        
        [Fact]
        public void TypeConversionHelpers_AsNullableType_ConvertsCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{AsNullableType type isNullable}}");
            
            // Act & Assert
            var data1 = new { type = "int", isNullable = true };
            var result1 = template(data1);
            Assert.Equal("int?", result1);
            
            var data2 = new { type = "string", isNullable = true };
            var result2 = template(data2);
            Assert.Equal("string", result2);
            
            var data3 = new { type = "int", isNullable = false };
            var result3 = template(data3);
            Assert.Equal("int", result3);
        }
        
        [Fact]
        public void BasicHelpers_ToJson_SerializesCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ToJson obj}}");
            var data = new { obj = new { name = "Test", value = 123 } };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Contains("\"name\":\"Test\"", result);
            Assert.Contains("\"value\":123", result);
        }
        
        [Fact]
        public void ContextHelpers_ContextAsJson_SerializesCorrectly()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            HandlebarsUtility.RegisterHelpers(handlebars);
            var template = handlebars.Compile("{{ContextAsJson}}");
            var data = new { name = "Test", value = 123 };
            
            // Act
            var result = template(data);
            
            // Assert
            Assert.Contains("\"name\":\"Test\"", result);
            Assert.Contains("\"value\":123", result);
        }
    }
}
