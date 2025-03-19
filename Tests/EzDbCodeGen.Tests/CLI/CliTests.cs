using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Classes;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Interfaces;
using Xunit;
using Xunit.Abstractions;
using Moq;
using EzDbSchema.Core.Interfaces;
using EzDbCodeGen.Core.Extensions;
using Newtonsoft.Json;

namespace EzDbCodeGen.Tests.CLI
{
    public class CliTests : IDisposable
    {
        private readonly string _testRootPath;
        private readonly string _testConfigPath;
        private readonly string _testTemplatesPath;
        private readonly string _testOutputPath;
        private readonly TextWriter _originalOutput;
        private StreamWriter _logWriter;
        private Configuration _config;

        public CliTests()
        {
            _testRootPath = Path.Combine(Path.GetTempPath(), "EzDbCodeGen.Tests");
            _testConfigPath = Path.Combine(_testRootPath, "Config");
            _testTemplatesPath = Path.Combine(_testRootPath, "Templates");
            _testOutputPath = Path.Combine(_testRootPath, "Output");

            // Clean up and recreate test directories
            if (Directory.Exists(_testRootPath))
            {
                Directory.Delete(_testRootPath, true);
            }
            Directory.CreateDirectory(_testConfigPath);
            Directory.CreateDirectory(_testTemplatesPath);
            Directory.CreateDirectory(_testOutputPath);

            // Save original console output
            _originalOutput = Console.Out;

            // Create log writer
            string logFile = Path.Combine(_testRootPath, $"test_run_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            _logWriter = new StreamWriter(logFile, true);
            Console.SetOut(_logWriter);

            // Create test template files
            CreateTestTemplates();

            // Set up configuration
            SetupConfiguration();
        }

        public void Dispose()
        {
            // Restore original console output
            Console.SetOut(_originalOutput);

            // Close and dispose log writer
            if (_logWriter != null)
            {
                _logWriter.Flush();
                _logWriter.Dispose();
            }

            // Clean up test directories
            try
            {
                if (Directory.Exists(_testOutputPath))
                    Directory.Delete(_testOutputPath, true);
                if (Directory.Exists(_testConfigPath))
                    Directory.Delete(_testConfigPath, true);
                if (Directory.Exists(_testTemplatesPath))
                    Directory.Delete(_testTemplatesPath, true);
            }
            catch (IOException)
            {
                // Ignore cleanup errors
            }
        }

        private void Log(string message)
        {
            var logFile = Path.Combine(_testRootPath, $"test_log_{DateTime.Now:yyyyMMdd}.txt");
            File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            _logWriter.WriteLine(message);
            _logWriter.Flush();
            _originalOutput.WriteLine(message);
        }

        private void CreateTestTemplates()
        {
            // Create model template with proper entity name and properties
            var modelTemplate = @"
public class {{EntityName}}
{
    {{#each Properties}}
    public {{NetType}} {{Name}} { get; set; }
    {{/each}}
}";
            File.WriteAllText(Path.Combine(_testTemplatesPath, "TestModelTemplate.hbs"), modelTemplate);

            // Create controller template with security and debug support
            var controllerTemplate = @"
{{#if EntitySecurity.Secured}}[Authorize]{{/if}}
public class {{EntityName}}Controller : ControllerBase
{
    private readonly ILogger<{{EntityName}}Controller> _logger;
    
    public {{EntityName}}Controller(ILogger<{{EntityName}}Controller> logger)
    {
        _logger = logger;
    }

    {{debug ""Controller template context""}}

    [HttpGet]
    public async Task<ActionResult<List<{{EntityName}}>>> GetAll()
    {
        _logger.LogInformation(""Getting all {{EntityName}}"");
        return Ok(new List<{{EntityName}}>());
    }
}";
            File.WriteAllText(Path.Combine(_testTemplatesPath, "TestControllerTemplate.hbs"), controllerTemplate);
        }

        private void SetupConfiguration()
        {
            var configPath = Path.Combine(_testConfigPath, "test-config.json");
            
            // Create base configuration with required properties
            _config = new Configuration
            {
                SourceFileName = configPath,
                OutputPath = _testOutputPath
            };
            
            // Set required values using SetConfigValue for consistency
            _config.SetConfigValue("ConnectionString", "Server=localhost;Database=TestDb;User Id=sa;Password=yourStrong(!)Password;");
            _config.SetConfigValue("Templates", new[] { _testTemplatesPath });
            _config.SetConfigValue("UseInMemoryStorage", true);
            _config.SetConfigValue("Database", new Database { DefaultSchema = "dbo", SchemaName = "dbo" });

            // Set DataTypeMap using proper type from Core.Classes
            var dataTypeMap = new List<EzDbCodeGen.Core.Classes.DataTypeMap>
            {
                new() { DataType = "nvarchar", TargetDataType = "string" },
                new() { DataType = "int", TargetDataType = "int" },
                new() { DataType = "datetime", TargetDataType = "DateTime" }
            };
            _config.SetConfigValue("DataTypeMap", dataTypeMap);
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(_testConfigPath);
            
            // Save configuration
            _config.SaveToFile(configPath);
            
            Log($"Created configuration file at: {configPath}");
        }

        [Fact]
        public void CodeGenerator_ProcessModelTemplate_GeneratesCorrectOutput()
        {
            // Arrange
            Log("Setting up test...");
            var mockDatabaseSchema = MockDatabaseSchema.CreateMockDatabase();
            var codeGenerator = new TestableCodeGenerator
            {
                Configuration = _config,
                OutputPath = _testOutputPath,
                Schema = mockDatabaseSchema
            };
            
            var templateDataInput = new TemplateDataInput(mockDatabaseSchema);
            var templatePath = Path.Combine(_testTemplatesPath, "TestModelTemplate.hbs");
            
            // Act
            Log("Processing template...");
            var result = codeGenerator.ProcessTemplate(templatePath, templateDataInput, _testOutputPath);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReturnCode.OkNoAddDels, result.Result);
            
            // Check the written files for content
            Assert.True(codeGenerator.WrittenFiles.Count > 0);
            var generatedContent = string.Join(Environment.NewLine, codeGenerator.WrittenFiles.Values);
            Assert.Contains("public class TestEntity", generatedContent);
            Log("Test completed successfully.");
        }

        [Fact]
        public void CodeGenerator_ProcessControllerTemplate_GeneratesCorrectOutput()
        {
            // Arrange
            var mockDatabaseSchema = MockDatabaseSchema.CreateMockDatabase();
            var codeGenerator = new TestableCodeGenerator
            {
                Configuration = _config,
                OutputPath = _testOutputPath,
                Schema = mockDatabaseSchema
            };
            
            // Add entity security configuration
            var entitySecurity = new Dictionary<string, EntitySecurity>
            {
                { "TestEntity", new EntitySecurity { Secured = true } }
            };
            _config.SetConfigValue("EntitySecurity", entitySecurity);
            
            var templateDataInput = new TemplateDataInput(mockDatabaseSchema);
            var templatePath = Path.Combine(_testTemplatesPath, "TestControllerTemplate.hbs");
            
            // Act
            var result = codeGenerator.ProcessTemplate(templatePath, templateDataInput, _testOutputPath);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReturnCode.OkNoAddDels, result.Result);
            
            // Check the written files for content
            Assert.True(codeGenerator.WrittenFiles.Count > 0);
            var generatedContent = string.Join(Environment.NewLine, codeGenerator.WrittenFiles.Values);
            Assert.Contains("[Authorize]", generatedContent);
            Assert.Contains("public class TestEntityController", generatedContent);
        }

        [Fact]
        public void CodeGenerator_WithEntitySecurity_RespectsSecuritySettings()
        {
            // Arrange
            var mockDatabaseSchema = MockDatabaseSchema.CreateMockDatabase();
            
            // Set up entity security to exclude Customer entity
            var entitySecurity = new Dictionary<string, EntitySecurity>
            {
                { "Customer", new EntitySecurity { Secured = true } },
                { "Order", new EntitySecurity { Secured = false } }
            };
            _config.SetConfigValue("EntitySecurity", entitySecurity);
            
            var codeGenerator = new TestableCodeGenerator
            {
                Configuration = _config,
                OutputPath = _testOutputPath,
                Schema = mockDatabaseSchema
            };
            
            var templateDataInput = new TemplateDataInput(mockDatabaseSchema);
            var templatePath = Path.Combine(_testTemplatesPath, "TestModelTemplate.hbs");
            
            // Act
            var result = codeGenerator.ProcessTemplate(templatePath, templateDataInput, _testOutputPath);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReturnCode.OkNoAddDels, result.Result);
            
            // Check the written files for content
            Assert.True(codeGenerator.WrittenFiles.Count > 0);
            var generatedContent = string.Join(Environment.NewLine, codeGenerator.WrittenFiles.Values);
            Assert.Contains("[Authorize]", generatedContent);
            Assert.DoesNotContain("public class Order", generatedContent);
        }

        [Fact]
        public void CodeGenerator_WithDebugMode_GeneratesDebugFiles()
        {
            // Arrange
            var mockDatabaseSchema = MockDatabaseSchema.CreateMockDatabase();
            var codeGenerator = new TestableCodeGenerator
            {
                Configuration = _config,
                OutputPath = _testOutputPath,
                Schema = mockDatabaseSchema,
                DebugMode = true
            };
            
            // Enable debug mode
            _config.SetConfigValue("Debug", true);
            
            var templateDataInput = new TemplateDataInput(mockDatabaseSchema);
            var templatePath = Path.Combine(_testTemplatesPath, "TestModelTemplate.hbs");
            
            // Act
            var result = codeGenerator.ProcessTemplate(templatePath, templateDataInput, _testOutputPath);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReturnCode.OkNoAddDels, result.Result);
            
            // Check the written files for content
            Assert.True(codeGenerator.WrittenFiles.Count > 0);
            var debugContent = string.Join(Environment.NewLine, codeGenerator.WrittenFiles.Values);
            Assert.Contains("Debug information", debugContent);
        }
    }
}
