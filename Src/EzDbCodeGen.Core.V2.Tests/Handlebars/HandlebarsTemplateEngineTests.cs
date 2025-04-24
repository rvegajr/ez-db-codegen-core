using System;
using System.Collections.Generic;
using FluentAssertions;
using EzDbCodeGen.Core.V2.Handlebars;
using EzDbCodeGen.Core.V2.Tests.TestCompat;
using EzDbSchema.Core.V2.Interfaces;
using EzDbSchema.Core.V2.Models;
using Moq;
using Xunit;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.V2.Tests.Handlebars
{
    public class HandlebarsTemplateEngineTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange & Act
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();

            // Assert
            engine.Should().NotBeNull();
            // Testing HandlebarsDotNet instance is challenging directly
            // We'll verify through behavior tests
        }

        [Fact]
        public void Compile_ShouldReturnCompiledTemplate()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var templateContent = "Hello {{name}}!";

            // Act
            var compiled = engine.Compile(templateContent);

            // Assert
            compiled.Should().NotBeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Compile_WithInvalidTemplate_ShouldThrowArgumentException(string? invalidTemplate)
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();

            // Act & Assert
            Action act = () => engine.Compile(invalidTemplate!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("template");
        }

        [Fact]
        public void Compile_WithInvalidHandlebarsTemplate_ShouldThrowException()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var invalidTemplate = "{{#if}}"; // Missing end block

            // Act & Assert
            Action act = () => engine.Compile(invalidTemplate);
            act.Should().Throw<HandlebarsCompilerException>();
        }

        [Fact]
        public void Render_ShouldProcessTemplateWithData()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var templateContent = "Hello {{name}}!";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };

            // Act
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("Hello World!");
        }

        [Fact]
        public void Render_WithNullCompiledTemplate_ShouldThrowArgumentNullException()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var data = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => engine.Render(null!, data);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("compiledTemplate");
        }

        [Fact]
        public void Render_WithNullData_ShouldUseEmptyObject()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var templateContent = "Hello Template!";
            var compiled = engine.Compile(templateContent);

            // Act
            var result = engine.Render(compiled, null);

            // Assert
            result.Should().Be("Hello Template!");
        }

        [Fact]
        public void RegisterHelper_ShouldMakeHelperAvailableInTemplates()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register a simple helper that uppercases text
            engine.RegisterHelper("uppercase", (writer, context, parameters) => {
                var str = parameters[0]?.ToString() ?? string.Empty;
                writer.WriteSafeString(str.ToUpper());
            });
            
            var templateContent = "Hello {{uppercase name}}!";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };

            // Act
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("Hello WORLD!");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RegisterHelper_WithInvalidHelperName_ShouldThrowArgumentException(string? invalidName)
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var helperAction = new HandlebarsHelper((writer, context, parameters) => { });

            // Act & Assert
            Action act = () => engine.RegisterHelper(invalidName!, helperAction);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("helperName");
        }

        [Fact]
        public void RegisterHelper_WithNullHelperAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();

            // Act & Assert
            Action act = () => engine.RegisterHelper("test", null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("helperAction");
        }

        [Fact]
        public void RegisterBlockHelper_ShouldMakeBlockHelperAvailableInTemplates()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register a simple block helper that wraps content in brackets
            engine.RegisterBlockHelper("brackets", (writer, options, context, arguments) => {
                writer.WriteSafeString("[");
                options.Template(writer, context);
                writer.WriteSafeString("]");
            });
            
            var templateContent = "{{#brackets}}Hello {{name}}!{{/brackets}}";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };

            // Act
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("[Hello World!]");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RegisterBlockHelper_WithInvalidHelperName_ShouldThrowArgumentException(string? invalidName)
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var blockHelperAction = new HandlebarsBlockHelper((writer, options, context, arguments) => { });

            // Act & Assert
            Action act = () => engine.RegisterBlockHelper(invalidName!, blockHelperAction);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("blockHelperName");
        }

        [Fact]
        public void RegisterBlockHelper_WithNullHelperAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();

            // Act & Assert
            Action act = () => engine.RegisterBlockHelper("test", null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("blockHelperAction");
        }

        [Fact]
        public void RegisterSchemaHelpers_ShouldRegisterDatabaseHelpers()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Create a mock database for testing
            var mockDatabase = new Mock<IDatabase>();
            mockDatabase.Setup(db => db.Name).Returns("TestDb");
            
            // Act
            engine.RegisterSchemaHelpers();
            var templateContent = "Database: {{database.Name}}";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "database", mockDatabase.Object }
            };
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("Database: TestDb");
        }

        [Fact]
        public void RegisterTypeHelpers_ShouldRegisterTypeConversionHelpers()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register type helpers
            engine.RegisterTypeHelpers();
            
            // Create a property with a type
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.Name).Returns("TestEntity");
            
            var property = new Property(
                "TestColumn",
                "TestProperty",
                "varchar",
                "string",
                mockEntity.Object,
                1);
            property.MaxLength = 50;
            
            // Act
            var templateContent = "Property type: {{convertType property 'csharp'}}";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "property", property }
            };
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Contain("string");
        }

        [Fact]
        public void RegisterStringHelpers_ShouldRegisterStringManipulationHelpers()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register string helpers
            engine.RegisterStringHelpers();
            
            // Act
            var templateContent = "Plural: {{pluralize 'entity'}}";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>();
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("Plural: entities");
        }

        [Fact]
        public void RegisterArrayHelpers_ShouldRegisterArrayManipulationHelpers()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register array helpers
            engine.RegisterArrayHelpers();
            
            // Act
            var templateContent = "{{#each (take items 2)}}{{this}}{{/each}}";
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "items", new[] { "one", "two", "three", "four" } }
            };
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("onetwo");
        }

        [Fact]
        public void RegisterComparisonHelpers_ShouldRegisterComparisonHelpers()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register comparison helpers
            engine.RegisterComparisonHelpers();
            
            // Act
            var templateContent = "{{#if (eq value 10)}}Equal{{else}}Not Equal{{/if}}";
            var compiled = engine.Compile(templateContent);
            
            var equalData = new Dictionary<string, object> { { "value", 10 } };
            var unequalData = new Dictionary<string, object> { { "value", 5 } };
            
            var equalResult = engine.Render(compiled, equalData);
            var unequalResult = engine.Render(compiled, unequalData);

            // Assert
            equalResult.Should().Be("Equal");
            unequalResult.Should().Be("Not Equal");
        }

        [Fact]
        public void RegisterFormatHelpers_ShouldRegisterFormattingHelpers()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Register format helpers
            engine.RegisterFormatHelpers();
            
            // Act
            var templateContent = "{{formatDate date 'yyyy-MM-dd'}}";
            var compiled = engine.Compile(templateContent);
            var testDate = new DateTime(2023, 1, 15);
            var data = new Dictionary<string, object>
            {
                { "date", testDate }
            };
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Be("2023-01-15");
        }

        [Fact]
        public void CompileAndRender_ShouldCompileAndRenderInOneStep()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var templateContent = "Hello {{name}}!";
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };

            // Act
            var result = engine.CompileAndRender(templateContent, data);

            // Assert
            result.Should().Be("Hello World!");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CompileAndRender_WithInvalidTemplate_ShouldThrowArgumentException(string? invalidTemplate)
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var data = new Dictionary<string, object>();

            // Act & Assert
            Action act = () => engine.CompileAndRender(invalidTemplate!, data);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("template");
        }

        [Fact]
        public void CompileAndRender_WithNullData_ShouldUseEmptyObject()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            var templateContent = "Hello Template!";

            // Act
            var result = engine.CompileAndRender(templateContent, null);

            // Assert
            result.Should().Be("Hello Template!");
        }

        [Fact]
        public void RegisterAll_ShouldRegisterAllHelperTypes()
        {
            // Arrange
            var engine = TestCompatibility.CreateHandlebarsTemplateEngine();
            
            // Act
            engine.RegisterAll();
            
            // Test with a template that uses helpers from different categories
            var templateContent = @"
Entity: {{entity.Name}}
Properties: {{count entity.Properties}}
First property: {{#with (first (values entity.Properties))}}
  Name: {{PropertyName}}
  Type: {{convertType this 'csharp'}}
  Is nullable: {{#if IsNullable}}Yes{{else}}No{{/if}}
{{/with}}";
            
            var mockProperty = new Mock<IProperty>();
            mockProperty.Setup(p => p.PropertyName).Returns("Id");
            mockProperty.Setup(p => p.IsNullable).Returns(false);
            mockProperty.Setup(p => p.DataType).Returns("int");
            mockProperty.Setup(p => p.ClrType).Returns("int");
            
            var mockEntity = new Mock<IEntity>();
            mockEntity.Setup(e => e.Name).Returns("TestEntity");
            mockEntity.Setup(e => e.Properties).Returns(new Dictionary<string, IProperty> 
            {
                { "Id", mockProperty.Object }
            });
            
            var compiled = engine.Compile(templateContent);
            var data = new Dictionary<string, object>
            {
                { "entity", mockEntity.Object }
            };
            
            var result = engine.Render(compiled, data);

            // Assert
            result.Should().Contain("Entity: TestEntity");
            result.Should().Contain("Properties: 1");
            result.Should().Contain("Name: Id");
            result.Should().Contain("Is nullable: No");
        }
    }
}
