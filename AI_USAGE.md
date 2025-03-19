# EzDbCodeGen AI Usage Guide

This guide provides examples and patterns for AI assistants to help users with the EzDbCodeGen library.

## Quick Start

```csharp
// 1. Create a configuration
var configuration = new Configuration();
configuration.SetValue("Namespace", "MyApp");
configuration.SetValue("OutputPath", "./Generated");

// 2. Set up entity security (optional)
var entitySecurity = new Dictionary<string, EntitySecurity>
{
    { "Customer", new EntitySecurity { Secured = true } },
    { "Order", new EntitySecurity { Secured = false } }
};
configuration.SetValue("EntitySecurity", entitySecurity);

// 3. Create a database schema (using EzDbSchema or a mock)
var database = new MockDatabaseSchema().CreateMockDatabase();
var templateDataInput = new TemplateDataInput(database);

// 4. Create the code generator
var codeGenerator = new TestCodeGenerator(configuration);

// 5. Process templates
var modelTemplatePath = "Templates/EFCoreModel.hbs";
codeGenerator.ProcessModelTemplate(modelTemplatePath, templateDataInput, "./Generated");

var controllerTemplatePath = "Templates/EFCoreController.hbs";
codeGenerator.ProcessControllerTemplate(controllerTemplatePath, templateDataInput, "./Generated");
```

## Template Processing

The code generator uses Handlebars.Net to process templates. Here's how to create a basic model template:

```handlebars
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace {{Namespace}}.Models
{
    [Table("{{TableName}}", Schema = "{{SchemaName}}")]
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
}
```

## Handling Entity Relationships

The code generator automatically processes entity relationships to create navigation properties:

```csharp
// Example of getting relationships for an entity
private IEnumerable<dynamic> GetRelationships(IDatabase database, IEntity entity)
{
    var relationships = new List<dynamic>();
    
    foreach (var relationship in database.Relationships)
    {
        // Process one-to-many relationships
        if (relationship.FromTableName == entity.TableName)
        {
            var toEntity = database.Entities[relationship.ToTableName];
            var isCollection = relationship.MultiplicityType == RelationshipMultiplicityType.OneToMany;
            
            relationships.Add(new
            {
                FromTableName = relationship.FromTableName,
                ToTableName = relationship.ToTableName,
                FromPropertyName = relationship.FromPropertyName,
                ToPropertyName = relationship.ToPropertyName,
                IsCollection = isCollection,
                RelatedEntity = toEntity.TableName,
                NavigationProperty = isCollection 
                    ? Pluralize(toEntity.TableName) 
                    : toEntity.TableName
            });
        }
        
        // Process many-to-one relationships
        if (relationship.ToTableName == entity.TableName && 
            relationship.MultiplicityType == RelationshipMultiplicityType.ManyToOne)
        {
            var fromEntity = database.Entities[relationship.FromTableName];
            
            relationships.Add(new
            {
                FromTableName = relationship.ToTableName,
                ToTableName = relationship.FromTableName,
                FromPropertyName = relationship.ToPropertyName,
                ToPropertyName = relationship.FromPropertyName,
                IsCollection = false,
                RelatedEntity = fromEntity.TableName,
                NavigationProperty = fromEntity.TableName
            });
        }
    }
    
    return relationships;
}
```

## Entity Security

The code generator can skip entities marked as secured:

```csharp
// Example of checking entity security
var entitySecurity = _configuration.EntitySecurity();
if (entitySecurity != null && 
    entitySecurity.TryGetValue(entity.TableName, out var security) && 
    security.Secured)
{
    Console.WriteLine($"Skipping secured entity: {entity.TableName}");
    continue;
}
```

## CLI Usage

The EzDbCodeGen tool can be installed as a .NET global tool:

```bash
dotnet tool install --global EzDbCodeGen.Cli
```

Then use it to generate code:

```bash
ezdbcodegen generate --config config.json --templates ./Templates --output ./Generated
```

## Debugging Tips

1. The code generator creates debug output files with the prefix `DEBUG_` that can be useful for troubleshooting.
2. Template data is also saved as JSON files with the prefix `DEBUG_DATA_` for inspecting the data passed to templates.
3. Use the console output to see detailed information about the code generation process.

## Common Issues and Solutions

1. **Missing navigation properties**: Ensure that relationships are properly defined in the database schema.
2. **Incorrect casing**: The code generator normalizes entity names to ensure proper casing.
3. **Template not found**: Check that the template path is correct and the file exists.
4. **Entity security not working**: Verify that the entity security configuration is properly set up.

## Best Practices

1. Use normalized entity names for consistent casing
2. Create separate templates for different types of code artifacts
3. Use debug output for troubleshooting
4. Test code generation with mock database schemas
5. Use entity security to control which entities are included in code generation
