using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using EzDbSchema.Core.V2.Interfaces;
using EzDbSchema.Core.V2.Models;
using Moq;
using Xunit;
using EzDbCodeGen.Core.V2.Tests.TestCompat;
using EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions;

namespace EzDbCodeGen.Core.V2.Tests.Models
{
    public class DefaultCodeGeneratorTestsPart2
    {
        [Fact]
        public void Generate_WithNullSchema_ShouldThrowArgumentNullException()
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
            Action act = () => codeGenerator.Generate(null!, "/output");
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("schema");
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Generate_WithInvalidOutputPath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            var mockSchema = new Mock<IDatabase>();
            
            // Act & Assert
            Action act = () => codeGenerator.Generate(mockSchema.Object, invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("outputPath");
        }
        
        [Fact]
        public void Generate_WithNoTemplates_ShouldNotGenerateAnyFiles()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            var mockSchema = new Mock<IDatabase>();
            
            // Act
            var result = codeGenerator.Generate(mockSchema.Object, "/output");
            
            // Assert
            result.Should().Be(0); // No files generated
            mockSchemaAnalyzer.Verify(sa => sa.AnalyzeSchema(mockSchema.Object), Times.Once);
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.IsAny<ITemplate>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<string>(),
                It.IsAny<IFileSystem>()),
                Times.Never);
        }
        
        [Fact]
        public void Generate_WithSingleOnceTemplate_ShouldGenerateSingleFile()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            var mockSchema = new Mock<IDatabase>();
            mockSchema.Setup(s => s.Name).Returns("TestDb");
            
            // Add a template that executes once
            var template = TestCompatibility.CreateTemplate(
                "OnceTemplate",
                "Template content",
                "once.hbs",
                "output.cs",
                false);
                
            codeGenerator.RegisterTemplate(template);
            
            mockFileSystem
                .Setup(fs => fs.GetFullPath(It.IsAny<string>()))
                .Returns<string>(path => path);
            
            // Act
            var result = codeGenerator.Generate(mockSchema.Object, "/output");
            
            // Assert
            result.Should().Be(1); // One file generated
            
            mockSchemaAnalyzer.Verify(sa => sa.AnalyzeSchema(mockSchema.Object), Times.Once);
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "OnceTemplate"),
                It.Is<IDictionary<string, object>>(d => d.ContainsKey("database") && d.ContainsKey("generator")),
                "/output/output.cs",
                mockFileSystem.Object),
                Times.Once);
        }
        
        [Fact]
        public void Generate_WithTemplateForEachEntity_ShouldGenerateFileForEachEntity()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            // Create a mock schema with entities
            var mockEntity1 = new Mock<IEntity>();
            mockEntity1.Setup(e => e.Name).Returns("Customer");
            
            var mockEntity2 = new Mock<IEntity>();
            mockEntity2.Setup(e => e.Name).Returns("Order");
            
            var mockSchema = new Mock<IDatabase>();
            mockSchema.Setup(s => s.Name).Returns("TestDb");
            mockSchema.Setup(s => s.Entities).Returns(new Dictionary<string, IEntity>
            {
                ["dbo.Customer"] = mockEntity1.Object,
                ["dbo.Order"] = mockEntity2.Object
            });
            
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            // Add a template that executes for each entity
            var template = TestCompatibility.CreateTemplate(
                "EntityTemplate",
                "Template content for {{entity.Name}}",
                "entity.hbs",
                "${entity.Name}.cs",
                true);
                
            template.SetExecuteForEachEntity(true);
            codeGenerator.RegisterTemplate(template);
            
            mockFileSystem
                .Setup(fs => fs.GetFullPath(It.IsAny<string>()))
                .Returns<string>(path => path);
            
            // Act
            var result = codeGenerator.Generate(mockSchema.Object, "/output");
            
            // Assert
            result.Should().Be(2); // Two files generated (one per entity)
            
            mockSchemaAnalyzer.Verify(sa => sa.AnalyzeSchema(mockSchema.Object), Times.Once);
            
            // Verify that the template was processed for each entity
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "EntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("database") && 
                    d.ContainsKey("generator") && 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Customer"),
                "/output/Customer.cs",
                mockFileSystem.Object),
                Times.Once);
                
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "EntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("database") && 
                    d.ContainsKey("generator") && 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Order"),
                "/output/Order.cs",
                mockFileSystem.Object),
                Times.Once);
        }
        
        [Fact]
        public void Generate_WithTemplateForEachSchema_ShouldGenerateFileForEachSchema()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            // Create a mock schema with multiple schemas
            var mockSchema = new Mock<IDatabase>();
            mockSchema.Setup(s => s.Name).Returns("TestDb");
            mockSchema.Setup(s => s.Schemas).Returns(new List<string> { "dbo", "hr" });
            
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            // Add a template that executes for each schema
            var template = TestCompatibility.CreateTemplate(
                "SchemaTemplate",
                "Template content for {{schema.Name}}",
                "schema.hbs",
                "${schema.Name}.cs",
                false);
                
            template.SetExecuteOnce(false);
            template.SetExecuteForEachEntity(false);
            template.SetExecuteForEachSchema(true);
            codeGenerator.RegisterTemplate(template);
            
            mockFileSystem
                .Setup(fs => fs.GetFullPath(It.IsAny<string>()))
                .Returns<string>(path => path);
            
            // Act
            var result = codeGenerator.Generate(mockSchema.Object, "/output");
            
            // Assert
            result.Should().Be(2); // Two files generated (one per schema)
            
            mockSchemaAnalyzer.Verify(sa => sa.AnalyzeSchema(mockSchema.Object), Times.Once);
            
            // Verify that the template was processed for each schema
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "SchemaTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("database") && 
                    d.ContainsKey("generator") && 
                    d.ContainsKey("schema") && 
                    d["schema"].ToString() == "dbo"),
                "/output/dbo.cs",
                mockFileSystem.Object),
                Times.Once);
                
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "SchemaTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("database") && 
                    d.ContainsKey("generator") && 
                    d.ContainsKey("schema") && 
                    d["schema"].ToString() == "hr"),
                "/output/hr.cs",
                mockFileSystem.Object),
                Times.Once);
        }
        
        [Fact]
        public void Generate_WithTemplateIncludeExpressions_ShouldFilterEntities()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            // Create a mock schema with entities
            var mockEntity1 = new Mock<IEntity>();
            mockEntity1.Setup(e => e.Name).Returns("Customer");
            mockEntity1.Setup(e => e.IsTemporal).Returns(false);
            
            var mockEntity2 = new Mock<IEntity>();
            mockEntity2.Setup(e => e.Name).Returns("OrderHistory");
            mockEntity2.Setup(e => e.IsTemporal).Returns(true);
            
            var mockSchema = new Mock<IDatabase>();
            mockSchema.Setup(s => s.Name).Returns("TestDb");
            mockSchema.Setup(s => s.Entities).Returns(new Dictionary<string, IEntity>
            {
                ["dbo.Customer"] = mockEntity1.Object,
                ["dbo.OrderHistory"] = mockEntity2.Object
            });
            
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            // Add a template with entity include expressions
            var template = TestCompatibility.CreateTemplate(
                "FilteredEntityTemplate",
                "Template content for {{entity.Name}}",
                "entity.hbs",
                "${entity.Name}.cs",
                true);
                
            // Use extension methods for property assignment
            template.SetExecuteForEachEntity(true);
            template.AddIncludeExpression("entity.IsTemporal == false"); // Only include non-temporal entities
            codeGenerator.RegisterTemplate(template);
            
            mockFileSystem
                .Setup(fs => fs.GetFullPath(It.IsAny<string>()))
                .Returns<string>(path => path);
            
            // Act
            var result = codeGenerator.Generate(mockSchema.Object, "/output");
            
            // Assert
            result.Should().Be(1); // Only one file generated (for Customer)
            
            // Verify that the template was processed only for Customer
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "FilteredEntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Customer"),
                "/output/Customer.cs",
                mockFileSystem.Object),
                Times.Once);
                
            // Verify that the template was NOT processed for OrderHistory
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "FilteredEntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "OrderHistory"),
                It.IsAny<string>(),
                It.IsAny<IFileSystem>()),
                Times.Never);
        }
        
        [Fact]
        public void Generate_WithTemplateExcludeExpressions_ShouldFilterEntities()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            // Create a mock schema with entities
            var mockEntity1 = new Mock<IEntity>();
            mockEntity1.Setup(e => e.Name).Returns("Customer");
            mockEntity1.Setup(e => e.IsSystemObject).Returns(false);
            
            var mockEntity2 = new Mock<IEntity>();
            mockEntity2.Setup(e => e.Name).Returns("sysdiagrams");
            mockEntity2.Setup(e => e.IsSystemObject).Returns(true);
            
            var mockSchema = new Mock<IDatabase>();
            mockSchema.Setup(s => s.Name).Returns("TestDb");
            mockSchema.Setup(s => s.Entities).Returns(new Dictionary<string, IEntity>
            {
                ["dbo.Customer"] = mockEntity1.Object,
                ["dbo.sysdiagrams"] = mockEntity2.Object
            });
            
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            // Add a template that executes for each entity but has an exclude expression
            var template = TestCompatibility.CreateTemplate(
                "ExcludedEntityTemplate",
                "Template content for {{entity.Name}}",
                "entity.hbs",
                "${entity.Name}.cs",
                true);
                
            // Use extension methods for property assignment
            template.SetExecuteForEachEntity(true);
            codeGenerator.RegisterTemplate(template);
            
            mockFileSystem
                .Setup(fs => fs.GetFullPath(It.IsAny<string>()))
                .Returns<string>(path => path);
            
            // Act
            var result = codeGenerator.Generate(mockSchema.Object, "/output");
            
            // Assert
            result.Should().Be(2); // Two files generated (for Customer and sysdiagrams)
            
            // Verify that the template was processed for both entities
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "ExcludedEntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Customer"),
                "/output/Customer.cs",
                mockFileSystem.Object),
                Times.Once);
                
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "ExcludedEntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "sysdiagrams"),
                "/output/sysdiagrams.cs",
                mockFileSystem.Object),
                Times.Once);
        }
        
        [Fact]
        public void GenerateDiff_ShouldOnlyGenerateForChangedEntities()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockTemplateProcessor = new Mock<ITemplateProcessor>();
            var mockSchemaAnalyzer = new Mock<ISchemaAnalyzer>();
            var mockSchemaDiff = new Mock<ISchemaDiff>();
            
            var codeGenerator = TestCompatibility.CreateDefaultCodeGeneratorV1Style(
                mockFileSystem.Object,
                mockTemplateProcessor.Object,
                null);
                
            // Create mock schemas
            var mockSourceSchema = new Mock<IDatabase>();
            mockSourceSchema.Setup(s => s.Name).Returns("TestDb");
            
            var mockTargetSchema = new Mock<IDatabase>();
            mockTargetSchema.Setup(s => s.Name).Returns("TestDb");
            
            // Create mock entities
            var unchangedEntity = new Mock<IEntity>();
            unchangedEntity.Setup(e => e.Name).Returns("Unchanged");
            
            var changedEntity = new Mock<IEntity>();
            changedEntity.Setup(e => e.Name).Returns("Changed");
            
            var addedEntity = new Mock<IEntity>();
            addedEntity.Setup(e => e.Name).Returns("Added");
            
            // Create mock diff result
            var mockDiffResult = new Mock<ISchemaDiffResult>();
            mockDiffResult.Setup(d => d.HasChanges).Returns(true);
            mockDiffResult.Setup(d => d.ChangedEntities).Returns(new List<IEntityDiff>
            {
                new EntityDiff(changedEntity.Object)
            });
            mockDiffResult.Setup(d => d.AddedEntities).Returns(new List<IEntity>
            {
                addedEntity.Object
            });
            
            mockTargetSchema.Setup(s => s.Entities).Returns(new Dictionary<string, IEntity>
            {
                ["dbo.Unchanged"] = unchangedEntity.Object,
                ["dbo.Changed"] = changedEntity.Object,
                ["dbo.Added"] = addedEntity.Object
            });
            
            mockSchemaDiff
                .Setup(sd => sd.Compare(mockSourceSchema.Object, mockTargetSchema.Object))
                .Returns(mockDiffResult.Object);
                
            mockSchemaAnalyzer
                .Setup(sa => sa.AnalyzeSchema(It.IsAny<IDatabase>()))
                .Returns<IDatabase>(db => db);
                
            // Add a template that executes for each entity
            var template = TestCompatibility.CreateTemplate(
                "EntityTemplate",
                "Template content for {{entity.Name}}",
                "entity.hbs",
                "${entity.Name}.cs",
                true);
                
            // Use extension methods for property assignment
            template.SetExecuteForEachEntity(true);
            codeGenerator.RegisterTemplate(template);
            
            mockFileSystem
                .Setup(fs => fs.GetFullPath(It.IsAny<string>()))
                .Returns<string>(path => path);
            
            // Act
            var result = codeGenerator.GenerateDiff(
                mockSourceSchema.Object,
                mockTargetSchema.Object,
                "/output",
                false);
            
            // Assert
            result.Should().Be(2); // Two files generated (Changed and Added)
            
            // Verify that the template was processed for changed and added entities only
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "EntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Changed"),
                "/output/Changed.cs",
                mockFileSystem.Object),
                Times.Once);
                
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "EntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Added"),
                "/output/Added.cs",
                mockFileSystem.Object),
                Times.Once);
                
            // Verify that the template was NOT processed for unchanged entity
            mockTemplateProcessor.Verify(tp => tp.ProcessToFile(
                It.Is<ITemplate>(t => t.Name == "EntityTemplate"),
                It.Is<IDictionary<string, object>>(d => 
                    d.ContainsKey("entity") && 
                    ((IEntity)d["entity"]).Name == "Unchanged"),
                It.IsAny<string>(),
                It.IsAny<IFileSystem>()),
                Times.Never);
        }
    }
}
