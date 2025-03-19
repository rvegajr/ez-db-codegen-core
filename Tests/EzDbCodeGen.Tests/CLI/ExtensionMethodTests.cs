using System;
using System.Collections.Generic;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Extentions;
using EzDbCodeGen.Core.Extensions;
using Xunit;
using Moq;

namespace EzDbCodeGen.Tests.CLI
{
    public class ExtensionMethodTests
    {
        [Fact]
        public void StringExtensions_ToCodeFriendly_WorksCorrectly()
        {
            // Arrange
            string input1 = "HelloWorld";
            string input2 = "USER_NAME";
            string input3 = "camelCase";

            // Act
            string result1 = EzDbCodeGen.Core.Extensions.StringExtensions.ToCodeFriendly(input1);
            string result2 = EzDbCodeGen.Core.Extensions.StringExtensions.ToCodeFriendly(input2);
            string result3 = EzDbCodeGen.Core.Extensions.StringExtensions.ToCodeFriendly(input3);

            // Assert
            Assert.Equal("hello_world", result1);
            Assert.Equal("user_name", result2);
            Assert.Equal("camel_case", result3);
        }

        [Fact]
        public void StringExtensions_ToPascalCase_WorksCorrectly()
        {
            // Arrange
            string input1 = "hello_world";
            string input2 = "USER_NAME";
            string input3 = "camelCase";

            // Act
            string result1 = EzDbCodeGen.Core.Extensions.StringExtensions.ToPascalCase(input1);
            string result2 = EzDbCodeGen.Core.Extensions.StringExtensions.ToPascalCase(input2);
            string result3 = EzDbCodeGen.Core.Extensions.StringExtensions.ToPascalCase(input3);

            // Assert
            Assert.Equal("HelloWorld", result1);
            Assert.Equal("UserName", result2);
            Assert.Equal("CamelCase", result3);
        }

        [Fact]
        public void StringExtensions_ToCamelCase_WorksCorrectly()
        {
            // Arrange
            string input1 = "hello_world";
            string input2 = "USER_NAME";
            string input3 = "123Start";

            // Act
            string result1 = EzDbCodeGen.Core.Extensions.StringExtensions.ToCamelCase(input1);
            string result2 = EzDbCodeGen.Core.Extensions.StringExtensions.ToCamelCase(input2);
            string result3 = EzDbCodeGen.Core.Extensions.StringExtensions.ToCamelCase(input3);

            // Assert
            Assert.Equal("helloWorld", result1);
            Assert.Equal("userName", result2);
            Assert.Equal("_123Start", result3);
        }

        [Fact]
        public void StringExtensions_ToSnakeCase_WorksCorrectly()
        {
            // Arrange
            string input1 = "HelloWorld";
            string input2 = "USER_NAME";
            string input3 = "camelCase";

            // Act
            string result1 = EzDbCodeGen.Core.Extensions.StringExtensions.ToSnakeCase(input1);
            string result2 = EzDbCodeGen.Core.Extensions.StringExtensions.ToSnakeCase(input2);
            string result3 = EzDbCodeGen.Core.Extensions.StringExtensions.ToSnakeCase(input3);

            // Assert
            Assert.Equal("hello_world", result1);
            Assert.Equal("user_name", result2);
            Assert.Equal("camel_case", result3);
        }

        [Fact]
        public void StringExtensions_NullHandling_WorksCorrectly()
        {
            // Arrange
            string? nullString = null;

            // Act & Assert - These should not throw exceptions
            Assert.Equal(string.Empty, EzDbCodeGen.Core.Extensions.StringExtensions.ToCodeFriendly(nullString));
            Assert.Equal(string.Empty, EzDbCodeGen.Core.Extensions.StringExtensions.ToPascalCase(nullString));
            Assert.Equal(string.Empty, EzDbCodeGen.Core.Extensions.StringExtensions.ToCamelCase(nullString));
            Assert.Equal(string.Empty, EzDbCodeGen.Core.Extensions.StringExtensions.ToSnakeCase(nullString));
        }
    }
}
