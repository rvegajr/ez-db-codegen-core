using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Handlebars;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Extentions;
using Xunit;

namespace EzDbCodeGen.Tests.EFCore
{
    public class EFCoreModelGenerationTests
    {
        private readonly string _outputPath;
        private readonly string _templatePath;
        private readonly Configuration _configuration;

        public EFCoreModelGenerationTests()
        {
            // Register Handlebars helpers
            HandlebarsExtensions.RegisterHelpers();
            
            // Setup paths with unique identifiers to avoid conflicts between test classes
            var testId = Guid.NewGuid().ToString();
            _outputPath = Path.Combine(Path.GetTempPath(), "EzDbCodeGen", "Tests", "Models", testId);
            _templatePath = Path.Combine(Path.GetTempPath(), "EzDbCodeGen", "Templates", "Models", testId);
            
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
            _configuration.SetValue("TemplatesPath", _templatePath);
            _configuration.SetValue("Namespace", "TestApp");
            _configuration.SourceFileName = Path.Combine(_outputPath, "ezdbcodegen.config.json");
            _configuration.SaveToFile(_configuration.SourceFileName);
        }
        
        private string RemoveWhitespace(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", "");
        }
        
        [Fact]
        public void GenerateEFCoreModel_ShouldCreateModelWithNavigationProperties()
        {
            // Arrange
            var mockDatabase = MockDatabaseSchema.CreateMockDatabaseWithRelationships();
            var templateDataInput = new TemplateDataInput(mockDatabase);
            var templatePath = Path.Combine(_templatePath, "EFCoreModel.hbs");
            
            // Create test code generator
            var codeGenerator = new TestCodeGenerator(_configuration);
            
            // Act
            codeGenerator.ProcessModelTemplate(templatePath, templateDataInput, _outputPath);
            
            // Assert
            var customerModelPath = Path.Combine(_outputPath, "Models", "Customer.cs");
            var orderModelPath = Path.Combine(_outputPath, "Models", "Order.cs");
            
            Assert.True(File.Exists(customerModelPath), "Customer model file should exist");
            Assert.True(File.Exists(orderModelPath), "Order model file should exist");
            
            var customerModelContent = File.ReadAllText(customerModelPath);
            var orderModelContent = File.ReadAllText(orderModelPath);
            
            Console.WriteLine("Customer Model Content:");
            Console.WriteLine(customerModelContent);
            
            Console.WriteLine("Order Model Content:");
            Console.WriteLine(orderModelContent);
            
            // Use the RemoveWhitespace method to normalize the content for comparison
            var normalizedCustomerContent = RemoveWhitespace(customerModelContent);
            var normalizedOrderContent = RemoveWhitespace(orderModelContent);
            
            Console.WriteLine("Normalized Customer Model Content:");
            Console.WriteLine(normalizedCustomerContent);
            
            Console.WriteLine("Normalized Order Model Content:");
            Console.WriteLine(normalizedOrderContent);
            
            Assert.Contains("public partial class Customer", customerModelContent);
            Assert.Contains("publicvirtualICollection<Order>Order{", normalizedCustomerContent);
            
            Assert.Contains("public partial class Order", orderModelContent);
            Assert.Contains("publicvirtualCustomerCustomer{", normalizedOrderContent);
        }
        
        [Fact]
        public void GenerateEFCoreModel_ShouldHandleNullableProperties()
        {
            // Arrange
            var mockDatabase = MockDatabaseSchema.CreateMockDatabaseWithNullableProperties();
            var templateDataInput = new TemplateDataInput(mockDatabase);
            var templatePath = Path.Combine(_templatePath, "EFCoreModel.hbs");
            
            // Create test code generator
            var codeGenerator = new TestCodeGenerator(_configuration);
            
            // Act
            codeGenerator.ProcessModelTemplate(templatePath, templateDataInput, _outputPath);
            
            // Assert
            var customerModelPath = Path.Combine(_outputPath, "Models", "Customer.cs");
            Assert.True(File.Exists(customerModelPath), "Customer model file should exist");
            
            var customerModelContent = File.ReadAllText(customerModelPath);
            Console.WriteLine("Customer Model Content (Nullable Properties):");
            Console.WriteLine(customerModelContent);
            
            // Use the RemoveWhitespace method to normalize the content for comparison
            var normalizedCustomerContent = RemoveWhitespace(customerModelContent);
            Console.WriteLine("Normalized Customer Model Content (Nullable Properties):");
            Console.WriteLine(normalizedCustomerContent);
            
            Assert.Contains("publicstring?CustomerEmail{get;set;}", normalizedCustomerContent);
        }
    }
}
