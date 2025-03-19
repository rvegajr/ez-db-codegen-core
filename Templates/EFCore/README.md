# EF Core Templates

These templates are designed to generate Entity Framework Core models and controllers from your database schema.

## Templates Included

1. **EFCoreModel.hbs**: Generates EF Core model classes with navigation properties based on relationships
2. **EFCoreController.hbs**: Generates API controllers for your EF Core models

## Usage

### Basic Usage

```csharp
// Create a configuration
var configuration = new Configuration();
configuration.SetValue("Namespace", "MyApp");

// Create a database schema (using EzDbSchema or a mock)
var database = GetDatabaseSchema(); // Your method to get the schema
var templateDataInput = new TemplateDataInput(database);

// Create the code generator
var codeGenerator = new CodeGenerator(configuration);

// Process templates
var modelTemplatePath = "Templates/EFCore/EFCoreModel.hbs";
codeGenerator.ProcessModelTemplate(modelTemplatePath, templateDataInput, "./Generated");

var controllerTemplatePath = "Templates/EFCore/EFCoreController.hbs";
codeGenerator.ProcessControllerTemplate(controllerTemplatePath, templateDataInput, "./Generated");
```

### Entity Security

You can control which entities are included in code generation using entity security:

```csharp
// Set up entity security
var entitySecurity = new Dictionary<string, EntitySecurity>
{
    { "Customer", new EntitySecurity { Secured = true } }, // Skip Customer entity
    { "Order", new EntitySecurity { Secured = false } }    // Include Order entity
};
configuration.SetValue("EntitySecurity", entitySecurity);
```

### Template Customization

You can customize these templates to fit your specific needs. The templates use Handlebars syntax and have access to the following data:

- `Namespace`: The namespace for generated code
- `TableName`: The name of the database table
- `EntityName`: The normalized entity name
- `SchemaName`: The database schema name
- `PrimaryKeyName`: The name of the primary key
- `Properties`: Collection of entity properties
- `Relationships`: Collection of entity relationships
- `Entity`: The entity object
- `Database`: The database object

## Debugging

The code generator creates debug output files with the prefix `DEBUG_` that can be useful for troubleshooting. Template data is also saved as JSON files with the prefix `DEBUG_DATA_` for inspecting the data passed to templates.

## Example Output

### Model Class

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Models
{
    [Table("Customer", Schema = "dbo")]
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
```

### Controller Class

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Models;
using MyApp.Data;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .ToListAsync();
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // Additional controller methods...
    }
}
```
