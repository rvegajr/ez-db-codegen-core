using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbSchema.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Tests.TestCompat;
using EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions;

namespace EzDbCodeGen.Core.V2.Tests.Models
{
    public class DefaultCodeGeneratorTests
    {
        [Fact]
        public void Constructor_WithValidDependencies_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Assert
            codeGenerator.FileSystem.Should().BeSameAs(mockFileSystem.Object);
            codeGenerator.TemplateProcessor.Should().BeSameAs(mockTemplateProcessor.Object);
            codeGenerator.SchemaAnalyzer.Should().BeNull();
            codeGenerator.Variables.Should().NotBeNull();
            codeGenerator.Variables.Should().BeEmpty();
            codeGenerator.Templates.Should().NotBeNull();
            codeGenerator.Templates.Should().BeEmpty();
        }
        
        [Fact]
        public void Constructor_WithNullFileSystem_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            // Act & Assert
            Action act = () => TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                null!,
                mockTemplateProcessor.Object,
                null);
            
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("fileSystem");
        }
        
        [Fact]
        public void Constructor_WithNullTemplateProcessor_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            // Act & Assert
            Action act = () => TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                null!,
                null);
            
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("templateProcessor");
        }
        
        [Fact]
        public void SetVariable_ShouldAddOrUpdateVariable()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act
            codeGenerator.SetVariable("key1", "value1");
            codeGenerator.SetVariable("key2", 123);
            codeGenerator.SetVariable("key1", "updated"); // Overwrite existing
            
            // Assert
            codeGenerator.Variables.Should().HaveCount(2);
            codeGenerator.Variables.Should().ContainKey("key1");
            codeGenerator.Variables.Should().ContainKey("key2");
            codeGenerator.Variables["key1"].Should().Be("updated");
            codeGenerator.Variables["key2"].Should().Be(123);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SetVariable_WithInvalidKey_ShouldThrowArgumentException(string? invalidKey)
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act & Assert
            Action act = () => codeGenerator.SetVariable(invalidKey!, "value");
            act.Should().Throw<ArgumentException>()
                .WithParameterName("key");
        }
        
        [Fact]
        public void GetVariable_ShouldReturnVariableValue()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            codeGenerator.SetVariable("key1", "value1");
            
            // Act
            var value = codeGenerator.GetVariable("key1");
            
            // Assert
            value.Should().Be("value1");
        }
        
        [Fact]
        public void GetVariable_WithNonExistentKey_ShouldReturnNull()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act
            var value = codeGenerator.GetVariable("nonexistent");
            
            // Assert
            value.Should().BeNull();
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetVariable_WithInvalidKey_ShouldThrowArgumentException(string? invalidKey)
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act & Assert
            Action act = () => codeGenerator.GetVariable(invalidKey!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("key");
        }
        
        [Fact]
        public void RegisterTemplate_ShouldAddTemplateToCollection()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            var template = new Template(
                "TestTemplate",
                "Template content",
                "test.hbs",
                "test.cs",
                "Test");
            
            // Act
            codeGenerator.RegisterTemplate(template);
            
            // Assert
            codeGenerator.Templates.Should().HaveCount(1);
            codeGenerator.Templates.Should().ContainKey("TestTemplate");
            codeGenerator.Templates["TestTemplate"].Should().BeSameAs(template);
        }
        
        [Fact]
        public void RegisterTemplate_WithDuplicateName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            var template1 = new Template(
                "TestTemplate",
                "Template content 1",
                "test1.hbs",
                "test1.cs",
                "Test");
                
            var template2 = new Template(
                "TestTemplate", // Same name
                "Template content 2",
                "test2.hbs",
                "test2.cs",
                "Test");
            
            codeGenerator.RegisterTemplate(template1);
            
            // Act & Assert
            Action act = () => codeGenerator.RegisterTemplate(template2);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*already registered*");
        }
        
        [Fact]
        public void RegisterTemplate_WithNullTemplate_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act & Assert
            Action act = () => codeGenerator.RegisterTemplate(null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("template");
        }
        
        [Fact]
        public void LoadTemplateFromFile_ShouldLoadAndRegisterTemplate()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var templateContent = "Template content from file";
            var templatePath = "/templates/test.hbs";
            
            mockFileSystem
                .Setup(fs => fs.FileExists(templatePath))
                .Returns(true);
                
            mockFileSystem
                .Setup(fs => fs.ReadAllText(templatePath))
                .Returns(templateContent);
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act
            var result = codeGenerator.LoadTemplateFromFile(
                templatePath,
                "TestTemplate",
                "test.cs",
                "Test");
            
            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestTemplate");
            result.Content.Should().Be(templateContent);
            result.TemplatePath.Should().Be(templatePath);
            result.OutputPath.Should().Be("test.cs");
            result.Category.Should().Be("Test");
            
            codeGenerator.Templates.Should().HaveCount(1);
            codeGenerator.Templates.Should().ContainKey("TestTemplate");
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void LoadTemplateFromFile_WithInvalidTemplatePath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act & Assert
            Action act = () => codeGenerator.LoadTemplateFromFile(
                invalidPath!,
                "TestTemplate",
                "test.cs",
                "Test");
                
            act.Should().Throw<ArgumentException>()
                .WithParameterName("templateFilePath");
        }
        
        [Fact]
        public void LoadTemplateFromFile_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var templatePath = "/templates/nonexistent.hbs";
            
            mockFileSystem
                .Setup(fs => fs.FileExists(templatePath))
                .Returns(false);
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
            
            // Act & Assert
            Action act = () => codeGenerator.LoadTemplateFromFile(
                templatePath,
                "TestTemplate",
                "test.cs",
                "Test");
                
            act.Should().Throw<FileNotFoundException>();
        }
    }
}
