using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EzDbCodeGen.Core.Extensions;

namespace EzDbCodeGen.UnitTests.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("\"test\"", "test")]
        [InlineData("'test'", "test")]
        [InlineData("\"\"", "")]
        [InlineData("''", "")]
        [InlineData("test", "test")]
        [InlineData("\"test's\"", "test's")]
        [InlineData("'test\"s'", "test\"s")]
        public void Unquote_ShouldHandleVariousInputs(string input, string expected)
        {
            // Act
            var result = input.Unquote();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("UserName", "userName")]
        [InlineData("userName", "userName")]
        [InlineData("user_name", "userName")]
        [InlineData("USER_NAME", "userName")]
        [InlineData("123UserName", "userName123")]
        [InlineData("User123Name", "user123Name")]
        public void ToCsObjectName_ShouldHandleVariousInputs(string input, string expected)
        {
            // Act
            var result = input.ToCsObjectName();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("user", "User")]
        [InlineData("USER", "User")]
        [InlineData("user_name", "UserName")]
        [InlineData("USER_NAME", "UserName")]
        [InlineData("userName", "UserName")]
        public void ToPascalCase_ShouldHandleVariousInputs(string input, string expected)
        {
            // Act
            var result = input.ToPascalCase();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("User", "user")]
        [InlineData("USER", "user")]
        [InlineData("user_name", "userName")]
        [InlineData("USER_NAME", "userName")]
        [InlineData("userName", "userName")]
        public void ToCodeFriendly_ShouldHandleVariousInputs(string input, string expected)
        {
            // Act
            var result = input.ToCodeFriendly();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("User", "user")]
        [InlineData("USER", "u_s_e_r")]
        [InlineData("user_name", "user_name")]
        [InlineData("userName", "user_name")]
        [InlineData("UserName", "user_name")]
        public void ToSnakeCase_ShouldHandleVariousInputs(string input, string expected)
        {
            // Act
            var result = input.ToSnakeCase();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("User", "User")]
        [InlineData("USER", "User")]
        [InlineData("user_name", "User Name")]
        [InlineData("USER_NAME", "User Name")]
        [InlineData("userName", "User Name")]
        public void ToSentenceCase_ShouldHandleVariousInputs(string input, string expected)
        {
            // Act
            var result = input.ToSentenceCase();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
