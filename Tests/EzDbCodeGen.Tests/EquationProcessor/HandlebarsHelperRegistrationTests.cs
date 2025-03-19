using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Handlebars;
using HandlebarsDotNet;
using Xunit;

namespace EzDbCodeGen.Tests.EquationProcessor
{
    public class HandlebarsHelperRegistrationTests
    {
        [Fact]
        public void RegisterAllHelpers_RegistersAllFormatHelpers()
        {
            // Arrange
            var handlebars = Handlebars.Create();
            
            // Act
            HandlebarsHelperRegistration.RegisterAllHelpers(handlebars);
            
            // Assert - Verify all helpers are registered by checking if they can be used
            var template = handlebars.Compile("{{FormatEz 'camelCase' 'TestString'}}");
            var result = template(new object());
            Assert.Equal("testString", result);
            
            template = handlebars.Compile("{{ConvertTypeEz 'int' 'csharp' false}}");
            result = template(new object());
            Assert.Equal("int", result);
            
            // Test LayoutEz by adding to a region and rendering it
            template = handlebars.Compile(@"
                {{#LayoutEz 'region' 'test'}}Content{{/LayoutEz}}
                {{RenderRegion 'test'}}
            ");
            result = template(new object());
            Assert.Contains("Content", result);
            
            // Test formatCode alias
            template = handlebars.Compile("{{formatCode 'csharp' 'public class Test{}'}}");
            result = template(new object());
            Assert.Contains("public class Test", result);
        }
        
        [Fact]
        public void RegisterAllHelpers_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => HandlebarsHelperRegistration.RegisterAllHelpers(null));
        }
    }
}
