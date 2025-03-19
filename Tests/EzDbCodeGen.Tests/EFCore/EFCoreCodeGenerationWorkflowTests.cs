using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Extentions;
using Moq;
using Xunit;

namespace EzDbCodeGen.Tests.EFCore
{
    public class EFCoreCodeGenerationWorkflowTests : IDisposable
    {
        private readonly string _outputPath;
        private readonly string _templatePath;
        private readonly Configuration _configuration;
        private readonly IDatabase _mockDatabase;

        public EFCoreCodeGenerationWorkflowTests()
        {
            // Register Handlebars helpers
            HandlebarsExtensions.RegisterHelpers();
            
            // Create mock database
            _mockDatabase = MockDatabaseSchema.CreateMockDatabase();
            
            // Setup paths with unique identifiers to avoid conflicts between test classes
            var testId = Guid.NewGuid().ToString();
            _outputPath = Path.Combine(Path.GetTempPath(), "EzDbCodeGen", "Tests", "Workflow", testId);
            _templatePath = Path.Combine(Path.GetTempPath(), "EzDbCodeGen", "Templates", "Workflow", testId);
            
            // Ensure directories exist
            Directory.CreateDirectory(_outputPath);
            Directory.CreateDirectory(_templatePath);
            
            try
            {
                // First try to find templates in the test project
                var sourceTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Templates");
                
                if (!Directory.Exists(sourceTemplatePath))
                {
                    // If not found, try the main project templates
                    sourceTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Templates");
                }
                
                if (Directory.Exists(sourceTemplatePath))
                {
                    // Create template content directly instead of copying files
                    var modelTemplatePath = Path.Combine(_templatePath, "EFCoreModel.hbs");
                    var controllerTemplatePath = Path.Combine(_templatePath, "EFCoreController.hbs");
                    
                    // Write model template
                    if (!File.Exists(modelTemplatePath))
                    {
                        var modelTemplateContent = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {{Namespace}}.Models
{
    [Table(""{{TableName}}"", Schema = ""{{SchemaName}}"")]
    public partial class {{EntityName}}
    {
        public {{EntityName}}()
        {
            {{#each Relationships}}
            {{#if IsCollection}}
            {{NavigationProperty}} = new HashSet<{{RelatedEntity}}>();
            {{/if}}
            {{/each}}
        }

        {{#each Properties}}
        {{#if IsPrimaryKey}}
        [Key]
        {{/if}}
        {{#if IsNullable}}
        public {{DataType}}? {{PropertyName}} { get; set; }
        {{else}}
        public {{DataType}} {{PropertyName}} { get; set; }
        {{/if}}
        {{/each}}

        {{#each Relationships}}
        {{#if IsCollection}}
        public virtual ICollection<{{RelatedEntity}}> {{NavigationProperty}} { get; set; }
        {{else}}
        public virtual {{RelatedEntity}} {{NavigationProperty}} { get; set; }
        {{/if}}
        {{/each}}
    }
}";
                        File.WriteAllText(modelTemplatePath, modelTemplateContent);
                    }
                    
                    // Write controller template
                    if (!File.Exists(controllerTemplatePath))
                    {
                        var controllerTemplateContent = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using {{Namespace}}.Data;
using {{Namespace}}.Models;

namespace {{Namespace}}.Controllers
{
    [Route(""api/[controller]"")]
    [ApiController]
    public class {{EntityName}}Controller : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public {{EntityName}}Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/{{EntityName}}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<{{EntityName}}>>> Get{{EntityName}}s()
        {
            return await _context.{{EntityName}}s
                {{#each Relationships}}
                .Include(x => x.{{NavigationProperty}})
                {{/each}}
                .ToListAsync();
        }

        // GET: api/{{EntityName}}/5
        [HttpGet(""{id}"")]
        public async Task<ActionResult<{{EntityName}}>> Get{{EntityName}}({{PrimaryKeyType}} id)
        {
            var {{entityNameLower}} = await _context.{{EntityName}}s
                {{#each Relationships}}
                .Include(x => x.{{NavigationProperty}})
                {{/each}}
                .FirstOrDefaultAsync(x => x.{{PrimaryKeyName}} == id);

            if ({{entityNameLower}} == null)
            {
                return NotFound();
            }

            return {{entityNameLower}};
        }

        // PUT: api/{{EntityName}}/5
        [HttpPut(""{id}"")]
        public async Task<IActionResult> Put{{EntityName}}({{PrimaryKeyType}} id, {{EntityName}} {{entityNameLower}})
        {
            if (id != {{entityNameLower}}.{{PrimaryKeyName}})
            {
                return BadRequest();
            }

            _context.Entry({{entityNameLower}}).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!{{EntityName}}Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/{{EntityName}}
        [HttpPost]
        public async Task<ActionResult<{{EntityName}}>> Post{{EntityName}}({{EntityName}} {{entityNameLower}})
        {
            _context.{{EntityName}}s.Add({{entityNameLower}});
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get{{EntityName}}), new { id = {{entityNameLower}}.{{PrimaryKeyName}} }, {{entityNameLower}});
        }

        // DELETE: api/{{EntityName}}/5
        [HttpDelete(""{id}"")]
        public async Task<IActionResult> Delete{{EntityName}}({{PrimaryKeyType}} id)
        {
            var {{entityNameLower}} = await _context.{{EntityName}}s.FindAsync(id);
            if ({{entityNameLower}} == null)
            {
                return NotFound();
            }

            _context.{{EntityName}}s.Remove({{entityNameLower}});
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool {{EntityName}}Exists({{PrimaryKeyType}} id)
        {
            return _context.{{EntityName}}s.Any(e => e.{{PrimaryKeyName}} == id);
        }
    }
}";
                        File.WriteAllText(controllerTemplatePath, controllerTemplateContent);
                    }
                }
                else
                {
                    Console.WriteLine($"Source template directory not found: {sourceTemplatePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up templates: {ex.Message}");
            }
            
            // Setup configuration
            _configuration = new Configuration
            {
                OutputPath = _outputPath
            };
            _configuration.SetConfigValue("TemplatesPath", _templatePath);
            _configuration.SetConfigValue("Namespace", "TestApp");
            _configuration.SourceFileName = Path.Combine(_outputPath, "ezdbcodegen.config.json");
            _configuration.SaveToFile(_configuration.SourceFileName);
        }
        
        [Fact]
        public void GenerateEFCoreApplication_ShouldCreateModelsAndControllers()
        {
            // Arrange
            var templateDataInput = new TemplateDataInput(_mockDatabase);
            
            // Create the test code generator
            var codeGenerator = new TestCodeGenerator(_configuration);
            
            // Act - Process model template
            var modelTemplatePath = GetTemplatePath("EFCoreModel.hbs");
            codeGenerator.ProcessModelTemplate(modelTemplatePath, templateDataInput, _outputPath);
            
            // Act - Process controller template
            var controllerTemplatePath = GetTemplatePath("EFCoreController.hbs");
            codeGenerator.ProcessControllerTemplate(controllerTemplatePath, templateDataInput, _outputPath);
            
            // Assert - Check if model files were created
            var customerModelPath = Path.Combine(_outputPath, "Models", "Customer.cs");
            var orderModelPath = Path.Combine(_outputPath, "Models", "Order.cs");
            
            Assert.True(File.Exists(customerModelPath), "Customer model file should exist");
            Assert.True(File.Exists(orderModelPath), "Order model file should exist");
            
            // Assert - Check if controller files were created
            var customerControllerPath = Path.Combine(_outputPath, "Controllers", "CustomerController.cs");
            var orderControllerPath = Path.Combine(_outputPath, "Controllers", "OrderController.cs");
            
            Assert.True(File.Exists(customerControllerPath), "Customer controller file should exist");
            Assert.True(File.Exists(orderControllerPath), "Order controller file should exist");
        }

        [Fact]
        public void GenerateEFCoreApplication_ShouldHandleEntitySecurity()
        {
            // Arrange
            var entitySecurity = new Dictionary<string, TestHelpers.EntitySecurity>
            {
                { "Customer", new TestHelpers.EntitySecurity { Enabled = true } },
                { "Order", new TestHelpers.EntitySecurity { Enabled = false } }
            };
            _configuration.SetConfigValue("EntitySecurity", entitySecurity);
            
            var templateDataInput = new TemplateDataInput(_mockDatabase);
            
            // Create the test code generator
            var codeGenerator = new TestCodeGenerator(_configuration);
            
            // Act - Process model template
            var modelTemplatePath = GetTemplatePath("EFCoreModel.hbs");
            codeGenerator.ProcessModelTemplate(modelTemplatePath, templateDataInput, _outputPath);
            
            // Act - Process controller template
            var controllerTemplatePath = GetTemplatePath("EFCoreController.hbs");
            codeGenerator.ProcessControllerTemplate(controllerTemplatePath, templateDataInput, _outputPath);
            
            // Assert - Check if model files were created
            var customerModelPath = Path.Combine(_outputPath, "Models", "Customer.cs");
            var orderModelPath = Path.Combine(_outputPath, "Models", "Order.cs");
            
            Assert.False(File.Exists(customerModelPath), "Customer model file should not exist because it's secured");
            Assert.True(File.Exists(orderModelPath), "Order model file should exist");
            
            // Assert - Check if controller files were created
            var customerControllerPath = Path.Combine(_outputPath, "Controllers", "CustomerController.cs");
            var orderControllerPath = Path.Combine(_outputPath, "Controllers", "OrderController.cs");
            
            Assert.False(File.Exists(customerControllerPath), "Customer controller file should not exist because it's secured");
            Assert.True(File.Exists(orderControllerPath), "Order controller file should exist");
        }
        
        private string GetTemplatePath(string templateName)
        {
            return Path.Combine(_templatePath, templateName);
        }
        
        private string RemoveWhitespace(string input)
        {
            return Regex.Replace(input, @"\s+", "");
        }
        
        public void Dispose()
        {
            // Clean up temporary directory after tests
            if (Directory.Exists(_outputPath))
            {
                try
                {
                    Directory.Delete(_outputPath, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
    
    // Mock interface for database provider
    public interface IDatabaseProvider
    {
        IDatabase GetDatabase();
    }
    
    // Template data input implementation for tests
    public class TemplateDataInput : ITemplateDataInput
    {
        private readonly IDatabase _database;
        
        public TemplateDataInput(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            SchemaName = "dbo"; // Default schema name
            Schema = database;
        }
        
        public bool VerboseMessages { get; set; }
        public string SchemaName { get; set; }
        public IDatabase Schema { get; set; }
        
        public IDatabase LoadSchema<T>(Configuration config) where T : new()
        {
            Schema = _database;
            return Schema;
        }
        
        public IDatabase LoadSchema(Configuration config)
        {
            Schema = _database;
            return Schema;
        }
    }
}
