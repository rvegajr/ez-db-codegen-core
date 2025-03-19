using System;
using EzDbCodeGen.Core.Handlebars;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class ConvertTypeEzTests
    {
        private readonly ConvertTypeEz _convertTypeEz;

        public ConvertTypeEzTests()
        {
            _convertTypeEz = new ConvertTypeEz();
        }

        [Theory]
        [InlineData("int", "csharp", false, "int")]
        [InlineData("int", "csharp", true, "int?")]
        [InlineData("nvarchar(50)", "csharp", false, "string")]
        [InlineData("nvarchar(50)", "csharp", true, "string?")]
        [InlineData("bit", "csharp", false, "bool")]
        [InlineData("datetime", "csharp", false, "DateTime")]
        public void ConvertType_CSharp_ReturnsCorrectType(string sqlType, string targetLanguage, bool nullable, string expected)
        {
            // Act
            string result = _convertTypeEz.Convert(sqlType, targetLanguage, nullable);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("int", "typescript", false, "number")]
        [InlineData("int", "typescript", true, "number | null")]
        [InlineData("nvarchar(50)", "typescript", false, "string")]
        [InlineData("bit", "typescript", false, "boolean")]
        [InlineData("datetime", "typescript", false, "Date")]
        public void ConvertType_TypeScript_ReturnsCorrectType(string sqlType, string targetLanguage, bool nullable, string expected)
        {
            // Act
            string result = _convertTypeEz.Convert(sqlType, targetLanguage, nullable);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("int", "java", false, "Integer")]
        [InlineData("int", "java", true, "Integer")]
        [InlineData("nvarchar(50)", "java", false, "String")]
        [InlineData("bit", "java", false, "Boolean")]
        [InlineData("datetime", "java", false, "Date")]
        public void ConvertType_Java_ReturnsCorrectType(string sqlType, string targetLanguage, bool nullable, string expected)
        {
            // Act
            string result = _convertTypeEz.Convert(sqlType, targetLanguage, nullable);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("int", "python", false, "int")]
        [InlineData("nvarchar(50)", "python", false, "str")]
        [InlineData("bit", "python", false, "bool")]
        [InlineData("datetime", "python", false, "datetime")]
        public void ConvertType_Python_ReturnsCorrectType(string sqlType, string targetLanguage, bool nullable, string expected)
        {
            // Act
            string result = _convertTypeEz.Convert(sqlType, targetLanguage, nullable);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertType_UnsupportedLanguage_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _convertTypeEz.Convert("int", "unsupported", false));
        }
    }
}
