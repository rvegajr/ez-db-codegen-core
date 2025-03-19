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

## HandleBar Custom Functions

* `{{ ContextAsJson }}` - Will dump the current context as a JSON file on the rendered file, useful for debugging
* `{{ Prefix $p1 }}` - Will append a string to the beginning of the string passed through $p1
* `{{ ExtractTableName $p1 }}` - Used the extract the table name from a schema.table object name format
* `{{ ExtractSchemaName $p1 }}` - Used the extract the schema name from a schema.table object name format
* `{{ ToSingular $p1 }}` - Will change $p1 to a singular word
* `{{ Comma }}` - Will output a comma 
* `{{ ToPlural $p1 }}` - Will change $p1 to a plural word
* `{{ ToNetType $p1 }}` - Assuming that the string is a sql type, it will return the corresponding .net type with a ? if the property is nullable
* `{{ ToCodeFriendly $p1 }}` - Will write a string removing code unfriendly characters
* `{{ PropertyNameSuffix $p1 }}` - Will output a code friendly string 
* `{{ ToJsType $p1 }}` - Assuming that the string is a sql type, it will return the corresponding javascript type appending "| null" if it is nullable
* `{{ AsFormattedName $p1 }}` - Strips ID, UID, or id from $p1 
* `{{ ToSnakeCase $p1 }}` - Will turn $p1 into snake case
* `{{ ToSingularSnakeCase $p1 }}` - Will turn $p1 into snake case singular
* `{{ ToTitleCaseSafeFileName $p1 }}` - Will turn $p1 into Title case and safe for a file name (excellent for the <FILE/> clause)
* `{{ ToCsObjectName $p1 }}` - Will convert $p1 to a string suitable for C# Code name
* `{{ StringFormat $p1 $p2 }}` - Versatile string function that lets you apply 1 or more formatting tasks on $p1, $p2 can contain one more of 'lower,upper,snake,title,pascal,trim,plural,single,nettype,jstype', performed in order 
* `{{ EntityCustomAttribute $p1 }}` - Upper string and replacing "US_" to ""
* `{{ IfPropertyExists $p1 }}` - Will search the parent context to see if the entity name exists, will only write the code after to {{/IfPropertyExists}} if true
* `{{ eq $p1 $p2 }}` - Compares $p1 and $p2 for equality, useful in conditional blocks

## Recent Changes

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