using System;
using EzDbCodeGen.Core.Handlebars;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class CodeFormatEzTests
    {
        private readonly CodeFormatEz _codeFormatEz;

        public CodeFormatEzTests()
        {
            _codeFormatEz = new CodeFormatEz();
        }

        [Fact]
        public void FormatCode_CSharp_ReturnsFormattedCode()
        {
            // Arrange
            string code = "public class Customer{public int Id{get;set;}}";

            // Act
            string result = _codeFormatEz.FormatCode("csharp", code);

            // Assert
            Assert.Contains("public class Customer", result);
            Assert.Contains("public int Id { get; set; }", result);
        }

        [Fact]
        public void FormatCode_SQL_ReturnsFormattedCode()
        {
            // Arrange
            string code = "SELECT id,name,email FROM customers WHERE id=1";

            // Act
            string result = _codeFormatEz.FormatCode("sql", code);

            // Assert
            Assert.Contains("SELECT", result);
            Assert.Contains("FROM", result);
            Assert.Contains("WHERE", result);
        }

        [Fact]
        public void FormatCode_TypeScript_ReturnsFormattedCode()
        {
            // Arrange
            string code = "interface Customer{id:number;name:string;}";

            // Act
            string result = _codeFormatEz.FormatCode("typescript", code);

            // Assert
            Assert.Contains("interface Customer", result);
            Assert.Contains("id: number;", result);
            Assert.Contains("name: string;", result);
        }

        [Fact]
        public void FormatCode_HTML_ReturnsFormattedCode()
        {
            // Arrange
            string code = "<div><p>Hello</p><p>World</p></div>";

            // Act
            string result = _codeFormatEz.FormatCode("html", code);

            // Assert
            Assert.Contains("<div>", result);
            Assert.Contains("  <p>Hello</p>", result);
            Assert.Contains("  <p>World</p>", result);
            Assert.Contains("</div>", result);
        }

        [Fact]
        public void FormatCode_UnsupportedLanguage_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _codeFormatEz.FormatCode("unsupported", "code"));
        }
    }
}
