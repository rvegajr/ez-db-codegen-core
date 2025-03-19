# EzDbCodeGen Templating Helpers

This document provides usage examples and documentation for the templating helpers available in the EzDbCodeGen library.

## Table of Contents

- [FormatEz](#formatez) - String formatting operations
- [ConvertTypeEz](#converttypeez) - Data type conversion
- [DocEz](#docez) - Documentation generation
- [LayoutEz](#layoutez) - Template structure management
- [CodeFormatEz](#codeformatez) - Code formatting
- [.NET 8.0 Considerations](#net-8-0-considerations) - EzDbCodeGen and .NET 8.0
- [EzDbSchema.Core Integration](#ezdbschema-core-integration) - Integrating EzDbCodeGen with EzDbSchema.Core

## FormatEz

The `FormatEz` helper provides string formatting capabilities.

### Usage

```handlebars
{{FormatEz "operation" value}}
{{format "operation" value}} <!-- Alias -->
```

### Operations

- **Case Transformations**
  - `camelCase`: Converts to camelCase
  - `pascalCase`: Converts to PascalCase
  - `snakeCase`: Converts to snake_case
  - `kebabCase`: Converts to kebab-case
  - `lowercase`: Converts to lowercase
  - `uppercase`: Converts to UPPERCASE

- **Indentation**
  - `indent`: Indents text with 4 spaces
  - `indent2`, `indent4`, etc.: Indents text with specified number of spaces

- **Tabbing**
  - `tab`: Adds a tab character
  - `tab2`, `tab3`, etc.: Adds multiple tab characters

- **Trimming**
  - `trim`: Removes whitespace from both ends
  - `trimStart`: Removes whitespace from the start
  - `trimEnd`: Removes whitespace from the end

### Chaining Operations

Operations can be chained using commas or pipes:

```handlebars
{{FormatEz "camelCase,trim" value}}
{{format "pascalCase|indent4" value}}
```

### Examples

```handlebars
{{FormatEz "camelCase" "user_name"}} <!-- Result: "userName" -->
{{format "pascalCase" "user_name"}} <!-- Result: "UserName" -->
{{FormatEz "snakeCase" "UserName"}} <!-- Result: "user_name" -->
{{format "kebabCase" "UserName"}} <!-- Result: "user-name" -->
{{FormatEz "indent4" "Hello\nWorld"}} <!-- Result: "    Hello\n    World" -->
{{format "camelCase,trim" " user_name "}} <!-- Result: "userName" -->
```

## ConvertTypeEz

The `ConvertTypeEz` helper converts SQL data types to their corresponding types in various programming languages.

### Usage

```handlebars
{{ConvertTypeEz sqlType targetLanguage isNullable}}
{{convertType sqlType targetLanguage isNullable}} <!-- Alias -->
```

### Supported Languages

- `csharp`: C# types
- `typescript`: TypeScript types
- `java`: Java types
- `python`: Python types

### Examples

```handlebars
{{ConvertTypeEz "int" "csharp" false}} <!-- Result: "int" -->
{{convertType "int" "csharp" true}} <!-- Result: "int?" -->
{{ConvertTypeEz "nvarchar(50)" "typescript" false}} <!-- Result: "string" -->
{{convertType "bit" "java" false}} <!-- Result: "Boolean" -->
{{ConvertTypeEz "datetime" "python" false}} <!-- Result: "datetime" -->
```

## DocEz

The `DocEz` helper generates documentation comments for properties and relationships.

### Usage

```handlebars
{{DocEz object format}}
{{doc object format}} <!-- Alias -->
```

### Supported Formats

- `xml`: XML documentation format (for C#, etc.)
- `jsdoc`: JSDoc format (for JavaScript/TypeScript)

### Examples

```handlebars
{{DocEz property "xml"}}
<!-- Result:
/// <summary>
/// The CustomerId property
/// </summary>
/// <remarks>Primary key</remarks>
-->

{{doc property "jsdoc"}}
<!-- Result:
/**
 * The CustomerId property
 * @type {number}
 * @primaryKey
 */
-->

{{DocEz relationship "xml"}}
<!-- Result:
/// <summary>
/// Relationship between Orders and Customers
/// </summary>
/// <remarks>Many-to-One relationship (FK_Orders_Customers)</remarks>
-->
```

## LayoutEz

The `LayoutEz` helper manages template structure through named regions.

### Usage

```handlebars
{{#LayoutEz "region" "regionName"}}
  content
{{/LayoutEz}}

{{RenderRegion "regionName"}}
{{RenderAllRegions}}
{{SetRegionOrder "region1" "region2" "region3"}}
{{ClearRegion "regionName"}}
```

### Examples

```handlebars
{{#LayoutEz "region" "imports"}}
using System;
using System.Collections.Generic;
{{/LayoutEz}}

{{#LayoutEz "region" "classDefinition"}}
public class Customer
{
    public int Id { get; set; }
}
{{/LayoutEz}}

{{SetRegionOrder "imports" "classDefinition"}}

{{RenderAllRegions}}
<!-- Result:
using System;
using System.Collections.Generic;

public class Customer
{
    public int Id { get; set; }
}
-->
```

## CodeFormatEz

The `CodeFormatEz` helper formats code according to language-specific conventions.

### Usage

```handlebars
{{CodeFormatEz language code}}
{{formatCode language code}} <!-- Alias -->
```

### Supported Languages

- `csharp` (or `cs`, `c#`): C# code
- `sql`: SQL code
- `typescript` (or `ts`): TypeScript code
- `javascript` (or `js`): JavaScript code
- `html`: HTML code
- `css`: CSS code
- `java`: Java code
- `python` (or `py`): Python code

### Examples

```handlebars
{{CodeFormatEz "csharp" "public class Customer{public int Id{get;set;}}"}}
<!-- Result:
public class Customer {
    public int Id { get; set; }
}
-->

{{formatCode "sql" "SELECT id,name FROM customers WHERE id=1"}}
<!-- Result:
SELECT id,name
FROM customers
WHERE id=1
-->

{{CodeFormatEz "html" "<div><p>Hello</p><p>World</p></div>"}}
<!-- Result:
<div>
  <p>Hello</p>
  <p>World</p>
</div>
-->
```

## .NET 8.0 Considerations

When using these helpers with .NET 8.0:

```handlebars
{{#each Properties}}
{{#if IsNullable}}
[Required(AllowEmptyStrings = true)]
public {{convertType DataType "csharp" true}} {{format "pascalCase" Name}} { get; set; } = null!;
{{else}}
[Required]
public {{convertType DataType "csharp" false}} {{format "pascalCase" Name}} { get; set; }
{{/if}}
{{/each}}
```

## EzDbSchema.Core Integration

### Custom Attributes

```handlebars
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

### Property Access Extensions

```csharp
public static T GetAttributeValue<T>(this KeyValuePair<string, IProperty> property, string key, T defaultValue = default)
{
    if (property.Value?.CustomAttributes == null) return defaultValue;
    return property.Value.CustomAttributes.TryGetValue(key, out var value) 
        ? (T)value 
        : defaultValue;
}
```

### JSON Serialization

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

### Schema Version Compatibility

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

### Best Practices

1. **Version Handling**:
   ```csharp
   var schemaVersion = schema.Version ?? "8.4.0";
   var useNewFeatures = Version.Parse(schemaVersion) >= Version.Parse("8.4.1");
   ```

2. **Property Type Safety**:
   ```csharp
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

1. **Missing References**:
```csharp
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

## Registering All Helpers

To register all helpers at once in your application:

```csharp
var handlebars = Handlebars.Create();
HandlebarsHelperRegistration.RegisterAllHelpers(handlebars);
```

This will register all the helpers described in this document with both their full names and aliases.
