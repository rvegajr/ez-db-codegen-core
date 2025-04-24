using System;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using FluentAssertions;
using Xunit;
using EzDbCodeGen.Core.V2.Tests.TestCompat;
using EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions;

namespace EzDbCodeGen.Core.V2.Tests.Models
{
    public class TemplateTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldSetProperties()
        {
            // Arrange & Act
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Template content",
                "test.hbs",
                "output.txt",
                false);

            // Assert
            template.Name.Should().Be("TestTemplate");
            template.Content().Should().Be("Template content");
            template.TemplatePath.Should().Be("test.hbs");
            template.OutputPath.Should().Be("output.txt");
            template.Category().Should().Be("Default");
            template.SkipWhenFileExists().Should().BeFalse();
            template.EntityGenerator.Should().BeFalse();
            template.GetExecuteOnce().Should().BeTrue();
            template.GetExecuteForEachEntity().Should().BeFalse();
            template.GetExecuteForEachSchema().Should().BeFalse();
            template.ProcessAsPartial().Should().BeFalse();
            template.IncludeExpressions.Should().NotBeNull().And.BeEmpty();
            template.ExcludeExpressions.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Constructor_WithInvalidName_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Action act = () => TestCompatibility.CreateTemplate(
                null!,
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            act.Should().Throw<ArgumentException>()
                .WithParameterName("name");
        }

        [Fact]
        public void Constructor_WithInvalidContent_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Action act = () => TestCompatibility.CreateTemplate(
                "TestTemplate",
                null!,
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            act.Should().Throw<ArgumentException>()
                .WithParameterName("content");
        }

        [Fact]
        public void Constructor_WithInvalidTemplatePath_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Action act = () => TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                null!,
                "${entity.Name}.cs",
                "Models");

            act.Should().Throw<ArgumentException>()
                .WithParameterName("templatePath");
        }

        [Fact]
        public void Constructor_WithInvalidOutputPath_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Action act = () => TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                null!,
                "Models");

            act.Should().Throw<ArgumentException>()
                .WithParameterName("outputPath");
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldDefaultToUncategorized()
        {
            // Arrange & Act
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                null!);

            // Assert
            template.Category.Should().Be("Uncategorized");
        }

        [Fact]
        public void Constructor_WithEmptyCategory_ShouldDefaultToUncategorized()
        {
            // Arrange & Act
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "");

            // Assert
            template.Category.Should().Be("Uncategorized");
        }

        [Fact]
        public void SetVariable_ShouldAddOrUpdateVariable()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act
            template.SetVariable("key1", "value1");
            template.SetVariable("key2", 123);
            template.SetVariable("key1", "updated"); // Overwrite existing

            // Assert
            template.Variables.Should().HaveCount(2);
            template.Variables.Should().ContainKey("key1");
            template.Variables.Should().ContainKey("key2");
            template.Variables["key1"].Should().Be("updated");
            template.Variables["key2"].Should().Be(123);
        }

        [Fact]
        public void SetVariable_WithInvalidKey_ShouldThrowArgumentException()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act & Assert
            Action act = () => template.SetVariable(null!, "value");
            act.Should().Throw<ArgumentException>()
                .WithParameterName("key");
        }

        [Fact]
        public void SetVariable_WithNullValue_ShouldRemoveVariable()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");
            template.SetVariable("key1", "value1");

            // Act
            template.SetVariable("key1", null);

            // Assert
            template.Variables.Should().BeEmpty();
        }

        [Fact]
        public void GetVariable_ShouldReturnVariableValue()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");
            template.SetVariable("key1", "value1");

            // Act
            var value = template.GetVariable("key1");

            // Assert
            value.Should().Be("value1");
        }

        [Fact]
        public void GetVariable_WithNonExistentKey_ShouldReturnNull()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act
            var value = template.GetVariable("nonexistent");

            // Assert
            value.Should().BeNull();
        }

        [Fact]
        public void GetVariable_WithInvalidKey_ShouldThrowArgumentException()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act & Assert
            Action act = () => template.GetVariable(null!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("key");
        }

        [Fact]
        public void AddIncludeExpression_ShouldAddExpressionToList()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act
            template.AddIncludeExpression("entity.Name.StartsWith('Test')");

            // Assert
            template.IncludeExpressions.Should().HaveCount(1);
            template.IncludeExpressions.Should().Contain("entity.Name.StartsWith('Test')");
        }

        [Fact]
        public void AddIncludeExpression_WithInvalidExpression_ShouldThrowArgumentException()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act & Assert
            Action act = () => template.AddIncludeExpression(null!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("expression");
        }

        [Fact]
        public void AddExcludeExpression_ShouldAddExpressionToList()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act
            template.AddExcludeExpression("entity.Name.StartsWith('Test')");

            // Assert
            template.ExcludeExpressions.Should().HaveCount(1);
            template.ExcludeExpressions.Should().Contain("entity.Name.StartsWith('Test')");
        }

        [Fact]
        public void AddExcludeExpression_WithInvalidExpression_ShouldThrowArgumentException()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act & Assert
            Action act = () => template.AddExcludeExpression(null!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("expression");
        }

        [Fact]
        public void AddBeforeRenderAction_ShouldAddActionToList()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act
            template.AddBeforeRenderAction("LogTemplateProcessing");

            // Assert
            template.BeforeRender.Should().HaveCount(1);
            template.BeforeRender.Should().Contain("LogTemplateProcessing");
        }

        [Fact]
        public void AddBeforeRenderAction_WithInvalidAction_ShouldThrowArgumentException()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act & Assert
            Action act = () => template.AddBeforeRenderAction(null!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("action");
        }

        [Fact]
        public void AddAfterRenderAction_ShouldAddActionToList()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act
            template.AddAfterRenderAction("FormatGeneratedCode");

            // Assert
            template.AfterRender.Should().HaveCount(1);
            template.AfterRender.Should().Contain("FormatGeneratedCode");
        }

        [Fact]
        public void AddAfterRenderAction_WithInvalidAction_ShouldThrowArgumentException()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");

            // Act & Assert
            Action act = () => template.AddAfterRenderAction(null!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("action");
        }

        [Fact]
        public void LoadFromFile_ShouldLoadTemplateFromFile()
        {
            // Arrange
            var templateFilePath = "Model.cs.hbs";
            var templateContent = "template content from file";
            var name = "ModelTemplate";
            var outputPath = "${entity.Name}.cs";
            var category = "Models";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.FileExists(templateFilePath)).Returns(true);
            mockFileSystem.Setup(fs => fs.ReadAllText(templateFilePath)).Returns(templateContent);

            // Act
            var template = TestCompatibility.LoadTemplateFromFile(
                mockFileSystem.Object,
                templateFilePath,
                name,
                outputPath,
                category);

            // Assert
            template.Should().NotBeNull();
            template.Name.Should().Be(name);
            template.Content.Should().Be(templateContent);
            template.TemplatePath.Should().Be(templateFilePath);
            template.OutputPath.Should().Be(outputPath);
            template.Category.Should().Be(category);
            template.Variables.Should().NotBeNull();
            template.Variables.Should().BeEmpty();
            template.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            template.LastModifiedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void LoadFromFile_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentFile = "nonexistent.hbs";

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.FileExists(nonExistentFile)).Returns(false);

            // Act & Assert
            Action act = () => TestCompatibility.LoadTemplateFromFile(mockFileSystem.Object, nonExistentFile, "ModelTemplate", "${entity.Name}.cs", "Models");
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void LoadFromFile_WithInvalidTemplateFilePath_ShouldThrowArgumentException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();

            // Act & Assert
            Action act = () => TestCompatibility.LoadTemplateFromFile(mockFileSystem.Object, null!, "ModelTemplate", "${entity.Name}.cs", "Models");
            act.Should().Throw<ArgumentException>()
                .WithParameterName("templateFilePath");
        }

        [Fact]
        public void LoadFromFile_WithNullFileSystem_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Action act = () => TestCompatibility.LoadTemplateFromFile(null!, "Model.cs.hbs", "ModelTemplate", "${entity.Name}.cs", "Models");
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("fileSystem");
        }

        [Fact]
        public void ToJson_ShouldSerializeTemplateCorrectly()
        {
            // Arrange
            var template = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");
            template.SetVariable("key1", "value1");
            template.AddIncludeExpression("entity.Name.StartsWith('Test')");
            template.ExecuteForEachEntity = true;

            // Act
            string json = template.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("TestTemplate");
            json.Should().Contain("Content of template");
            json.Should().Contain("Model.cs.hbs");
            json.Should().Contain("${entity.Name}.cs");
            json.Should().Contain("Models");
            json.Should().Contain("key1");
            json.Should().Contain("value1");
            json.Should().Contain("entity.Name.StartsWith('Test')");
            json.Should().Contain("true"); // ExecuteForEachEntity
        }

        [Fact]
        public void Clone_ShouldCreateDeepCopyOfTemplate()
        {
            // Arrange
            var original = TestCompatibility.CreateTemplate(
                "TestTemplate",
                "Content of template",
                "Model.cs.hbs",
                "${entity.Name}.cs",
                "Models");
            original.SetVariable("key1", "value1");
            original.AddIncludeExpression("entity.Name.StartsWith('Test')");
            original.ExecuteForEachEntity = true;

            // Act
            var clone = (Template)original.Clone();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Name.Should().Be(original.Name);
            clone.Content.Should().Be(original.Content);
            clone.TemplatePath.Should().Be(original.TemplatePath);
            clone.OutputPath.Should().Be(original.OutputPath);
            clone.Category.Should().Be(original.Category);
            clone.ExecuteForEachEntity.Should().Be(original.ExecuteForEachEntity);
            
            // Variables should be deep copied
            clone.Variables.Should().NotBeSameAs(original.Variables);
            clone.Variables.Should().BeEquivalentTo(original.Variables);
            
            // Changing the clone should not affect the original
            clone.SetVariable("key1", "modified");
            clone.Variables["key1"].Should().Be("modified");
            original.Variables["key1"].Should().Be("value1");
        }
    }
}
