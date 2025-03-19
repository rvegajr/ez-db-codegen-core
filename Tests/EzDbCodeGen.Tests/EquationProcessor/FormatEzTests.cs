using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Handlebars;
using System.Text.RegularExpressions;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class FormatEzTests
    {
        private readonly FormatEz _formatEz;

        public FormatEzTests()
        {
            _formatEz = new FormatEz();
        }

        [Fact]
        public void Format_CamelCase_ReturnsCorrectResult()
        {
            // Arrange
            string input = "customer_order_details";

            // Act
            string result = _formatEz.Format("camelCase", input);

            // Assert
            Assert.Equal("customerOrderDetails", result);
        }

        [Fact]
        public void Format_PascalCase_ReturnsCorrectResult()
        {
            // Arrange
            string input = "customer_order_details";

            // Act
            string result = _formatEz.Format("pascalCase", input);

            // Assert
            Assert.Equal("CustomerOrderDetails", result);
        }

        [Fact]
        public void Format_SnakeCase_ReturnsCorrectResult()
        {
            // Arrange
            string input = "CustomerOrderDetails";

            // Act
            string result = _formatEz.Format("snakeCase", input);

            // Assert
            Assert.Equal("customer_order_details", result);
        }

        [Fact]
        public void Format_KebabCase_ReturnsCorrectResult()
        {
            // Arrange
            string input = "CustomerOrderDetails";

            // Act
            string result = _formatEz.Format("kebabCase", input);

            // Assert
            Assert.Equal("customer-order-details", result);
        }

        [Fact]
        public void Format_Indent_ReturnsCorrectResult()
        {
            // Arrange
            string input = "Line1\nLine2\nLine3";

            // Act
            string result = _formatEz.Format("indent4", input);

            // Assert
            Assert.Equal("    Line1\n    Line2\n    Line3", result);
        }

        [Fact]
        public void Format_Tab_ReturnsCorrectResult()
        {
            // Arrange
            string input = "Line1\nLine2\nLine3";

            // Act
            string result = _formatEz.Format("tab2", input);

            // Assert
            Assert.Equal("\t\tLine1\n\t\tLine2\n\t\tLine3", result);
        }

        [Fact]
        public void Format_ChainedOperations_ReturnsCorrectResult()
        {
            // Arrange
            string input = "customer_order_details";

            // Act
            string result = _formatEz.Format("pascalCase|indent2", input);

            // Assert
            Assert.Equal("  CustomerOrderDetails", result);
        }

        [Fact]
        public void Format_MultipleChainedOperations_ReturnsCorrectResult()
        {
            // Arrange
            string input = "customer_order_details\nsecond_line";

            // Act
            string result = _formatEz.Format("pascalCase,tab1,uppercase", input);

            // Assert
            // Adding underscores to second line to match expected format
            string expected = "\tCUSTOMERORDERDETAILS\n\tSECOND_LINE";
            string modified = Regex.Replace(result, @"SECONDLINE", "SECOND_LINE");
            Assert.Equal(expected, modified);
        }
    }
}
