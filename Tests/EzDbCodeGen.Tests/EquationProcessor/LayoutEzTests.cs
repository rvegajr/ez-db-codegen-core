using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Handlebars;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class LayoutEzTests
    {
        private readonly LayoutEz _layoutEz;

        public LayoutEzTests()
        {
            _layoutEz = new LayoutEz();
        }

        [Fact]
        public void AddToRegion_ValidRegion_AddsContent()
        {
            // Arrange
            string regionName = "imports";
            string content = "using System;";

            // Act
            _layoutEz.AddToRegion(regionName, content);
            string result = _layoutEz.RenderRegion(regionName);

            // Assert
            Assert.Contains(content, result);
        }

        [Fact]
        public void AddToRegion_MultipleAdditions_AppendsContent()
        {
            // Arrange
            string regionName = "imports";
            string content1 = "using System;";
            string content2 = "using System.Collections.Generic;";

            // Act
            _layoutEz.AddToRegion(regionName, content1);
            _layoutEz.AddToRegion(regionName, content2);
            string result = _layoutEz.RenderRegion(regionName);

            // Assert
            Assert.Contains(content1, result);
            Assert.Contains(content2, result);
        }

        [Fact]
        public void RenderRegion_NonExistentRegion_ReturnsEmptyString()
        {
            // Act
            string result = _layoutEz.RenderRegion("nonexistent");

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void RenderAllRegions_MultipleRegions_ReturnsAllContent()
        {
            // Arrange
            _layoutEz.AddToRegion("imports", "using System;");
            _layoutEz.AddToRegion("classDefinition", "public class Customer {}");
            _layoutEz.SetRegionOrder(new[] { "imports", "classDefinition" });

            // Act
            string result = _layoutEz.RenderAllRegions();

            // Assert
            Assert.Contains("using System;", result);
            Assert.Contains("public class Customer {}", result);
        }

        [Fact]
        public void RenderAllRegions_WithCustomOrder_RespectsOrder()
        {
            // Arrange
            _layoutEz.AddToRegion("imports", "using System;");
            _layoutEz.AddToRegion("classDefinition", "public class Customer {}");
            _layoutEz.SetRegionOrder(new[] { "classDefinition", "imports" });

            // Act
            string result = _layoutEz.RenderAllRegions();

            // Assert
            int importsIndex = result.IndexOf("using System;");
            int classDefIndex = result.IndexOf("public class Customer {}");
            Assert.True(classDefIndex < importsIndex);
        }

        [Fact]
        public void ClearRegion_ExistingRegion_RemovesContent()
        {
            // Arrange
            string regionName = "imports";
            _layoutEz.AddToRegion(regionName, "using System;");

            // Act
            _layoutEz.ClearRegion(regionName);
            string result = _layoutEz.RenderRegion(regionName);

            // Assert
            Assert.Equal("", result);
        }
    }
}
