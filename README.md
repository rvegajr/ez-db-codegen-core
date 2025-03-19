# ez-db-codegen-cli

EzDbCodeGen now works as a local tool.  
Easy code generation based on a database schema given by [EZDbSchema](https://github.com/rvegajr/ez-db-schema-core). The template language this application uses is HandleBars. 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system. This nuget package will dump the published cli package for code generation and a powershell script to run it. The nuget package will dump everything you need for code generation into the project you have selected under the EzDbCodeGen folder.    

### Prerequisites
* [Net 8.0+](https://www.microsoft.com/net/learn/get-started) - You will get everything you need except the sdk! Please download the latest version of this before trying to run the powershell script
* You will need MSSQL with some database installed. If you need a sample database, feel free to look for the [World Wide Importers](https://github.com/Microsoft/sql-server-samples/releases/tag/wide-world-importers-v1.0) samples.

NOTE: If you have not set your powershell execution remote policy first, you will need to do this as noted in [Powershell Execution Policy](https://www.pdq.com/blog/powershell-how-to-write-your-first-powershell-script/)
* Open the powershell command prompt in administrator mode and type:
Set-ExecutionPolicy RemoteSigned

### Using this project:

1. Navigate to an empty directory where you want to install this tool at.
2. Using the command line: `dotnet new tool-manifest`
3. Once this has completed:  
   `dotnet tool install EzDbCodeGen.Cli --interactive`  
   (or to update: `dotnet tool update EzDbCodeGen.Cli --interactive`)
4. You will need a database that you can run the sample templates against. This utility will build the connection string, test it, download the sample files from the nuget library, copy them to the proper location, then perform the code generation as instructed by the templates.  
   `dotnet ezdbcg` 

## CLI Options

The EzDbCodeGen CLI tool (`ezdbcg`) supports the following command-line options:

```
Usage: ezdbcg [options]

Options:
  -a|--app-name <NAME>           Application name (default: "MyApp")
  -s|--schema-name <NAME>        Schema name (default: "MySchema")
  -t|--template <PATH>           Template file or directory path
  -c|--connection-string <CS>    Database connection string
  -v|--verbose                   Enable verbose output
  -o|--output <PATH>             Output directory path
  --save-settings                Save current settings for future use
  -?|-h|--help                   Show help information
```

### Examples

**Basic usage with prompts:**
```bash
dotnet ezdbcg
```
This will prompt you for any required information that isn't provided.

**Generate code with all parameters:**
```bash
dotnet ezdbcg -a MyApplication -s dbo -t ./Templates/Entity.hbs -c "Server=localhost;Database=MyDB;Trusted_Connection=True;" -o ./Output -v
```

**Save settings for future use:**
```bash
dotnet ezdbcg -a MyApplication -s dbo -c "Server=localhost;Database=MyDB;Trusted_Connection=True;" --save-settings
```

### In-Memory Testing

For testing purposes, EzDbCodeGen supports an in-memory mode that doesn't require a real database connection. This is particularly useful for unit tests and CI/CD pipelines.

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

### Using with EF Core

EzDbCodeGen now supports Entity Framework Core code generation with improved templates and handling of entity relationships. You can generate EF Core models and controllers with proper navigation properties and relationship handling.

Example usage:

```csharp
// Create a configuration
var configuration = new Configuration();
configuration.SetValue("Namespace", "MyApp");

// Set up entity security (optional)
var entitySecurity = new Dictionary<string, EntitySecurity>
{
    { "Customer", new EntitySecurity { Secured = true } }
};
configuration.SetValue("EntitySecurity", entitySecurity);

// Create a code generator
var codeGenerator = new CodeGenerator(configuration);

// Process templates
codeGenerator.ProcessModelTemplate("Templates/EFCoreModel.hbs", templateDataInput, outputPath);
codeGenerator.ProcessControllerTemplate("Templates/EFCoreController.hbs", templateDataInput, outputPath);
```

## Core Interfaces

EzDbCodeGen is built around the core interfaces from [EzDbSchema](https://github.com/rvegajr/ez-db-schema-core). Here are the key interfaces you'll work with:

```csharp
// Root interface for database schema
public interface IDatabase
{
    string Name { get; }
    string DefaultSchema { get; }
    IEntityDictionary<string, IEntity> Entities { get; }
}

// Represents a database table or view
public interface IEntity
{
    string TableName { get; }
    string TableAlias { get; }
    string DatabaseSchema { get; }
    IPropertyDictionary<string, IProperty> Properties { get; }
    IRelationshipReferenceList Relationships { get; }
}

// Represents a database column
public interface IProperty
{
    string PropertyName { get; }
    string ColumnName { get; }
    string DataType { get; }
    bool IsNullable { get; }
    bool IsPrimaryKey { get; }
    bool IsIdentity { get; }
}

// Represents table relationships
public interface IRelationship
{
    string ConstraintName { get; }
    string FromTableName { get; }
    string ToTableName { get; }
    RelationshipMultiplicityType MultiplicityType { get; }
    IEntity FromEntity { get; }
    IEntity ToEntity { get; }
}
```

### Using Interfaces in Templates

Your Handlebars templates can access these interfaces directly. Here are some common patterns:

```handlebars
{{#each Entities}}
public class {{format "pascalCase" TableName}}
{
    {{#each Properties}}
    {{#if IsPrimaryKey}}[Key]{{/if}}
    {{#if IsRequired}}[Required]{{/if}}
    public {{convertType DataType "csharp" IsNullable}} {{format "pascalCase" PropertyName}} { get; set; }
    {{/each}}

    {{#each Relationships}}
    {{#if (eq MultiplicityType "OneToMany")}}
    public virtual ICollection<{{format "pascalCase" ToTableName}}> {{format "pascalCase" ToTableName}} { get; set; }
        = new List<{{format "pascalCase" ToTableName}}>();
    {{else}}
    public virtual {{format "pascalCase" ToTableName}} {{format "pascalCase" ToTableName}} { get; set; }
    {{/if}}
    {{/each}}
}
{{/each}}
```

For more detailed examples and patterns, see [AI_USAGE.md](AI_USAGE.md).

## Templating Capabilities

EzDbCodeGen Core is a powerful code generation library that leverages Handlebars templates to generate code from database schemas.

### Features

- **Database Schema Support**: Works with EzDbSchema to read and analyze database structures
- **Handlebars Templating**: Rich set of custom helpers for code generation
- **Multi-Language Support**: Generate code for C#, TypeScript, Java, Python, and more
- **Customizable Output**: Control formatting and layout of generated code

### Templating Helpers

#### FormatEz
String formatting helper with multiple operations:
```handlebars
{{format "camelCase,uppercase" propertyName}}
{{format "snakeCase,tab1" className}}
```

Supported operations:
- Case transformations: camelCase, pascalCase, snakeCase, kebabCase, uppercase, lowercase
- Indentation: indent2, indent4 (default), indent8
- Tabbing: tab1, tab2, tab4

#### ConvertTypeEz
Data type conversion helper:
```handlebars
{{convertType sqlType "csharp" nullable}}
{{convertType "varchar(50)" "typescript"}}
```

Supported target languages:
- C# (csharp)
- TypeScript
- JavaScript
- Java
- Python

#### DocEz
Documentation generator for properties and relationships:
```handlebars
{{doc property "xml"}}
{{doc relationship "jsdoc"}}
```

Supported formats:
- XML (for C#)
- JSDoc (for TypeScript/JavaScript)
- JavaDoc (for Java)
- Docstring (for Python)

#### LayoutEz
Template structure manager:
```handlebars
{{#layout}}
  {{#region "header"}}
    // Auto-generated code
  {{/region}}
  {{#region "body"}}
    public class {{className}} {
      {{> properties}}
    }
  {{/region}}
{{/layout}}
```

#### CodeFormatEz
Code formatter for multiple languages:
```handlebars
{{formatCode "csharp" codeString}}
{{formatCode "sql" queryString}}
```

Supported languages:
- C#
- SQL
- TypeScript/JavaScript
- HTML
- CSS
- Java
- Python

## AI Support

This project includes AI support files to help AI assistants understand and work with the codebase:

- `AI_INDEX.md`: An index of the project structure and key components
- `AI_USAGE.md`: Detailed examples and usage patterns for AI assistants

## Deployment

This project was designed to be hosted and distributed with nuget.com.

## Built With

* [.NET 8.0](https://www.microsoft.com/net/learn/get-started) - The framework used
* [Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net) - Template engine
* [EzDbSchema](https://github.com/rvegajr/ez-db-schema-core) - Database schema provider

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/rvegajr/651875c08acb76009e563db128f33e7e) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/rvegajr/tags). 

## Authors

* **Ricky Vega** - *Initial work* - [Noctusoft](https://github.com/rvegajr)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

Many thanks to the following projects that have helped in this project
* EzDBSchema 
* McMaster.Extensions.CommandLineUtils
* Handlebars.Net

## HandleBar Custom Function Guide

EzDbCodeGen provides a rich set of Handlebars helpers to make template creation easier and more powerful. This guide documents all available custom functions to help you create effective templates.

### Basic Helpers

| Helper | Description | Example |
|--------|-------------|---------|
| `{{Debugger}}` | Adds a debug point (useful for development) | `{{Debugger "Checking value"}}` |
| `{{Comma}}` | Outputs a comma (useful in template loops) | `{{#each Properties}}{{Name}}{{#unless @last}}{{Comma}}{{/unless}}{{/each}}` |
| `{{ToJson value}}` | Converts a value to JSON format | `{{ToJson Entity}}` |
| `{{ContextAsJson}}` | Dumps the current context as JSON | `{{ContextAsJson true}}` (true for indented) |

### Type Conversion Helpers

| Helper | Description | Example |
|--------|-------------|---------|
| `{{ToNetType}}` | Converts database type to .NET type | `{{ToNetType DataType}}` |
| `{{ToJsType}}` | Converts database type to JavaScript type | `{{ToJsType DataType}}` |
| `{{AsNullableType type isNullable}}` | Adds nullable marker (?) if needed | `{{AsNullableType "int" IsNullable}}` |

### String Formatting Helpers

| Helper | Description | Example |
|--------|-------------|---------|
| `{{ToPascalCase}}` | Converts string to PascalCase | `{{ToPascalCase TableName}}` |
| `{{ToCamelCase}}` | Converts string to camelCase | `{{ToCamelCase PropertyName}}` |
| `{{ToSnakeCase}}` | Converts string to snake_case | `{{ToSnakeCase TableName}}` |
| `{{ToSingular}}` | Converts plural to singular | `{{ToSingular TableName}}` |
| `{{ToPlural}}` | Converts singular to plural | `{{ToPlural EntityName}}` |
| `{{ToCodeFriendly}}` | Makes string code-friendly | `{{ToCodeFriendly RawName}}` |
| `{{ExtractTableName}}` | Extracts table name from schema.table | `{{ExtractTableName "dbo.Customer"}}` |
| `{{ExtractSchemaName}}` | Extracts schema name from schema.table | `{{ExtractSchemaName "dbo.Customer"}}` |
| `{{AsFormattedName}}` | Formats name by removing common suffixes | `{{AsFormattedName TableName}}` |
| `{{Prefix prefix value}}` | Adds prefix to string | `{{Prefix "I" EntityName}}` |
| `{{StringFormat value format1 format2...}}` | Applies multiple string formats | `{{StringFormat TableName "pascal" "singular"}}` |

### Conditional Helpers

| Helper | Description | Example |
|--------|-------------|---------|
| `{{#IfPropertyExists propertyName}}` | Checks if property exists | `{{#IfPropertyExists "IsNullable"}}...{{else}}...{{/IfPropertyExists}}` |
| `{{#IfFunctionExists functionName}}` | Checks if function exists | `{{#IfFunctionExists "GetRelationships"}}...{{/IfFunctionExists}}` |
| `{{#IfCond left op right}}` | Conditional logic with operators | `{{#IfCond Value "==" "string"}}...{{else}}...{{/IfCond}}` |
| `{{#Switch value}}{{#Case 'option'}}...{{/Case}}{{/Switch}}` | Switch/case logic | `{{#Switch DataType}}{{#Case "int"}}Integer{{/Case}}{{#Default}}String{{/Default}}{{/Switch}}` |

### TypeScript Helpers

| Helper | Description | Example |
|--------|-------------|---------|
| `{{tsType}}` | Converts to TypeScript type | `{{tsType DataType}}` |
| `{{tsInterface}}` | Formats name as interface | `{{tsInterface TableName}}` |
| `{{tsModel}}` | Formats name as model | `{{tsModel TableName}}` |
| `{{tsService}}` | Formats name as service | `{{tsService TableName}}` |
| `{{tsComponent}}` | Formats name as component | `{{tsComponent TableName}}` |
| `{{tsModule}}` | Formats name as module | `{{tsModule TableName}}` |
| `{{tsRouting}}` | Formats name as routing | `{{tsRouting TableName}}` |
| `{{tsStore}}` | Formats name as store | `{{tsStore TableName}}` |

### Foreign Key Helpers

| Helper | Description | Example |
|--------|-------------|---------|
| `{{POCOModelFKProperties}}` | Generates POCO model foreign key properties | `{{POCOModelFKProperties "    "}}` |
| `{{POCOModelFKManyToZeroToOne}}` | Generates POCO model many-to-zero-or-one relationships | `{{POCOModelFKManyToZeroToOne "    "}}` |
| `{{GetForeignKeyProperties}}` | Gets foreign key properties for an entity | `{{#each (GetForeignKeyProperties Entity)}}...{{/each}}` |
| `{{GetOneToOneReferences}}` | Gets one-to-one references | `{{#each (GetOneToOneReferences Entity)}}...{{/each}}` |
| `{{GetForeignKeyCollectionsForEntity}}` | Gets foreign key collections | `{{#each (GetForeignKeyCollectionsForEntity Entity)}}...{{/each}}` |

### Template Examples

#### Entity Framework Core Model

```handlebars
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column("{{Name}}")]
        public {{AsNullableType (ToNetType DataType) IsNullable}} {{ToPascalCase Name}} { get; set; }
        {{/each}}

        {{POCOModelFKProperties "        "}}

        {{#each (GetForeignKeyCollectionsForEntity this)}}
        public virtual ICollection<{{RelatedEntity}}> {{NavigationProperty}} { get; set; }
        {{/each}}
    }
}
```

#### TypeScript Interface

```handlebars
export interface {{tsInterface TableName}} {
    {{#each Properties}}
    {{ToCamelCase Name}}: {{tsType DataType}}{{#if IsNullable}} | null{{/if}};
    {{/each}}
    
    {{#each (GetOneToOneReferences this)}}
    {{ToCamelCase NavigationProperty}}?: {{tsInterface RelatedEntity}};
    {{/each}}
    
    {{#each (GetForeignKeyCollectionsForEntity this)}}
    {{ToCamelCase NavigationProperty}}?: {{tsInterface RelatedEntity}}[];
    {{/each}}
}
```

For more detailed examples and usage patterns, refer to the [AI_USAGE.md](AI_USAGE.md) file.

## Recent Changes

### 8.5.0 (March 2025)
- Major refactoring with improved code organization
- Enhanced dotnet tool installation and usage experience
- Consolidated projects into fewer, more focused components
- Fixed all test issues, particularly in TestableCodeGenerator class
- Added proper Handlebars helpers for security and debugging
- Implemented null-safe handling throughout the codebase
- Added new templates in a dedicated Templates directory
- Improved documentation for dotnet tool usage

### 8.4.2 (March 2025)
- Fixed EF Core code generation tests and implementation
- Added entity name normalization for consistent casing
- Improved entity security handling
- Added comprehensive debug logging
- Enhanced template processing with Handlebars
- Added AI support files for better integration with AI assistants
- Updated documentation with EF Core examples

### 8.4.1 (February 2025)
- Added support for .NET 8.0
- Fixed issues with ambiguous references between Newtonsoft.Json and System.Text.Json
- Addressed namespace issues with EzDbSchema.Core.Extensions vs EzDbSchema.Core.Extentions
- Fixed null reference warnings due to nullable reference types being enabled
- Improved handling of property access in KeyValuePair<string, IProperty>

### 8.4.0 (January 2025)
- Updated to .NET 8.0
- Added configuration string fixes
- Updated package dependencies:
  - EzDbSchema to 8.4.1
  - EzDbSchema.MsSql to 8.4.0
  - Handlebars.Net to 2.1.4
  - Newtonsoft.Json to 13.0.3