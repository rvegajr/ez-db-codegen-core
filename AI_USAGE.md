# EzDbCodeGen AI Usage Guide

This guide provides comprehensive examples and patterns for AI assistants to help users with the EzDbCodeGen library.

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

## Schema Structure

The database schema structure used by EzDbCodeGen is based on the EzDbSchema.Core library. Here's a detailed outline of the key components:

```
IDatabase
├── Name (string)
├── DefaultSchema (string)
├── Entities (IEntityDictionary<string, IEntity>)
│   ├── IEntity
│   │   ├── TableName (string)
│   │   ├── TableAlias (string)
│   │   ├── DatabaseSchema (string)
│   │   ├── EntityType (string) - Table, View, etc.
│   │   ├── Properties (IPropertyDictionary<string, IProperty>)
│   │   │   ├── IProperty
│   │   │   │   ├── PropertyName (string)
│   │   │   │   ├── ColumnName (string)
│   │   │   │   ├── DataType (string)
│   │   │   │   ├── IsNullable (bool)
│   │   │   │   ├── IsPrimaryKey (bool)
│   │   │   │   ├── IsIdentity (bool)
│   │   │   │   ├── IsRequired (bool)
│   │   │   │   ├── MaxLength (int)
│   │   │   │   ├── Precision (int)
│   │   │   │   ├── Scale (int)
│   │   │   │   ├── DefaultValue (string)
│   │   │   │   ├── IsReadOnly (bool)
│   │   │   │   ├── IsVisible (bool)
│   │   │   │   ├── IsEditable (bool)
│   │   │   │   ├── DisplayName (string)
│   │   │   │   ├── Description (string)
│   │   │   │   ├── ValidationRules (string)
│   │   │   │   ├── RequiresEncryption (bool)
│   │   │   │   ├── RequiresMasking (bool)
│   │   │   │   └── IsSensitive (bool)
│   │   ├── Relationships (IRelationshipReferenceList)
│   │   │   ├── IRelationship
│   │   │   │   ├── ConstraintName (string)
│   │   │   │   ├── FromTableName (string)
│   │   │   │   ├── FromColumnName (string)
│   │   │   │   ├── FromPropertyName (string)
│   │   │   │   ├── ToTableName (string)
│   │   │   │   ├── ToColumnName (string)
│   │   │   │   ├── ToPropertyName (string)
│   │   │   │   ├── MultiplicityType (RelationshipMultiplicityType)
│   │   │   │   │   ├── OneToOne
│   │   │   │   │   ├── OneToMany
│   │   │   │   │   ├── ManyToOne
│   │   │   │   │   ├── ManyToMany
│   │   │   │   │   ├── ZeroOrOneToOne
│   │   │   │   │   ├── ZeroOrOneToMany
│   │   │   │   │   └── ManyToZeroOrOne
│   │   │   │   ├── IsOptional (bool)
│   │   │   │   ├── CascadeDelete (bool)
│   │   │   │   ├── FromEntity (IEntity)
│   │   │   │   ├── ToEntity (IEntity)
│   │   │   │   ├── FromProperty (IProperty)
│   │   │   │   └── ToProperty (IProperty)
│   │   ├── RelationshipGroups (IRelationshipGroups)
│   │   ├── PrimaryKeys (IPrimaryKeyProperties)
│   │   ├── HasPrimaryKeys() (bool)
│   │   ├── HasForeignKeyConstraints (bool)
│   │   ├── RequiresAuthorization (bool)
│   │   ├── HasRowLevelSecurity (bool)
│   │   ├── IsAuditable() (bool)
│   │   └── IsVersioned (bool)
└── Relationships (List<IRelationship>)
```

## Template Processing

The code generator uses Handlebars.Net to process templates. You can create templates for various code artifacts:

### Entity Framework Core Model Template

```handlebars
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace {{Namespace}}.Models
{
    [Table("{{ExtractTableName TableName}}", Schema = "{{ExtractSchemaName TableName}}")]
    public partial class {{ToPascalCase (ToSingular (ExtractTableName TableName))}}
    {
        public {{ToPascalCase (ToSingular (ExtractTableName TableName))}}()
        {
            {{#each (GetForeignKeyCollectionsForEntity this)}}
            {{NavigationProperty}} = new HashSet<{{RelatedEntity}}>();
            {{/each}}
        }

        {{#each Properties}}
        {{#if IsPrimaryKey}}
        [Key]
        {{/if}}
        {{#if IsIdentity}}
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        {{/if}}
        {{#if MaxLength}}
        [StringLength({{MaxLength}})]
        {{/if}}
        {{#if IsRequired}}
        [Required]
        {{/if}}
        [Column("{{ColumnName}}")]
        public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase PropertyName}} { get; set; }
        {{/each}}

        {{POCOModelFKProperties "        "}}

        {{#each (GetForeignKeyCollectionsForEntity this)}}
        public virtual ICollection<{{RelatedEntity}}> {{NavigationProperty}} { get; set; }
        {{/each}}
    }
}
```

### Fluent API Configuration Template

```handlebars
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {{Namespace}}.Models;

namespace {{Namespace}}.Data.Configuration
{
    public class {{EntityName}}Configuration : IEntityTypeConfiguration<{{EntityName}}>
    {
        public void Configure(EntityTypeBuilder<{{EntityName}}> builder)
        {
            builder.ToTable("{{ExtractTableName TableName}}", "{{ExtractSchemaName TableName}}");

            {{#each Properties}}
            {{#if IsPrimaryKey}}
            builder.HasKey(e => e.{{PropertyName}});
            {{/if}}
            
            {{#if MaxLength}}
            builder.Property(e => e.{{PropertyName}})
                .HasMaxLength({{MaxLength}});
            {{/if}}
            
            {{#if IsRequired}}
            builder.Property(e => e.{{PropertyName}})
                .IsRequired();
            {{/if}}
            {{/each}}

            {{#each Relationships}}
            {{#if (eq MultiplicityType "OneToMany")}}
            builder.HasMany(d => d.{{NavigationProperty}})
                .WithOne(p => p.{{InverseNavigationProperty}})
                .HasForeignKey(d => d.{{ForeignKeyProperty}})
                .OnDelete({{#if CascadeDelete}}DeleteBehavior.Cascade{{else}}DeleteBehavior.ClientSetNull{{/if}})
                .HasConstraintName("{{ConstraintName}}");
            {{else if (eq MultiplicityType "ManyToOne")}}
            builder.HasOne(d => d.{{NavigationProperty}})
                .WithMany(p => p.{{InverseNavigationProperty}})
                .HasForeignKey(d => d.{{ForeignKeyProperty}})
                .OnDelete({{#if CascadeDelete}}DeleteBehavior.Cascade{{else}}DeleteBehavior.ClientSetNull{{/if}})
                .HasConstraintName("{{ConstraintName}}");
            {{else if (eq MultiplicityType "OneToOne")}}
            builder.HasOne(d => d.{{NavigationProperty}})
                .WithOne(p => p.{{InverseNavigationProperty}})
                .HasForeignKey<{{EntityName}}>(d => d.{{ForeignKeyProperty}})
                .OnDelete({{#if CascadeDelete}}DeleteBehavior.Cascade{{else}}DeleteBehavior.ClientSetNull{{/if}})
                .HasConstraintName("{{ConstraintName}}");
            {{/if}}
            {{/each}}
        }
    }
}
```

## Enhanced Conditional Expressions with IfCondEz

The `IfCondEz` helper provides a powerful way to navigate the schema tree and evaluate complex conditions in your templates. It supports a rich expression language with proper order of operations, schema navigation, and various operators.

### Basic Syntax

```handlebars
{{#IfCondEz "expression"}}
  // Code to render if expression evaluates to true
{{else}}
  // Code to render if expression evaluates to false
{{/IfCondEz}}
```

### Schema Navigation

The `IfCondEz` helper uses a dot notation with `$` as the root context:

```handlebars
{{#IfCondEz "$.EntityType='Table'"}}
  // This is a table
{{else}}
  // This is not a table
{{/IfCondEz}}
```

### Property Access

You can access nested properties using dot notation or array/dictionary syntax:

```handlebars
{{#IfCondEz "$.Properties['Id'].IsPrimaryKey=true"}}
  // Entity has a primary key named Id
{{/IfCondEz}}

{{#IfCondEz "$.Properties.Count > 0"}}
  // Entity has properties
{{/IfCondEz}}
```

### Supported Operators

#### Comparison Operators
- `=` or `==` - Equal to
- `!=` or `<>` - Not equal to
- `>` - Greater than
- `<` - Less than
- `>=` - Greater than or equal to
- `<=` - Less than or equal to

#### Logical Operators
- `&&` - Logical AND
- `||` - Logical OR
- `!` or `not` - Logical NOT

#### String Operators
- `contains` - Check if a string contains another string
- `startsWith` - Check if a string starts with another string
- `endsWith` - Check if a string ends with another string

### Complex Expressions

You can combine multiple conditions and use parentheses to control the order of evaluation:

```handlebars
{{#IfCondEz "($.EntityType='View' || $.EntityType='Table') && $.Properties.Count > 0"}}
  // This is a view or table with properties
{{/IfCondEz}}

{{#IfCondEz "$.EntityType='View' && $.Relationships.Count=0"}}
  // This is a view with no relationships
{{/IfCondEz}}

{{#IfCondEz "!($.Properties['Id'].IsPrimaryKey=true) || $.EntityType contains 'View'"}}
  // Either has no primary key named Id or is a view
{{/IfCondEz}}
```

### Real-World Examples

#### Conditional Template for Views vs. Tables

```handlebars
{{#IfCondEz "$.EntityType='View' && $.Relationships.Count=0"}}
  // This is a view with no relationships
  public partial class {{TableName}}View 
  {
    // View properties only
    {{#each Properties}}
    public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase PropertyName}} { get; set; }
    {{/each}}
  }
{{else}}
  // This is a regular entity or a view with relationships
  public partial class {{ToPascalCase TableName}}
  {
    // Regular entity implementation
    {{#each Properties}}
    public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase PropertyName}} { get; set; }
    {{/each}}
    
    {{#each (GetForeignKeyCollectionsForEntity this)}}
    public virtual ICollection<{{RelatedEntity}}> {{NavigationProperty}} { get; set; }
    {{/each}}
  }
{{/IfCondEz}}
```

#### Conditional Logic Based on Primary Key and Relationships

```handlebars
{{#IfCondEz "$.Properties['Id'].IsPrimaryKey=true && ($.Relationships.Count > 0 || $.EntityType='Table')"}}
  // Entity with primary key that is either a table or has relationships
  public partial class {{ToPascalCase TableName}}Entity : IEntity
  {
    // Full entity implementation with navigation properties
    {{#each Properties}}
    public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase PropertyName}} { get; set; }
    {{/each}}
    
    {{#each (GetForeignKeyCollectionsForEntity this)}}
    public virtual ICollection<{{RelatedEntity}}> {{NavigationProperty}} { get; set; }
    {{/each}}
  }
{{else}}
  // Simple data transfer object
  public partial class {{ToPascalCase TableName}}Dto
  {
    // DTO implementation without navigation properties
    {{#each Properties}}
    public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase PropertyName}} { get; set; }
    {{/each}}
  }
{{/IfCondEz}}
```

## Handling Entity Relationships

The code generator automatically processes entity relationships to create navigation properties. Here's how different relationship types are handled:

### One-to-Many Relationships

```csharp
// Example of processing one-to-many relationships
private IEnumerable<dynamic> GetOneToManyRelationships(IEntity entity)
{
    var relationships = new List<dynamic>();
    
    // Get relationships where this entity is the "one" side
    var oneToManyRelationships = entity.Relationships
        .Where(r => r.FromTableName == entity.TableName && 
               (r.MultiplicityType == RelationshipMultiplicityType.OneToMany ||
                r.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToMany));
    
    foreach (var relationship in oneToManyRelationships)
    {
        var toEntity = relationship.ToEntity;
        if (toEntity == null) continue;
        
        // Create navigation property for the collection
        var navigationProperty = toEntity.TableName.ToPlural().ToPascalCase();
        
        relationships.Add(new
        {
            ConstraintName = relationship.ConstraintName,
            FromTableName = relationship.FromTableName,
            ToTableName = relationship.ToTableName,
            NavigationProperty = navigationProperty,
            RelatedEntity = toEntity.TableName.ToSingular().ToPascalCase(),
            ForeignKeyProperty = relationship.ToColumnName.ToPascalCase(),
            InverseNavigationProperty = entity.TableName.ToSingular().ToPascalCase(),
            MultiplicityType = "OneToMany",
            CascadeDelete = relationship.CascadeDelete
        });
    }
    
    return relationships;
}
```

### Many-to-One Relationships

```csharp
// Example of processing many-to-one relationships
private IEnumerable<dynamic> GetManyToOneRelationships(IEntity entity)
{
    var relationships = new List<dynamic>();
    
    // Get relationships where this entity is the "many" side
    var manyToOneRelationships = entity.Relationships
        .Where(r => r.ToTableName == entity.TableName && 
               r.MultiplicityType == RelationshipMultiplicityType.ManyToOne);
    
    foreach (var relationship in manyToOneRelationships)
    {
        var fromEntity = relationship.FromEntity;
        if (fromEntity == null) continue;
        
        // Create navigation property for the reference
        var navigationProperty = fromEntity.TableName.ToSingular().ToPascalCase();
        
        relationships.Add(new
        {
            ConstraintName = relationship.ConstraintName,
            FromTableName = relationship.FromTableName,
            ToTableName = relationship.ToTableName,
            NavigationProperty = navigationProperty,
            RelatedEntity = fromEntity.TableName.ToSingular().ToPascalCase(),
            ForeignKeyProperty = relationship.FromColumnName.ToPascalCase(),
            InverseNavigationProperty = entity.TableName.ToPlural().ToPascalCase(),
            MultiplicityType = "ManyToOne",
            CascadeDelete = relationship.CascadeDelete
        });
    }
    
    return relationships;
}
```

### One-to-One Relationships

```csharp
// Example of processing one-to-one relationships
private IEnumerable<dynamic> GetOneToOneRelationships(IEntity entity)
{
    var relationships = new List<dynamic>();
    
    // Get one-to-one relationships
    var oneToOneRelationships = entity.Relationships
        .Where(r => r.MultiplicityType == RelationshipMultiplicityType.OneToOne ||
                    r.MultiplicityType == RelationshipMultiplicityType.ZeroOrOneToOne);
    
    foreach (var relationship in oneToOneRelationships)
    {
        IEntity relatedEntity;
        string navigationProperty;
        string foreignKeyProperty;
        
        if (relationship.FromTableName == entity.TableName)
        {
            relatedEntity = relationship.ToEntity;
            navigationProperty = relatedEntity.TableName.ToSingular().ToPascalCase();
            foreignKeyProperty = relationship.FromColumnName.ToPascalCase();
        }
        else
        {
            relatedEntity = relationship.FromEntity;
            navigationProperty = relatedEntity.TableName.ToSingular().ToPascalCase();
            foreignKeyProperty = relationship.ToColumnName.ToPascalCase();
        }
        
        if (relatedEntity == null) continue;
        
        relationships.Add(new
        {
            ConstraintName = relationship.ConstraintName,
            NavigationProperty = navigationProperty,
            RelatedEntity = relatedEntity.TableName.ToSingular().ToPascalCase(),
            ForeignKeyProperty = foreignKeyProperty,
            InverseNavigationProperty = entity.TableName.ToSingular().ToPascalCase(),
            MultiplicityType = "OneToOne",
            CascadeDelete = relationship.CascadeDelete
        });
    }
    
    return relationships;
}
```

## Handlebars Helpers Reference

EzDbCodeGen provides a rich set of Handlebars helpers to make template creation easier:

### Basic Helpers

- `{{Debugger}}` - Adds a debug point (useful for development)
- `{{Comma}}` - Outputs a comma (useful in template loops)
- `{{ToJson value}}` - Converts a value to JSON format
- `{{ContextAsJson}}` - Dumps the current context as JSON (great for debugging)

### Type Conversion Helpers

- `{{ToNetType}}` - Converts a database type to .NET type
- `{{ToJsType}}` - Converts a database type to JavaScript type
- `{{AsNullableType type isNullable}}` - Adds nullable marker (?) if needed

### String Formatting Helpers

- `{{ToPascalCase}}` - Converts string to PascalCase
- `{{ToCamelCase}}` - Converts string to camelCase
- `{{ToSnakeCase}}` - Converts string to snake_case
- `{{ToSingular}}` - Converts plural to singular
- `{{ToPlural}}` - Converts singular to plural
- `{{ToCodeFriendly}}` - Makes string code-friendly
- `{{ExtractTableName}}` - Extracts table name from schema.table
- `{{ExtractSchemaName}}` - Extracts schema name from schema.table
- `{{AsFormattedName}}` - Formats name by removing common suffixes
- `{{Prefix prefix value}}` - Adds prefix to string
- `{{StringFormat value format1 format2...}}` - Applies multiple string formats

### Conditional Helpers

- `{{#IfPropertyExists propertyName}}...{{/IfPropertyExists}}` - Checks if property exists
- `{{#IfFunctionExists functionName}}...{{/IfFunctionExists}}` - Checks if function exists
- `{{#IfCond left op right}}...{{else}}...{{/IfCond}}` - Conditional logic with operators
- `{{#Switch value}}{{#Case 'option'}}...{{/Case}}{{#Default}}...{{/Default}}{{/Switch}}` - Switch/case logic
- `{{#IfCondEz "expression"}}...{{else}}...{{/IfCondEz}}` - Enhanced conditional expression evaluator with schema navigation and equation processing

### TypeScript Helpers

- `{{tsType}}` - Converts to TypeScript type
- `{{tsInterface}}` - Formats name as interface
- `{{tsModel}}` - Formats name as model
- `{{tsService}}` - Formats name as service
- `{{tsComponent}}` - Formats name as component
- `{{tsModule}}` - Formats name as module
- `{{tsRouting}}` - Formats name as routing
- `{{tsStore}}` - Formats name as store

### Foreign Key Helpers

- `{{POCOModelFKProperties}}` - Generates POCO model foreign key properties
- `{{POCOModelFKManyToZeroToOne}}` - Generates POCO model many-to-zero-or-one relationships
- `{{GetForeignKeyProperties}}` - Gets foreign key properties for an entity
- `{{GetOneToOneReferences}}` - Gets one-to-one references
- `{{GetForeignKeyCollectionsForEntity}}` - Gets foreign key collections

## Core Interfaces

EzDbCodeGen works primarily through these core interfaces from EzDbSchema.Core:

### IDatabase
The root interface representing your database schema:
```csharp
public interface IDatabase
{
    string Name { get; }
    string DefaultSchema { get; }
    IEntityDictionary<string, IEntity> Entities { get; }
}
```

### IEntity
Represents a database table or view:
```csharp
public interface IEntity
{
    string TableName { get; }
    string TableAlias { get; }
    string DatabaseSchema { get; }
    string EntityType { get; }  // Table, View, etc.
    IPropertyDictionary<string, IProperty> Properties { get; }
    IRelationshipReferenceList Relationships { get; }
    bool HasPrimaryKeys();
    bool HasForeignKeyConstraints { get; }
}
```

### IProperty
Represents a column in your database:
```csharp
public interface IProperty
{
    string PropertyName { get; }
    string ColumnName { get; }
    string DataType { get; }
    bool IsNullable { get; }
    bool IsPrimaryKey { get; }
    bool IsIdentity { get; }
    bool IsRequired { get; }
    int MaxLength { get; }
    string DefaultValue { get; }
}
```

### IRelationship
Represents relationships between tables:
```csharp
public interface IRelationship
{
    string ConstraintName { get; }
    string FromTableName { get; }
    string FromColumnName { get; }
    string ToTableName { get; }
    string ToColumnName { get; }
    RelationshipMultiplicityType MultiplicityType { get; }
    bool IsOptional { get; }
    IEntity FromEntity { get; }
    IEntity ToEntity { get; }
}
```

### Using the Interfaces in Templates

Here are common patterns for working with these interfaces in your templates:

1. **Accessing Entity Properties**:
```handlebars
{{#each Entities}}
public class {{format "pascalCase" TableName}}
{
    {{#each Properties}}
    public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" PropertyName}} { get; set; }
    {{/each}}
}
{{/each}}
```

2. **Working with Relationships**:
```handlebars
{{#each Relationships}}
{{#if (eq MultiplicityType "OneToMany")}}
public virtual ICollection<{{format "pascalCase" ToTableName}}> {{format "pascalCase" ToTableName}} { get; set; } 
    = new List<{{format "pascalCase" ToTableName}}>();
{{else}}
public virtual {{format "pascalCase" ToTableName}} {{format "pascalCase" ToTableName}} { get; set; } = null!;
{{/if}}
{{/each}}
```

3. **Handling Primary Keys**:
```handlebars
{{#if HasPrimaryKeys}}
{{#each Properties}}
{{#if IsPrimaryKey}}
[Key]
public {{convertType DataType "csharp" false}} {{format "pascalCase" PropertyName}} { get; set; }
{{/if}}
{{/each}}
{{/if}}
```

4. **Schema-Aware Generation**:
```handlebars
namespace {{Namespace}}.{{DatabaseSchema}}
{
    [Table("{{TableName}}", Schema = "{{DatabaseSchema}}")]
    public class {{format "pascalCase" TableName}}
    {
        // Properties here
    }
}
```

### Best Practices for Interface Usage

1. **Type Safety**:
```csharp
// Check entity type before processing
{{#if (eq EntityType "Table")}}
// Generate table-specific code
{{else}}
// Generate view-specific code
{{/if}}
```

2. **Null Handling**:
```handlebars
{{#each Properties}}
{{#if IsRequired}}
[Required]
public {{convertType DataType "csharp" false}} {{format "pascalCase" PropertyName}} { get; set; }
{{else}}
public {{convertType DataType "csharp" true}}? {{format "pascalCase" PropertyName}} { get; set; }
{{/if}}
{{/each}}
```

3. **Relationship Navigation**:
```handlebars
{{#each Relationships}}
// Access related entities
var fromEntity = FromEntity;
var toEntity = ToEntity;
var fromProperty = FromProperty;
var toProperty = ToProperty;
{{/each}}
```

4. **Custom Attributes**:
```handlebars
{{#each Properties}}
{{#if CustomAttributes}}
/// <summary>
/// {{lookup CustomAttributes "Description"}}
/// </summary>
{{#if (lookup CustomAttributes "Deprecated")}}
[Obsolete("{{lookup CustomAttributes "Deprecated"}}")]
{{/if}}
public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" PropertyName}} { get; set; }
{{/if}}
{{/each}}
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

## Template Building Guide

When building templates from source files, follow these steps:

1. **Analyze the source file structure** - Identify the patterns and components
2. **Extract variable parts** - Replace with Handlebars expressions
3. **Add conditional logic** - Use Handlebars conditionals for variations
4. **Handle loops** - Use Handlebars each helpers for collections
5. **Test with sample data** - Verify output matches expectations

### Example: Converting a C# class to a template

Original class:
```csharp
public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Order> Orders { get; set; }
}
```

Converted to template:
```handlebars
{{#layout}}
  {{#region "header"}}
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;
  {{/region}}

  {{#region "class"}}
    [Table("{{ExtractTableName TableName}}", Schema = "{{ExtractSchemaName TableName}}")]
    public partial class {{ToPascalCase (ToSingular (ExtractTableName TableName))}}
    {
        public {{ToPascalCase (ToSingular (ExtractTableName TableName))}}()
        {
            {{#each (GetForeignKeyCollectionsForEntity this)}}
            {{NavigationProperty}} = new HashSet<{{RelatedEntity}}>();
            {{/each}}
        }

        {{#each Properties}}
        {{#if IsPrimaryKey}}
        [Key]
        {{/if}}
        {{#if IsIdentity}}
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        {{/if}}
        {{#if MaxLength}}
        [StringLength({{MaxLength}})]
        {{/if}}
        {{#if IsRequired}}
        [Required]
        {{/if}}
        [Column("{{ColumnName}}")]
        public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase PropertyName}} { get; set; }
        {{/each}}

        {{POCOModelFKProperties "        "}}

        {{#each (GetForeignKeyCollectionsForEntity this)}}
        public virtual ICollection<{{RelatedEntity}}> {{NavigationProperty}} { get; set; }
        {{/each}}
    }
  {{/region}}
{{/layout}}
```

## CLI Usage

The EzDbCodeGen tool can be installed as a .NET global tool:

```bash
dotnet tool install --global EzDbCodeGen.Cli
```

Then use it to generate code:

```bash
ezdbcodegen -a MyApp -s dbo -t Templates/EFCoreModel.hbs -c "Server=localhost;Database=MyDB;Trusted_Connection=True;" -o ./Generated -v
```

### CLI Options

```
Usage: ezdbcg [options]

Options:
  -a|--app-name <n>              Application name (default: "MyApp")
  -s|--schema-name <n>           Schema name (default: "MySchema")
  -t|--template <PATH>           Template file or directory path
  -c|--connection-string <CS>    Database connection string
  -v|--verbose                   Enable verbose output
  -o|--output <PATH>             Output directory path
  --save-settings                Save current settings for future use
  -?|-h|--help                   Show help information
```

## In-Memory Testing

For testing purposes, EzDbCodeGen supports an in-memory mode:

```csharp
var command = new CommandMainTestable
{
    UseInMemoryStorage = true,
    AppName = "TestApp",
    SchemaName = "TestSchema",
    TemplateFileNameOrPath = "path/to/template.hbs",
    ConnectionString = "Server=localhost;Database=TestDB;User Id=test;Password=test;"
};

// Set up mock database
command.SetMockDatabase(mockDatabase);

// Add template to in-memory storage
command.InMemoryFiles[command.TemplateFileNameOrPath] = "{{AppName}}";

// Execute
var result = command.TestOnExecute();
```

## Debugging Tips

1. The code generator creates debug output files with the prefix `DEBUG_` that can be useful for troubleshooting.
2. Template data is also saved as JSON files with the prefix `DEBUG_DATA_` for inspecting the data passed to templates.
3. Use the console output to see detailed information about the code generation process.
4. Use the `{{ContextAsJson}}` helper to dump the current context for debugging.

## Common Issues and Solutions

1. **Missing navigation properties**: Ensure that relationships are properly defined in the database schema.
2. **Incorrect casing**: The code generator normalizes entity names to ensure proper casing.
3. **Template not found**: Check that the template path is correct and the file exists.
4. **Entity security not working**: Verify that the entity security configuration is properly set up.
5. **Null reference exceptions**: Check for null values in templates using conditional helpers.
6. **Namespace issues**: Be aware of the difference between EzDbSchema.Core.Extensions and EzDbSchema.Core.Extentions.

## Best Practices

1. Use normalized entity names for consistent casing
2. Create separate templates for different types of code artifacts
3. Use debug output for troubleshooting
4. Test code generation with mock database schemas
5. Use entity security to control which entities are included in code generation
6. Leverage Handlebars helpers to simplify templates
7. Use fluent API for complex entity relationships
8. Create reusable partial templates for common patterns
9. Implement proper null handling for collections and properties
10. Use fully-qualified type names to avoid ambiguous references

## AI Usage Guide for EzDbCodeGen Core

This guide helps AI assistants understand and work with the EzDbCodeGen Core library's templating system.

## Core Concepts

### Template Structure
Templates should follow these principles:
1. **Single Responsibility**: Each template should generate one type of output
2. **Dependency Injection Ready**: Use layouts to separate concerns
3. **Testable**: Include test cases for template outputs
4. **SOLID Compliant**: Follow SOLID principles in template organization

## Handlebars Helpers Reference

### 1. FormatEz Helper
```handlebars
{{format "<operations>" value}}
```

Operations (can be chained with comma or pipe):
- **Case Transformations**:
  - `camelCase`: "my_variable" → "myVariable"
  - `pascalCase`: "my_variable" → "MyVariable"
  - `snakeCase`: "myVariable" → "my_variable"
  - `kebabCase`: "myVariable" → "my-variable"
  - `uppercase`: "myVariable" → "MYVARIABLE"
  - `lowercase`: "MyVariable" → "myvariable"

- **Indentation**:
  - `indent2`: 2 spaces
  - `indent4`: 4 spaces (default)
  - `indent8`: 8 spaces

- **Tabbing**:
  - `tab1`: 1 tab
  - `tab2`: 2 tabs
  - `tab4`: 4 tabs

Example:
```handlebars
{{format "camelCase,tab1" "user_name"}} → "	userName"
{{format "pascalCase|uppercase" "my_var"}} → "MYVAR"
```

### 2. ConvertTypeEz Helper
```handlebars
{{convertType sqlType targetLanguage [isNullable]}}
```

Supported mappings:
```typescript
// SQL to C#
"int" → "int" | "int?"
"varchar(50)" → "string"
"datetime" → "DateTime" | "DateTime?"

// SQL to TypeScript
"int" → "number"
"varchar" → "string"
"datetime" → "Date"

// SQL to Java
"int" → "Integer"
"varchar" → "String"
"datetime" → "LocalDateTime"

// SQL to Python
"int" → "int"
"varchar" → "str"
"datetime" → "datetime"
```

Example:
```handlebars
{{convertType "int" "csharp" true}} → "int?"
{{convertType "varchar(50)" "typescript"}} → "string"
```

### 3. DocEz Helper
```handlebars
{{doc object format}}
```

Documentation formats:
- **XML (C#)**:
```handlebars
{{doc property "xml"}}
→ "/// <summary>
   /// Property description
   /// </summary>
   /// <remarks>Additional info</remarks>"
```

- **JSDoc (TypeScript/JavaScript)**:
```handlebars
{{doc property "jsdoc"}}
→ "/**
   * Property description
   * @type {string}
   */"
```

### 4. LayoutEz Helper
Template organization helper for structured code generation:

```handlebars
{{#layout}}
  {{#region "imports"}}
    using System;
    using System.Collections.Generic;
  {{/region}}

  {{#region "class"}}
    public class {{className}} {
      {{> properties}}
    }
  {{/region}}

  {{#region "methods"}}
    {{> crud-methods}}
  {{/region}}
{{/layout}}
```

Features:
- Named regions for organization
- Partial template support
- Region ordering control
- Conditional region rendering

### 5. CodeFormatEz Helper
```handlebars
{{formatCode language codeString}}
```

Language-specific formatting:

**C#**:
```handlebars
{{formatCode "csharp" "public class User{public int Id{get;set;}}"}}
→ "public class User {
     public int Id { get; set; }
   }"
```

**SQL**:
```handlebars
{{formatCode "sql" "SELECT id,name FROM users WHERE active=1"}}
→ "SELECT id, name
   FROM users
   WHERE active = 1"
```

## AI Template Generation Guidelines

1. **Schema Analysis**:
```csharp
// Analyze schema before generating templates
foreach (var table in schema.Tables)
{
    // Check relationships
    var hasOneToMany = table.Relationships.Any(r => r.Multiplicity == "OneToMany");
    // Check primary keys
    var hasPrimaryKey = table.Properties.Any(p => p.IsPrimaryKey);
}
```

2. **Template Pattern Selection**:
- Entity classes: Use class templates with property generation
- Controllers: Use REST API templates with CRUD operations
- ViewModels: Use DTO templates with mapping
- Documentation: Use documentation templates with type info

3. **Code Style Compliance**:
- Use `FormatEz` for consistent naming
- Use `CodeFormatEz` for language-specific formatting
- Use `DocEz` for standardized documentation
- Use `LayoutEz` for organized output

4. **Testing Templates**:
```csharp
// Create test cases for templates
[Fact]
public void Template_GeneratesValidCode()
{
    var template = @"
        {{#each Tables}}
        public class {{format 'pascalCase' Name}} {
            {{#each Properties}}
            {{doc this 'xml'}}
            public {{convertType DataType 'csharp' IsNullable}} {{format 'pascalCase' Name}} { get; set; }
            {{/each}}
        }
        {{/each}}
    ";
    
    var result = generator.Generate(template, testSchema);
    Assert.Contains("public class", result);
}
```

## Common Template Patterns

1. **Entity Class Template**:
```handlebars
{{#layout}}
  {{#region "header"}}
    using System;
    using System.ComponentModel.DataAnnotations;
  {{/region}}

  {{#region "class"}}
    {{doc table "xml"}}
    public class {{format "pascalCase" table.Name}}
    {
        {{#each table.Properties}}
        {{doc this "xml"}}
        public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" Name}} { get; set; }
        {{/each}}
    }
  {{/region}}
{{/layout}}
```

2. **API Controller Template**:
```handlebars
{{#layout}}
  {{#region "header"}}
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
  {{/region}}

  {{#region "class"}}
    [ApiController]
    [Route("api/[controller]")]
    public class {{format "pascalCase" table.Name}}Controller : ControllerBase
    {
        {{> crud-endpoints}}
    }
  {{/region}}
{{/layout}}
```

3. **Repository Template**:
```handlebars
{{#layout}}
  {{#region "interface"}}
    public interface I{{format "pascalCase" table.Name}}Repository
    {
        {{> repository-methods}}
    }
  {{/region}}

  {{#region "implementation"}}
    public class {{format "pascalCase" table.Name}}Repository : I{{format "pascalCase" table.Name}}Repository
    {
        {{> repository-implementation}}
    }
  {{/region}}
{{/layout}}
```

## Best Practices

1. **Template Organization**:
   - Use layouts for structured output
   - Separate concerns into regions
   - Use partial templates for reusable components

2. **Naming Conventions**:
   - Use consistent casing through FormatEz
   - Follow language conventions
   - Use descriptive region names

3. **Documentation**:
   - Include XML/JSDoc comments
   - Document relationships
   - Add usage examples

4. **Error Handling**:
   - Validate input schema
   - Handle null values
   - Include type checks

5. **Testing**:
   - Create unit tests for templates
   - Test edge cases
   - Verify formatting

6. **Performance**:
   - Use efficient helpers
   - Minimize template complexity
   - Cache compiled templates

## Troubleshooting

Common issues and solutions:

1. **Incorrect Type Conversion**:
   - Verify SQL type mapping
   - Check nullable flag
   - Use proper target language

2. **Formatting Issues**:
   - Chain format operations correctly
   - Check language-specific rules
   - Verify template structure

3. **Documentation Generation**:
   - Ensure proper metadata
   - Check format specification
   - Verify helper usage

## Connection Testing

The CLI tool provides robust connection testing capabilities through an interface-first design:

```bash
# Test connection with basic settings
ezdbcg --test-connection -c "Server=localhost;Database=MyDb;User Id=sa;Password=****"

# Test connection with custom timeout (in seconds)
ezdbcg --test-connection --connection-timeout 10 -c "Server=localhost;Database=MyDb;User Id=sa;Password=****"

# Test connection with SSL/TLS trust for development
ezdbcg --test-connection -c "Server=localhost;Database=MyDb;User Id=sa;Password=****;TrustServerCertificate=True"
```

### Error Handling

The connection tester provides detailed feedback for various failure scenarios:

1. **Connection String Format**
   - Empty or null connection string
   - Missing required parameters (e.g., database name)
   - Invalid connection string format

2. **Authentication Issues**
   - Invalid credentials
   - Insufficient permissions
   - Missing or incorrect user/password

3. **Network and Security**
   - Server unreachable
   - SSL/TLS certificate validation failures
   - Connection timeouts
   - Pre-login handshake errors

4. **Database Access**
   - Database does not exist
   - Database not accessible
   - Server configuration issues

### Interface-Based Design

The connection testing feature follows our interface-first approach:

```csharp
// Core interface for connection testing
public interface IConnectionTester
{
    Task<(ReturnCode Code, string Message)> TestConnectionAsync(string connectionString, int? timeoutSeconds = null);
}

// Usage in CLI
var tester = new MsSqlConnectionTester();
var (code, message) = await tester.TestConnectionAsync(connectionString, timeoutSeconds: 10);
```

This design allows for:
- Easy extension to other database providers
- Consistent error handling across implementations
- Clean separation of concerns
- Testable components

```
## Connection Testing

The CLI tool provides robust connection testing capabilities through an interface-first design:

```bash
# Test connection with basic settings
ezdbcg --test-connection -c "Server=localhost;Database=MyDb;User Id=sa;Password=****"

# Test connection with custom timeout (in seconds)
ezdbcg --test-connection --connection-timeout 10 -c "Server=localhost;Database=MyDb;User Id=sa;Password=****"

# Test connection with SSL/TLS trust for development
ezdbcg --test-connection -c "Server=localhost;Database=MyDb;User Id=sa;Password=****;TrustServerCertificate=True"
```

### Error Handling

The connection tester provides detailed feedback for various failure scenarios:

1. **Connection String Format**
   - Empty or null connection string
   - Missing required parameters (e.g., database name)
   - Invalid connection string format

2. **Authentication Issues**
   - Invalid credentials
   - Insufficient permissions
   - Missing or incorrect user/password

3. **Network and Security**
   - Server unreachable (with timeout details)
   - SSL/TLS certificate validation failures
   - Connection timeouts (customizable)
   - Pre-login handshake errors

4. **Database Access**
   - Database does not exist
   - Database not accessible
   - Server configuration issues

### Interface-Based Design

The connection testing feature follows our interface-first approach:

```csharp
// Core interface for connection testing
public interface IConnectionTester
{
    Task<(ReturnCode Code, string Message)> TestConnectionAsync(
        string connectionString, 
        int? timeoutSeconds = null
    );
}

// Example usage in code
var tester = new MsSqlConnectionTester();
var (code, message) = await tester.TestConnectionAsync(
    connectionString,
    timeoutSeconds: 10 // Optional timeout
);
```

This design allows for:
- Easy extension to other database providers
- Consistent error handling across implementations
- Clean separation of concerns
- Testable components
- Customizable timeouts

### Best Practices

1. **Connection String Security**
   ```csharp
   // Use connection builder for safe string manipulation
   var builder = new SqlConnectionStringBuilder(connectionString)
   {
       TrustServerCertificate = true,
       ConnectTimeout = 30
   };
   ```

2. **Error Handling**
   ```csharp
   try
   {
       var result = await tester.TestConnectionAsync(connectionString);
       // Handle result based on ReturnCode
   }
   catch (Exception ex)
   {
       // Handle unexpected errors
   }
   ```

3. **Timeout Configuration**
   - Default timeout: 30 seconds
   - Configurable via CLI: `--connection-timeout`
   - Overridable in connection string
   - Respects existing timeout settings

### EzDbSchema.Core Integration

### Common Integration Issues

1. **Handling Custom Attributes**:
```csharp
// Template pattern for accessing custom attributes safely
{{#each Properties}}
    {{#if CustomAttributes}}
    /// <summary>
    /// {{lookup CustomAttributes "Description"}}
    /// </summary>
    {{#if (lookup CustomAttributes "Deprecated")}}
    [Obsolete("{{lookup CustomAttributes "Deprecated"}}")]
    {{/if}}
    public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" Name}} { get; set; }
    {{/if}}
{{/each}}
```

2. **Property Access Extensions**:
```handlebars
{{#layout}}
  {{#region "extensions"}}
    public static class PropertyExtensions
    {
        public static T GetAttributeValue<T>(this KeyValuePair<string, IProperty> property, string key, T defaultValue = default)
        {
            if (property.Value?.CustomAttributes == null) return defaultValue;
            return property.Value.CustomAttributes.TryGetValue(key, out var value) 
                ? (T)value 
                : defaultValue;
        }
    }
  {{/region}}
{{/layout}}
```

3. **JSON Serialization Conflicts**:
```handlebars
{{#layout}}
  {{#region "header"}}
    #if NET8_0_OR_GREATER
    using System.Text.Json;
    using System.Text.Json.Serialization;
    #else
    using Newtonsoft.Json;
    #endif
  {{/region}}

  {{#region "class"}}
    #if NET8_0_OR_GREATER
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    #else
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    #endif
    public class {{format "pascalCase" table.Name}}
    {
        {{#each Properties}}
        #if NET8_0_OR_GREATER
        [JsonPropertyName("{{format "camelCase" Name}}")]
        #else
        [JsonProperty("{{format "camelCase" Name}}")]
        #endif
        public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" Name}} { get; set; }
        {{/each}}
    }
  {{/region}}
{{/layout}}
```

### Namespace Resolution

When dealing with namespace conflicts:

```handlebars
{{#layout}}
  {{#region "header"}}
    using CoreExtensions = EzDbSchema.Core.Extensions;
    using LocalExtensions = YourNamespace.Extensions;
  {{/region}}

  {{#region "implementation"}}
    public class {{format "pascalCase" table.Name}}Repository
    {
        public void ProcessProperty(IProperty property)
        {
            var coreValue = CoreExtensions.PropertyExtensions.GetValue(property);
            var localValue = LocalExtensions.PropertyExtensions.GetValue(property);
        }
    }
  {{/region}}
{{/layout}}
```

### Schema Version Compatibility

Template patterns for handling different schema versions:

```handlebars
{{#layout}}
  {{#region "version_check"}}
    {{#if (gt SchemaVersion "8.4.0")}}
    // New schema features
    public required string NewFeature { get; set; }
    {{else}}
    // Legacy compatibility
    public string NewFeature { get; set; } = null!;
    {{/if}}
  {{/region}}
{{/layout}}
```

### Best Practices for Schema Integration

1. **Version Handling**:
   ```csharp
   // Check schema version in template
   var schemaVersion = schema.Version ?? "8.4.0";
   var useNewFeatures = Version.Parse(schemaVersion) >= Version.Parse("8.4.1");
   ```

2. **Property Type Safety**:
   ```csharp
   // Safe property type conversion
   public static string GetSafeTypeName(IProperty property)
   {
       if (string.IsNullOrEmpty(property?.DataType))
           return "object";
       
       return property.DataType.ToLowerInvariant() switch
       {
           "nvarchar" or "varchar" or "char" => "string",
           "int" or "bigint" => "int",
           "bit" => "bool",
           "datetime" or "datetime2" => "DateTime",
           _ => "object"
       };
   }
   ```

3. **Relationship Handling**:
```handlebars
{{#each Relationships}}
{{#if (eq Multiplicity "OneToMany")}}
public virtual ICollection<{{format "pascalCase" ReferencedTable}}> {{format "pascalCase" PropertyName}} { get; set; } 
    = new List<{{format "pascalCase" ReferencedTable}}>();
{{else}}
public virtual {{format "pascalCase" ReferencedTable}} {{format "pascalCase" PropertyName}} { get; set; } = null!;
{{/if}}
{{/each}}
```

### Error Prevention

Common error patterns and their solutions:

1. **Missing References**:
```csharp
// Add package reference check in template
{{#layout}}
  {{#region "package_check"}}
    #if !EZDBSCHEMA_CORE_REFERENCE
    #error Please add a reference to EzDbSchema.Core package
    #endif
  {{/region}}
{{/layout}}
```

2. **Type Resolution**:
```csharp
// Safe type resolution helper
public static string ResolveTypeName(string sqlType, bool isNullable)
{
    var baseType = sqlType.Split('(')[0].ToLowerInvariant();
    var csharpType = baseType switch
    {
        "nvarchar" or "varchar" or "char" => "string",
        "int" => "int",
        "bigint" => "long",
        "bit" => "bool",
        "decimal" or "money" => "decimal",
        "datetime" or "datetime2" => "DateTime",
        "uniqueidentifier" => "Guid",
        _ => "object"
    };
    
    return csharpType == "string" ? csharpType : isNullable ? $"{csharpType}?" : csharpType;
}
```

3. **Null Checking**:
```handlebars
{{#each Properties}}
{{#if (and DataType (not IsNullable))}}
    [Required]
    public {{convertType DataType "csharp" false}} {{format "pascalCase" Name}} { get; set; }
{{else}}
    public {{convertType DataType "csharp" true}}? {{format "pascalCase" Name}} { get; set; }
{{/if}}
{{/each}}
```

## EzDbCodeGen Core Integration

### Common Integration Issues

1. **Handling Custom Attributes**:
```csharp
// Template pattern for accessing custom attributes safely
{{#each Properties}}
    {{#if CustomAttributes}}
    /// <summary>
    /// {{lookup CustomAttributes "Description"}}
    /// </summary>
    {{#if (lookup CustomAttributes "Deprecated")}}
    [Obsolete("{{lookup CustomAttributes "Deprecated"}}")]
    {{/if}}
    public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" Name}} { get; set; }
    {{/if}}
{{/each}}
```

2. **Property Access Extensions**:
```handlebars
{{#layout}}
  {{#region "extensions"}}
    public static class PropertyExtensions
    {
        public static T GetAttributeValue<T>(this KeyValuePair<string, IProperty> property, string key, T defaultValue = default)
        {
            if (property.Value?.CustomAttributes == null) return defaultValue;
            return property.Value.CustomAttributes.TryGetValue(key, out var value) 
                ? (T)value 
                : defaultValue;
        }
    }
  {{/region}}
{{/layout}}
```

3. **JSON Serialization Conflicts**:
```handlebars
{{#layout}}
  {{#region "header"}}
    #if NET8_0_OR_GREATER
    using System.Text.Json;
    using System.Text.Json.Serialization;
    #else
    using Newtonsoft.Json;
    #endif
  {{/region}}

  {{#region "class"}}
    #if NET8_0_OR_GREATER
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    #else
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    #endif
    public class {{format "pascalCase" table.Name}}
    {
        {{#each Properties}}
        #if NET8_0_OR_GREATER
        [JsonPropertyName("{{format "camelCase" Name}}")]
        #else
        [JsonProperty("{{format "camelCase" Name}}")]
        #endif
        public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" Name}} { get; set; }
        {{/each}}
    }
  {{/region}}
{{/layout}}
```

### Namespace Resolution

When dealing with namespace conflicts:

```handlebars
{{#layout}}
  {{#region "header"}}
    using CoreExtensions = EzDbSchema.Core.Extensions;
    using LocalExtensions = YourNamespace.Extensions;
  {{/region}}

  {{#region "implementation"}}
    public class {{format "pascalCase" table.Name}}Repository
    {
        public void ProcessProperty(IProperty property)
        {
            var coreValue = CoreExtensions.PropertyExtensions.GetValue(property);
            var localValue = LocalExtensions.PropertyExtensions.GetValue(property);
        }
    }
  {{/region}}
{{/layout}}
```

### Schema Version Compatibility

Template patterns for handling different schema versions:

```handlebars
{{#layout}}
  {{#region "version_check"}}
    {{#if (gt SchemaVersion "8.4.0")}}
    // New schema features
    public required string NewFeature { get; set; }
    {{else}}
    // Legacy compatibility
    public string NewFeature { get; set; } = null!;
    {{/if}}
  {{/region}}
{{/layout}}
```

### Best Practices for Schema Integration

1. **Version Handling**:
   ```csharp
   // Check schema version in template
   var schemaVersion = schema.Version ?? "8.4.0";
   var useNewFeatures = Version.Parse(schemaVersion) >= Version.Parse("8.4.1");
   ```

2. **Property Type Safety**:
   ```csharp
   // Safe property type conversion
   public static string GetSafeTypeName(IProperty property)
   {
       if (string.IsNullOrEmpty(property?.DataType))
           return "object";
       
       return property.DataType.ToLowerInvariant() switch
       {
           "nvarchar" or "varchar" or "char" => "string",
           "int" or "bigint" => "int",
           "bit" => "bool",
           "datetime" or "datetime2" => "DateTime",
           _ => "object"
       };
   }
   ```

3. **Relationship Handling**:
```handlebars
{{#each Relationships}}
{{#if (eq Multiplicity "OneToMany")}}
public virtual ICollection<{{format "pascalCase" ReferencedTable}}> {{format "pascalCase" PropertyName}} { get; set; } 
    = new List<{{format "pascalCase" ReferencedTable}}>();
{{else}}
public virtual {{format "pascalCase" ReferencedTable}} {{format "pascalCase" PropertyName}} { get; set; } = null!;
{{/if}}
{{/each}}
```

### Error Prevention

Common error patterns and their solutions:

1. **Missing References**:
```csharp
// Add package reference check in template
{{#layout}}
  {{#region "package_check"}}
    #if !EZDBSCHEMA_CORE_REFERENCE
    #error Please add a reference to EzDbSchema.Core package
    #endif
  {{/region}}
{{/layout}}
```

2. **Type Resolution**:
```csharp
// Safe type resolution helper
public static string ResolveTypeName(string sqlType, bool isNullable)
{
    var baseType = sqlType.Split('(')[0].ToLowerInvariant();
    var csharpType = baseType switch
    {
        "nvarchar" or "varchar" or "char" => "string",
        "int" => "int",
        "bigint" => "long",
        "bit" => "bool",
        "decimal" or "money" => "decimal",
        "datetime" or "datetime2" => "DateTime",
        "uniqueidentifier" => "Guid",
        _ => "object"
    };
    
    return csharpType == "string" ? csharpType : isNullable ? $"{csharpType}?" : csharpType;
}
```

3. **Null Checking**:
```handlebars
{{#each Properties}}
{{#if (and DataType (not IsNullable))}}
    [Required]
    public {{convertType DataType "csharp" false}} {{format "pascalCase" Name}} { get; set; }
{{else}}
    public {{convertType DataType "csharp" true}}? {{format "pascalCase" Name}} { get; set; }
{{/if}}
{{/each}}
