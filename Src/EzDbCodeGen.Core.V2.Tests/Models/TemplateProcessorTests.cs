using System;
using System.Collections.Generic;
using FluentAssertions;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbCodeGen.Core.V2.Tests.TestCompat;
using Moq;
using Xunit;

namespace EzDbCodeGen.Core.V2.Tests.Models
{
    public class TemplateProcessorTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            
            // Act
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            
            // Assert
            processor.TemplateEngine.Should().BeSameAs(mockTemplateEngine.Object);
        }
        
        [Fact]
        public void Constructor_WithNullTemplateEngine_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            var mockFileSystem = new MockFileSystem();
            Action act = () => new TemplateProcessor(null!, mockFileSystem);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("templateEngine");
        }
        
        [Fact]
        public void Process_ShouldCompileAndRenderTemplate()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var templateContent = "Hello {{name}}!";
            var expectedOutput = "Hello World!";
            
            mockTemplateEngine
                .Setup(te => te.Compile(templateContent, It.IsAny<IDictionary<string, object>>()))
                .Returns(expectedOutput);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "output.txt",
                "Test");
                
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };
            
            // Act
            var result = processor.Process(templateModel, data);
            
            // Assert
            result.Should().Be(expectedOutput);
            mockTemplateEngine.Verify(te => te.Compile(templateContent, It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
        
        [Fact]
        public void Process_WithNullTemplate_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var data = new Dictionary<string, object>();
            
            // Act & Assert
            Action act = () => processor.Process(null!, data);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("template");
        }
        
        [Fact]
        public void Process_WithNullData_ShouldUseEmptyDictionary()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var templateContent = "Hello Template!";
            var expectedOutput = "Hello Template!";
            
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(expectedOutput);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "output.txt",
                "Test");
            
            // Act
            var result = processor.Process(templateModel, null);
            
            // Assert
            result.Should().Be(expectedOutput);
            mockTemplateEngine.Verify(te => te.Compile(templateContent, It.IsAny<IDictionary<string, object>>()), Times.Once);
        }
        
        [Fact]
        public void Process_ShouldMergeTemplateVariablesWithData()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var templateContent = "Hello {{name}}! Welcome to {{company}}!";
            var expectedOutput = "Hello John! Welcome to Contoso!";
            
            // Capture the data passed to Compile
            IDictionary<string, object> capturedData = null!;
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Callback<string, IDictionary<string, object>>((template, data) => capturedData = data)
                .Returns(expectedOutput);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "output.txt",
                "Test");
            
            // Add template variables
            templateModel.AddTemplateData("company", "Contoso");
            
            var data = new Dictionary<string, object>
            {
                { "name", "John" }
            };
            
            // Act
            var result = processor.Process(templateModel, data);
            
            // Assert
            result.Should().Be(expectedOutput);
            capturedData.Should().ContainKey("name");
            capturedData["name"].Should().Be("John");
            capturedData.Should().ContainKey("company");
            capturedData["company"].Should().Be("Contoso");
        }
        
        [Fact]
        public void Process_WithDataOverridingTemplateVariables_ShouldUseDataValue()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var templateContent = "Hello {{name}}! Welcome to {{company}}!";
            var expectedOutput = "Hello John! Welcome to Microsoft!";
            
            // Capture the data passed to Compile
            IDictionary<string, object> capturedData = null!;
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Callback<string, IDictionary<string, object>>((template, data) => capturedData = data)
                .Returns(expectedOutput);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "output.txt",
                "Test");
            
            // Add template variables
            templateModel.AddTemplateData("company", "Contoso");
            templateModel.AddTemplateData("name", "Jane");
            
            var data = new Dictionary<string, object>
            {
                { "name", "John" },
                { "company", "Microsoft" }
            };
            
            // Act
            var result = processor.Process(templateModel, data);
            
            // Assert
            result.Should().Be(expectedOutput);
            capturedData.Should().ContainKey("name");
            capturedData["name"].Should().Be("John"); // Data value overrides template value
            capturedData.Should().ContainKey("company");
            capturedData["company"].Should().Be("Microsoft"); // Data value overrides template value
        }
        
        [Fact]
        public void Process_WithCompilationError_ShouldThrowException()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var templateContent = "Hello {{name}!"; // Missing closing brace
            var compilationException = new Exception("Invalid template syntax");
            
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(compilationException);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "output.txt",
                "Test");
            
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };
            
            // Act & Assert
            Action act = () => processor.Process(templateModel, data);
            act.Should().Throw<Exception>().Which.Message.Should().Be("Invalid template syntax");
        }
        
        [Fact]
        public void ProcessToFile_ShouldGenerateFileWithRenderedContent()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            
            var templateContent = "Hello {{name}}!";
            var expectedOutput = "Hello World!";
            var outputPath = "/output/file.txt";
            
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(expectedOutput);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "file.txt",
                "Test");
                
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };
            
            // Act
            processor.ProcessToFile(templateModel, data, outputPath, mockFileSystem);
            
            // Assert
            mockFileSystem.FileExists(outputPath).Should().BeTrue();
            mockFileSystem.ReadAllText(outputPath).Should().Be(expectedOutput);
        }
        
        [Fact]
        public void ProcessToFile_WithNullTemplate_ShouldThrowArgumentNullException()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var data = new Dictionary<string, object>();
            
            // Act & Assert
            Action act = () => processor.ProcessToFile(null!, data, "/output/file.txt", mockFileSystem);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("template");
        }
        
        [Fact]
        public void ProcessToFile_WithInvalidOutputPath_ShouldThrowArgumentException()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                "content",
                "template.hbs",
                "file.txt",
                "Test");
            var data = new Dictionary<string, object>();
            
            string? invalidPath = null;
            
            // Act & Assert
            Action act = () => processor.ProcessToFile(templateModel, data, invalidPath!, mockFileSystem);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("outputPath");
        }
        
        [Fact]
        public void ProcessToFile_WithEmptyRenderedContent_ShouldNotWriteFile()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            
            var templateContent = "Hello {{name}}!";
            var emptyContent = string.Empty;
            var outputPath = "/output/file.txt";
            
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(emptyContent);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "file.txt",
                "Test");
            
            // Set skipWhenOutputIsEmpty flag
            templateModel.SetSkipWhenOutputIsEmpty(true);
                
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };
            
            // Act
            processor.ProcessToFile(templateModel, data, outputPath, mockFileSystem);
            
            // Assert
            mockFileSystem.FileExists(outputPath).Should().BeFalse();
        }
        
        [Fact]
        public void ProcessToFile_WithExistingFileAndSkipFlag_ShouldNotOverwriteFile()
        {
            // Arrange
            var mockTemplateEngine = new Mock<ITemplateEngine>();
            var mockFileSystem = new MockFileSystem();
            
            var templateContent = "Hello {{name}}!";
            var originalContent = "Original content";
            var newContent = "Hello World!";
            var outputPath = "/output/file.txt";
            
            // Add the file to mock file system
            mockFileSystem.AddMockFile(outputPath, originalContent);
            
            mockTemplateEngine
                .Setup(te => te.Compile(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(newContent);
            
            var processor = new TemplateProcessor(mockTemplateEngine.Object, mockFileSystem);
            var templateModel = TemplateCompat.CreateLegacyTemplate(
                "TestTemplate",
                templateContent,
                "template.hbs",
                "file.txt",
                "Test");
            
            // Set skipWhenFileExists flag
            templateModel.SetSkipWhenFileExists(true);
                
            var data = new Dictionary<string, object>
            {
                { "name", "World" }
            };
            
            // Act
            processor.ProcessToFile(templateModel, data, outputPath, mockFileSystem);
            
            // Assert
            mockFileSystem.FileExists(outputPath).Should().BeTrue();
            mockFileSystem.ReadAllText(outputPath).Should().Be(originalContent); // Should not be overwritten
        }
    }
}
