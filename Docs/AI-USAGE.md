# AI Usage Guide for EzDbCodeGen Template Generation

## Quick Start for AI Systems

### Step 1: Initial Analysis
```csharp
// When provided with a folder path, first analyze its structure
var folderPath = "/path/to/source";
var analysis = new {
    FileTypes = new[] { ".cs", ".json", ".xml", ".md" },
    Patterns = new[] {
        "namespace *.{name}",
        "public class {name}",
        "<Project Sdk=\"{sdk}\">",
    },
    SpecialFiles = new[] {
        ".gitignore",
        "README.md",
        "*.csproj"
    }
};
```

### Step 2: Template Creation Strategy
1. Create base structure template
2. Create file-type specific templates
3. Create special file templates
4. Create dynamic content templates
5. Validate and test templates

### Step 3: Implementation Example
```handlebars
{{!-- root_template.hbs --}}
{{#with RootDirectory}}
  {{> directory_structure}}
  {{> special_files}}
  {{> documentation}}
{{/with}}

{{!-- Implement each partial with specific logic --}}
```

## Overview
This document provides instructions for AI systems to generate Handlebars templates that can recreate specific folder structures and their contents. The templates will maintain file hierarchy, content, and metadata while supporting dynamic replacements.

## Detailed Template Generation Process

### 1. Advanced Folder Analysis

#### 1.1 Directory Structure Analysis
```csharp
public class DirectoryAnalysis {
    public Dictionary<string, int> FileTypeCount { get; set; }
    public Dictionary<string, string[]> CommonPatterns { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public List<string> IgnoredPaths { get; set; }
}
```

#### 1.2 Pattern Recognition
```csharp
public class PatternRecognition {
    // File naming patterns
    public static readonly string[] CommonPrefixes = { "I", "Abstract", "Base", "Custom" };
    public static readonly string[] CommonSuffixes = { "Controller", "Service", "Repository", "Factory" };
    
    // Content patterns
    public static readonly Dictionary<string, string[]> FileTypePatterns = new() {
        [".cs"] = new[] { 
            "namespace", 
            "using", 
            "public class",
            "private readonly"
        },
        [".xml"] = new[] {
            "<?xml",
            "<Project",
            "<PropertyGroup",
            "<ItemGroup"
        }
    };
}
```

#### 1.3 Metadata Extraction
```csharp
public class FileMetadata {
    public string Encoding { get; set; }
    public string LineEndings { get; set; }
    public string IndentationType { get; set; }
    public int IndentationSize { get; set; }
    public Dictionary<string, string> CustomProperties { get; set; }
}
```

### 1. Folder Analysis
When provided with a folder path, analyze:
- Directory structure
- File types and extensions
- File content patterns
- Naming conventions
- Special files (e.g., .gitignore, README.md)

### 2. Comprehensive Template Structure

#### 2.1 Base Template System
```handlebars
{{!-- base_template.hbs --}}
{
  "version": "1.0.0",
  "generator": "EzDbCodeGen",
  "timestamp": "{{timestamp}}",
  "structure": {
    "root": "{{RootDirectory.Path}}",
    "directories": [
      {{#each Directories}}
        {{> directory_entry}}
      {{/each}}
    ],
    "files": [
      {{#each Files}}
        {{> file_entry}}
      {{/each}}
    ]
  }
}
```

#### 2.2 Directory Template System
```handlebars
{{!-- directory_entry.hbs --}}
{
  "name": "{{Name}}",
  "path": "{{RelativePath}}",
  "type": "directory",
  "metadata": {
    "created": "{{Created}}",
    "modified": "{{Modified}}",
    "attributes": [{{#each Attributes}}"{{this}}"{{#unless @last}},{{/unless}}{{/each}}]
  },
  "contents": [
    {{#each Files}}
      {{> file_entry}}
    {{/each}}
    {{#each Subdirectories}}
      {{> directory_entry}}
    {{/each}}
  ]
}
```

#### 2.3 File Template System
```handlebars
{{!-- file_entry.hbs --}}
{
  "name": "{{Name}}",
  "path": "{{RelativePath}}",
  "type": "file",
  "extension": "{{Extension}}",
  "metadata": {
    "created": "{{Created}}",
    "modified": "{{Modified}}",
    "encoding": "{{Encoding}}",
    "size": {{Size}},
    "attributes": [{{#each Attributes}}"{{this}}"{{#unless @last}},{{/unless}}{{/each}}]
  },
  "content": {
    "template": "{{> (lookup_template Extension)}}",
    "variables": {
      {{#each Variables}}
        "{{@key}}": "{{this}}"{{#unless @last}},{{/unless}}
      {{/each}}
    }
  }
}
```

#### 2.4 Content Templates by File Type
```handlebars
{{!-- csharp_template.hbs --}}
using System;
using System.Collections.Generic;
{{#each Usings}}
using {{this}};
{{/each}}

namespace {{Namespace}}
{
    {{#each Attributes}}
    [{{this}}]
    {{/each}}
    public class {{ClassName}}
    {
        {{#each Properties}}
        public {{Type}} {{Name}} { get; set; }
        {{/each}}

        {{#each Methods}}
        public {{ReturnType}} {{Name}}({{Parameters}})
        {
            {{Body}}
        }
        {{/each}}
    }
}

{{!-- xml_template.hbs --}}
<?xml version="1.0" encoding="{{Encoding}}"?>
<{{RootElement}} {{#each RootAttributes}}{{Name}}="{{Value}}"{{/each}}>
    {{#each Children}}
    <{{Name}} {{#each Attributes}}{{Name}}="{{Value}}"{{/each}}>
        {{Content}}
    </{{Name}}>
    {{/each}}
</{{RootElement}}>

{{!-- json_template.hbs --}}
{
  {{#each Properties}}
  "{{@key}}": {{#if (isObject this)}}{{> json_object this}}{{else}}"{{this}}"{{/if}}{{#unless @last}},{{/unless}}
  {{/each}}
}
```

#### 2.1 Directory Template
```handlebars
{{#each Directories}}
  {{#if IsRoot}}
    {{> directory_structure}}
  {{/if}}
{{/each}}
```

#### 2.2 Directory Partial
```handlebars
{{!-- directory_structure.hbs --}}
{{#each Files}}
  {{> file_content Name=Name Path=Path}}
{{/each}}
{{#each Subdirectories}}
  {{> directory_structure}}
{{/each}}
```

### 3. Comprehensive Helper System

#### 3.1 Advanced Path Helpers
```handlebars
{{!-- Path manipulation helpers --}}
{{PathCombine path1 path2}}              {{!-- Combines path segments --}}
{{NormalizePath path}}                   {{!-- Normalizes path separators --}}
{{GetRelativePath base path}}            {{!-- Gets relative path --}}
{{GetDirectoryName path}}                {{!-- Gets directory name --}}
{{GetFileName path}}                     {{!-- Gets file name --}}
{{GetFileNameWithoutExtension path}}     {{!-- Gets file name without extension --}}
{{GetExtension path}}                    {{!-- Gets file extension --}}
{{IsAbsolutePath path}}                  {{!-- Checks if path is absolute --}}
{{MakeRelativePath basePath targetPath}} {{!-- Creates relative path --}}

{{!-- Advanced path operations --}}
{{#PathExists path}}
  {{!-- Path exists logic --}}
{{/PathExists}}

{{#IsDirectory path}}
  {{!-- Directory specific logic --}}
{{/IsDirectory}}

{{#IsFile path}}
  {{!-- File specific logic --}}
{{/IsFile}}
```

#### 3.2 Enhanced Content Helpers
```handlebars
{{!-- Content manipulation helpers --}}
{{FormatCode content language}}          {{!-- Formats code with proper indentation --}}
{{MinifyCode content language}}          {{!-- Minifies code --}}
{{EscapeString content}}                {{!-- Escapes special characters --}}
{{Base64Encode content}}                {{!-- Encodes content to base64 --}}
{{Base64Decode content}}                {{!-- Decodes base64 content --}}
{{StripComments content language}}       {{!-- Removes comments --}}
{{NormalizeLineEndings content}}        {{!-- Normalizes line endings --}}

{{!-- Content analysis helpers --}}
{{#DetectLanguage content}}
  {{!-- Language specific handling --}}
{{/DetectLanguage}}

{{#HasPattern content pattern}}
  {{!-- Pattern matching logic --}}
{{/HasPattern}}

{{#ContainsUsing content namespace}}
  {{!-- Namespace usage detection --}}
{{/ContainsUsing}}
```

#### 3.3 Advanced Naming Helpers
```handlebars
{{!-- Case conversion helpers --}}
{{PascalCase name}}                     {{!-- Converts to PascalCase --}}
{{CamelCase name}}                      {{!-- Converts to camelCase --}}
{{SnakeCase name}}                      {{!-- Converts to snake_case --}}
{{KebabCase name}}                      {{!-- Converts to kebab-case --}}
{{ConstantCase name}}                   {{!-- Converts to CONSTANT_CASE --}}

{{!-- Name manipulation helpers --}}
{{Pluralize name}}                      {{!-- Pluralizes a name --}}
{{Singularize name}}                    {{!-- Singularizes a name --}}
{{Humanize name}}                       {{!-- Makes name human-readable --}}
{{Titleize name}}                       {{!-- Converts to Title Case --}}
{{Underscore name}}                     {{!-- Adds underscores --}}
{{Dasherize name}}                      {{!-- Adds dashes --}}
{{Camelize name}}                       {{!-- Camelizes a name --}}
{{Classify name}}                       {{!-- Creates a class name --}}
{{Demodulize name}}                     {{!-- Removes module names --}}
{{Tableize name}}                       {{!-- Creates table name --}}

{{!-- Advanced naming helpers --}}
{{#IsValidIdentifier name}}
  {{!-- Identifier validation logic --}}
{{/IsValidIdentifier}}

{{#SuggestName type name}}
  {{!-- Name suggestion logic --}}
{{/SuggestName}}
```

#### 3.4 Type System Helpers
```handlebars
{{!-- Type checking helpers --}}
{{#IsNumeric value}}
  {{!-- Numeric type logic --}}
{{/IsNumeric}}

{{#IsString value}}
  {{!-- String type logic --}}
{{/IsString}}

{{#IsArray value}}
  {{!-- Array type logic --}}
{{/IsArray}}

{{#IsObject value}}
  {{!-- Object type logic --}}
{{/IsObject}}

{{!-- Type conversion helpers --}}
{{ToInt value}}                         {{!-- Converts to integer --}}
{{ToFloat value}}                       {{!-- Converts to float --}}
{{ToString value}}                      {{!-- Converts to string --}}
{{ToBoolean value}}                     {{!-- Converts to boolean --}}
{{ToArray value}}                       {{!-- Converts to array --}}
{{ToObject value}}                      {{!-- Converts to object --}}

{{!-- Type manipulation helpers --}}
{{GetType value}}                       {{!-- Gets type information --}}
{{Cast value type}}                     {{!-- Casts to specified type --}}
{{Convert value fromType toType}}       {{!-- Converts between types --}}
```

#### 3.1 Path Helpers
```handlebars
{{PathCombine path1 path2}}     {{!-- Combines path segments --}}
{{NormalizePath path}}          {{!-- Normalizes path separators --}}
{{GetRelativePath base path}}   {{!-- Gets relative path --}}
```

#### 3.2 Content Helpers
```handlebars
{{#Switch FileExtension}}
  {{#Case ".cs"}}
    {{> csharp_template}}
  {{/Case}}
  {{#Case ".json"}}
    {{> json_template}}
  {{/Case}}
{{/Switch}}
```

#### 3.3 Naming Helpers
```handlebars
{{PascalCase name}}
{{CamelCase name}}
{{SnakeCase name}}
```

### 4. Comprehensive Template Context System

#### 4.1 Root Context Structure
```typescript
interface TemplateContext {
  // Basic information
  RootDirectory: DirectoryInfo;
  Metadata: MetadataInfo;
  Configuration: ConfigurationInfo;
  
  // Advanced features
  Variables: VariableCollection;
  Helpers: HelperCollection;
  Partials: PartialCollection;
  Cache: CacheManager;
  Logger: LogManager;
}

// Detailed directory information
interface DirectoryInfo {
  Path: string;
  Name: string;
  Files: FileInfo[];
  Directories: DirectoryInfo[];
  Metadata: {
    Created: Date;
    Modified: Date;
    Attributes: string[];
    Owner: string;
    Permissions: string;
  };
  Statistics: {
    TotalFiles: number;
    TotalDirectories: number;
    TotalSize: number;
    AverageFileSize: number;
    LargestFile: string;
    MostCommonExtension: string;
  };
}

// Comprehensive file information
interface FileInfo {
  Name: string;
  Path: string;
  RelativePath: string;
  Extension: string;
  Content: string;
  IsTemplate: boolean;
  Metadata: {
    Created: Date;
    Modified: Date;
    Attributes: string[];
    Owner: string;
    Permissions: string;
    Encoding: string;
    LineEndings: string;
    HasBOM: boolean;
  };
  Analysis: {
    Language: string;
    Complexity: number;
    LineCount: number;
    CharacterCount: number;
    Imports: string[];
    Dependencies: string[];
    Patterns: Pattern[];
  };
}

// Configuration management
interface ConfigurationInfo {
  IgnorePatterns: string[];
  FileExtensionHandlers: Record<string, string>;
  Placeholders: Record<string, string>;
  Settings: {
    IndentStyle: 'space' | 'tab';
    IndentSize: number;
    LineEnding: 'lf' | 'crlf';
    Encoding: string;
    MaxFileSize: number;
    CompressionEnabled: boolean;
    ValidationRules: ValidationRule[];
  };
}

// Variable management
interface VariableCollection {
  Global: Record<string, any>;
  Local: Record<string, any>;
  Environment: Record<string, string>;
  Computed: Record<string, () => any>;
  Watchers: Record<string, (newValue: any, oldValue: any) => void>;
}

// Helper management
interface HelperCollection {
  Path: PathHelpers;
  String: StringHelpers;
  Type: TypeHelpers;
  Format: FormatHelpers;
  Validation: ValidationHelpers;
  Custom: Record<string, (...args: any[]) => any>;
}

// Partial template management
interface PartialCollection {
  Registered: Record<string, string>;
  Dynamic: Record<string, (context: any) => string>;
  Cached: Record<string, string>;
  Dependencies: Record<string, string[]>;
}

// Cache management
interface CacheManager {
  Templates: Map<string, CompiledTemplate>;
  Partials: Map<string, CompiledPartial>;
  Content: Map<string, string>;
  Metadata: Map<string, object>;
  Clear(): void;
  Invalidate(key: string): void;
  Statistics: CacheStatistics;
}

// Logging system
interface LogManager {
  Info(message: string, ...args: any[]): void;
  Warn(message: string, ...args: any[]): void;
  Error(message: string, ...args: any[]): void;
  Debug(message: string, ...args: any[]): void;
  Metrics: {
    TemplateCompilations: number;
    CacheHits: number;
    CacheMisses: number;
    RenderTime: number;
  };
}
```

The following context structure is expected:

```typescript
interface TemplateContext {
  RootDirectory: {
    Path: string;
    Name: string;
    Files: FileInfo[];
    Directories: DirectoryInfo[];
  };
  Metadata: {
    CreatedDate: string;
    LastModified: string;
    Author: string;
    Version: string;
  };
  Configuration: {
    IgnorePatterns: string[];
    FileExtensionHandlers: Record<string, string>;
    Placeholders: Record<string, string>;
  };
}

interface FileInfo {
  Name: string;
  Path: string;
  RelativePath: string;
  Extension: string;
  Content: string;
  IsTemplate: boolean;
  Metadata: Record<string, string>;
}

interface DirectoryInfo {
  Name: string;
  Path: string;
  RelativePath: string;
  Files: FileInfo[];
  Directories: DirectoryInfo[];
  IsRoot: boolean;
}
```

## Comprehensive Usage Examples

### 1. Complete Project Structure Generation

#### 1.1 Solution Structure
```handlebars
{{!-- solution_template.hbs --}}
{{#with Solution}}
  {{> solution_file}}
  {{#each Projects}}
    {{> project_structure}}
  {{/each}}
{{/with}}

{{!-- solution_file.hbs --}}
Microsoft Visual Studio Solution File, Format Version 12.00
{{#each Projects}}
Project("{{{Guid}}}") = "{{{Name}}}", "{{{RelativePath}}}", "{{{ProjectGuid}}}"
EndProject
{{/each}}
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
EndGlobal
```

#### 1.2 Project Structure
```handlebars
{{!-- project_structure.hbs --}}
<Project Sdk="{{Sdk}}">
  <PropertyGroup>
    <TargetFramework>{{TargetFramework}}</TargetFramework>
    <RootNamespace>{{Namespace}}</RootNamespace>
    {{#each ProjectProperties}}
    <{{Key}}>{{Value}}</{{Key}}>
    {{/each}}
  </PropertyGroup>

  <ItemGroup>
    {{#each Dependencies}}
    <PackageReference Include="{{Name}}" Version="{{Version}}" />
    {{/each}}
  </ItemGroup>

  {{#each ItemGroups}}
  <ItemGroup>
    {{#each Items}}
    <{{Type}} Include="{{Path}}" />
    {{/each}}
  </ItemGroup>
  {{/each}}
</Project>
```

### 2. Code File Generation

#### 2.1 Class Template
```handlebars
{{!-- class_template.hbs --}}
{{#with Class}}
using System;
{{#each Usings}}
using {{this}};
{{/each}}

namespace {{Namespace}}
{
    {{#each Attributes}}
    [{{Name}}{{#if Parameters}}({{Parameters}}){{/if}}]
    {{/each}}
    {{Accessibility}} {{#if IsStatic}}static {{/if}}{{#if IsAbstract}}abstract {{/if}}class {{Name}}{{#if BaseClass}} : {{BaseClass}}{{/if}}{{#if Interfaces}} : {{#each Interfaces}}{{this}}{{#unless @last}}, {{/unless}}{{/each}}{{/if}}
    {
        {{#each Fields}}
        {{> field_template}}
        {{/each}}

        {{#each Properties}}
        {{> property_template}}
        {{/each}}

        {{#each Constructors}}
        {{> constructor_template}}
        {{/each}}

        {{#each Methods}}
        {{> method_template}}
        {{/each}}
    }
}
{{/with}}
```

#### 2.2 Interface Template
```handlebars
{{!-- interface_template.hbs --}}
{{#with Interface}}
using System;
{{#each Usings}}
using {{this}};
{{/each}}

namespace {{Namespace}}
{
    {{#each Attributes}}
    [{{Name}}{{#if Parameters}}({{Parameters}}){{/if}}]
    {{/each}}
    {{Accessibility}} interface {{Name}}{{#if BaseInterfaces}} : {{#each BaseInterfaces}}{{this}}{{#unless @last}}, {{/unless}}{{/each}}{{/if}}
    {
        {{#each Properties}}
        {{> interface_property_template}}
        {{/each}}

        {{#each Methods}}
        {{> interface_method_template}}
        {{/each}}
    }
}
{{/with}}
```

### 3. Database Schema Generation

#### 3.1 Table Creation
```handlebars
{{!-- table_template.hbs --}}
CREATE TABLE [{{Schema}}].[{{TableName}}]
(
    {{#each Columns}}
    [{{Name}}] {{SqlType}}{{#if Length}}({{Length}}){{/if}}{{#if IsNullable}} NULL{{else}} NOT NULL{{/if}}{{#if IsIdentity}} IDENTITY(1,1){{/if}}{{#unless @last}},{{/unless}}
    {{/each}}

    {{#if PrimaryKey}}
    CONSTRAINT [PK_{{TableName}}] PRIMARY KEY CLUSTERED 
    (
        {{#each PrimaryKey.Columns}}
        [{{this}}]{{#unless @last}},{{/unless}}
        {{/each}}
    )
    {{/if}}
)
```

#### 3.2 Stored Procedure Generation
```handlebars
{{!-- stored_procedure_template.hbs --}}
CREATE PROCEDURE [{{Schema}}].[{{Name}}]
    {{#each Parameters}}
    @{{Name}} {{SqlType}}{{#if Length}}({{Length}}){{/if}}{{#if DefaultValue}} = {{DefaultValue}}{{/if}}{{#unless @last}},{{/unless}}
    {{/each}}
AS
BEGIN
    SET NOCOUNT ON;

    {{#each Statements}}
    {{this}}
    {{/each}}
END
```

### 4. Configuration File Generation

#### 4.1 JSON Configuration
```handlebars
{{!-- config_template.hbs --}}
{
  "version": "{{Version}}",
  "environment": "{{Environment}}",
  "logging": {
    "level": "{{Logging.Level}}",
    "providers": [
      {{#each Logging.Providers}}
      {
        "name": "{{Name}}",
        "configuration": {{> provider_config}}
      }{{#unless @last}},{{/unless}}
      {{/each}}
    ]
  },
  "database": {
    "connectionString": "{{Database.ConnectionString}}",
    "provider": "{{Database.Provider}}",
    "migrations": {
      "enabled": {{Database.Migrations.Enabled}},
      "autoRun": {{Database.Migrations.AutoRun}}
    }
  }
}
```

#### 4.2 XML Configuration
```handlebars
{{!-- config_template.hbs --}}
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    {{#each AppSettings}}
    <add key="{{Key}}" value="{{Value}}" />
    {{/each}}
  </appSettings>
  
  <connectionStrings>
    {{#each ConnectionStrings}}
    <add name="{{Name}}" connectionString="{{Value}}" providerName="{{Provider}}" />
    {{/each}}
  </connectionStrings>
  
  <system.web>
    <authentication mode="{{Authentication.Mode}}">
      {{#if Authentication.Forms}}
      <forms loginUrl="{{Authentication.Forms.LoginUrl}}" timeout="{{Authentication.Forms.Timeout}}" />
      {{/if}}
    </authentication>
  </system.web>
</configuration>
```

### 1. Generate Project Structure
```handlebars
{{!-- project_structure.hbs --}}
{{#with RootDirectory}}
  # {{Name}}

  ## Directory Structure
  {{#each Directories}}
    - {{Name}}/
    {{#each Files}}
      - {{RelativePath}}
    {{/each}}
  {{/each}}

  ## Files
  {{#each Files}}
    ### {{Name}}
    ```{{Extension}}
    {{{Content}}}
    ```
  {{/each}}
{{/with}}
```

### 2. Generate File Content
```handlebars
{{!-- file_content.hbs --}}
{{#Switch Extension}}
  {{#Case ".cs"}}
    namespace {{Namespace}}
    {
        public class {{ClassName}}
        {
            {{{Content}}}
        }
    }
  {{/Case}}
  {{#Default}}
    {{{Content}}}
  {{/Default}}
{{/Switch}}
```

## Template Generation Rules

1. **Path Handling**
   - Always use forward slashes (/) for paths
   - Use relative paths from root directory
   - Handle spaces in paths

2. **Content Processing**
   - Preserve indentation
   - Maintain line endings
   - Escape special characters
   - Handle binary files (base64 encoding)

3. **Naming Conventions**
   - Preserve original casing
   - Support multiple casing styles
   - Handle special characters

4. **Metadata**
   - Preserve file permissions
   - Maintain timestamps
   - Keep file attributes

## AI Instructions

When asked to generate a template for a folder:

1. **Analyze Structure**
   ```handlebars
   {{#AnalyzeDirectory path}}
     {{> structure_template}}
   {{/AnalyzeDirectory}}
   ```

2. **Generate Base Template**
   ```handlebars
   {{#with RootDirectory}}
     {{> base_template}}
   {{/with}}
   ```

3. **Add Content Templates**
   ```handlebars
   {{#each FileTypes}}
     {{> (lookup . 'template')}}
   {{/each}}
   ```

4. **Include Helpers**
   ```handlebars
   {{#RegisterHelper 'PathCombine'}}
   {{#RegisterHelper 'ContentFormat'}}
   {{#RegisterHelper 'NameFormat'}}
   ```

## Example Usage

To generate a template for a specific folder:

1. Get the folder path
2. Analyze structure and patterns
3. Generate appropriate templates
4. Include necessary helpers
5. Validate generated templates

## Best Practices

1. **Template Organization**
   - Separate templates by file type
   - Use partials for reusable components
   - Maintain clear hierarchy

2. **Error Handling**
   - Validate paths
   - Handle missing files
   - Provide clear error messages

3. **Performance**
   - Use lazy loading for large directories
   - Cache template results
   - Optimize helper functions

4. **Maintenance**
   - Document template structure
   - Version templates
   - Include update instructions

## Advanced Patterns and Best Practices

### 1. Smart Template Generation

#### 1.1 Pattern Detection and Template Selection
```typescript
interface PatternDetector {
    detectFileType(path: string, content: string): string;
    detectLanguage(content: string): string;
    detectFramework(files: FileInfo[]): string;
    detectArchitecture(structure: DirectoryInfo): string;
}

interface TemplateSelector {
    selectTemplate(patterns: PatternDetection): string;
    customizeTemplate(template: string, context: any): string;
    validateTemplate(template: string): boolean;
}
```

#### 1.2 Content Analysis
```typescript
interface ContentAnalyzer {
    // Code structure analysis
    findDependencies(content: string): string[];
    findImports(content: string): string[];
    findClasses(content: string): ClassInfo[];
    findMethods(content: string): MethodInfo[];
    
    // Pattern analysis
    findCommonPatterns(contents: string[]): Pattern[];
    findNamingConventions(names: string[]): NamingPattern[];
    findCodeStyle(content: string): StyleGuide;
    
    // Architecture analysis
    detectLayering(structure: DirectoryInfo): ArchitecturePattern;
    detectDesignPatterns(codebase: CodebaseInfo): DesignPattern[];
}
```

### 2. Intelligent Template Customization

#### 2.1 Dynamic Template Adaptation
```handlebars
{{!-- dynamic_template.hbs --}}
{{#with TemplateContext}}
  {{#if DetectedFramework}}
    {{> (concat DetectedFramework '_template')}}
  {{else}}
    {{#switch ProjectType}}
      {{#case 'web'}}
        {{> web_template}}
      {{#case 'library'}}
        {{> library_template}}
      {{#case 'console'}}
        {{> console_template}}
      {{#default}}
        {{> generic_template}}
    {{/switch}}
  {{/if}}
{{/with}}
```

#### 2.2 Context-Aware Template Generation
```handlebars
{{!-- context_aware_template.hbs --}}
{{#with ProjectContext}}
  {{#if IsTestProject}}
    {{> test_project_template}}
  {{else if IsMicroservice}}
    {{> microservice_template}}
  {{else if IsWebApi}}
    {{> webapi_template}}
  {{else}}
    {{> standard_template}}
  {{/if}}

  {{#if RequiresAuthentication}}
    {{> auth_template}}
  {{/if}}

  {{#if UsesDependencyInjection}}
    {{> di_container_template}}
  {{/if}}
{{/with}}
```

### 3. Advanced Template Patterns

#### 3.1 Composite Templates
```handlebars
{{!-- composite_template.hbs --}}
{{#compose 'ProjectStructure'}}
  {{#layer 'Infrastructure'}}
    {{> database_context}}
    {{> repositories}}
    {{> logging}}
  {{/layer}}

  {{#layer 'Domain'}}
    {{> entities}}
    {{> value_objects}}
    {{> domain_services}}
  {{/layer}}

  {{#layer 'Application'}}
    {{> commands}}
    {{> queries}}
    {{> handlers}}
  {{/layer}}

  {{#layer 'Presentation'}}
    {{> controllers}}
    {{> views}}
    {{> view_models}}
  {{/layer}}
{{/compose}}
```

#### 3.2 Aspect-Oriented Templates
```handlebars
{{!-- aspect_template.hbs --}}
{{#aspect 'Logging'}}
  {{#pointcut 'Method'}}
    {{#before}}
      _logger.LogInformation("Entering {MethodName}", nameof({{MethodName}}));
    {{/before}}
    
    {{#after}}
      _logger.LogInformation("Exiting {MethodName}", nameof({{MethodName}}));
    {{/after}}
    
    {{#around}}
      try
      {
        {{proceed}}
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error in {MethodName}", nameof({{MethodName}}));
        throw;
      }
    {{/around}}
  {{/pointcut}}
{{/aspect}}
```

### 4. Best Practices for AI Template Generation

#### 4.1 Template Organization
1. **Hierarchical Structure**
   ```
   templates/
   ├── base/
   │   ├── project.hbs
   │   ├── class.hbs
   │   └── interface.hbs
   ├── patterns/
   │   ├── repository.hbs
   │   ├── service.hbs
   │   └── factory.hbs
   ├── infrastructure/
   │   ├── database.hbs
   │   ├── logging.hbs
   │   └── caching.hbs
   └── specialized/
       ├── webapi.hbs
       ├── mvc.hbs
       └── console.hbs
   ```

2. **Template Naming Conventions**
   - Use descriptive, lowercase names
   - Include purpose in name (e.g., `user_service_template.hbs`)
   - Group related templates with prefixes
   - Use suffixes for variations (e.g., `repository_async_template.hbs`)

#### 4.2 Code Generation Guidelines
1. **Consistency**
   - Maintain consistent naming conventions
   - Use consistent code formatting
   - Follow language-specific best practices

2. **Flexibility**
   - Use parameterized templates
   - Support multiple coding styles
   - Allow for customization

3. **Maintainability**
   - Document template purpose and usage
   - Keep templates modular
   - Use clear, descriptive variable names

4. **Performance**
   - Optimize template parsing
   - Use caching where appropriate
   - Minimize template complexity

#### 4.3 Template Validation
```typescript
interface TemplateValidator {
    validateSyntax(template: string): ValidationResult;
    validateContext(template: string, context: any): ValidationResult;
    validateOutput(generated: string): ValidationResult;
    validateConsistency(templates: Template[]): ValidationResult;
}

interface ValidationResult {
    isValid: boolean;
    errors: ValidationError[];
    warnings: ValidationWarning[];
    suggestions: Suggestion[];
}
```

### 5. Common Patterns


### 5. Implementation Patterns

#### 5.1 Project Structure Replication
```handlebars
{{!-- project_replication.hbs --}}
{{#with ProjectAnalysis}}
  // Generate directory structure
  {{#each Directories}}
    {{> directory_creation}}
  {{/each}}

  // Generate files with appropriate templates
  {{#each Files}}
    {{#with (analyzeFile this)}}
      {{> (getTemplateForFile this)}}
    {{/with}}
  {{/each}}

  // Generate special files
  {{#if HasSolution}}
    {{> solution_template}}
  {{/if}}

  {{#if HasTests}}
    {{> test_project_template}}
  {{/if}}

  // Generate documentation
  {{> readme_template}}
  {{> documentation_templates}}
{{/with}}
```

#### 5.2 Intelligent File Analysis
```typescript
interface FileAnalyzer {
    analyze(file: FileInfo): FileAnalysis;
}

interface FileAnalysis {
    type: FileType;
    language: ProgrammingLanguage;
    dependencies: Dependency[];
    structure: CodeStructure;
    patterns: DetectedPattern[];
    metrics: CodeMetrics;
}

interface TemplateGenerator {
    generateTemplate(analysis: FileAnalysis): string;
    customizeTemplate(template: string, context: any): string;
}
```

#### 5.3 Smart Template Selection
```typescript
class TemplateSelector {
    selectTemplate(file: FileInfo): Template {
        const analysis = analyzer.analyze(file);
        const baseTemplate = this.getBaseTemplate(analysis.type);
        const customizations = this.getCustomizations(analysis);
        return this.mergeTemplates(baseTemplate, customizations);
    }

    private getBaseTemplate(type: FileType): Template {
        switch (type) {
            case FileType.CSharpClass:
                return templates.get('class.hbs');
            case FileType.Interface:
                return templates.get('interface.hbs');
            case FileType.Configuration:
                return templates.get('config.hbs');
            default:
                return templates.get('generic.hbs');
        }
    }

    private getCustomizations(analysis: FileAnalysis): Customization[] {
        return [
            ...this.getLanguageCustomizations(analysis.language),
            ...this.getPatternCustomizations(analysis.patterns),
            ...this.getMetricsCustomizations(analysis.metrics)
        ];
    }
}
```

### 6. Quick Reference

#### 6.1 Common Template Patterns
```handlebars
{{!-- File generation --}}
{{#each Files}}
  {{> (lookup . 'template')}}
{{/each}}

{{!-- Conditional content --}}
{{#if Condition}}
  {{> template_a}}
{{else}}
  {{> template_b}}
{{/if}}

{{!-- Pattern matching --}}
{{#switch FileType}}
  {{#case 'cs'}}C# File{{/case}}
  {{#case 'ts'}}TypeScript File{{/case}}
  {{#default}}Unknown File{{/default}}
{{/switch}}
```

#### 6.2 Helper Functions
```typescript
// Path helpers
PathCombine(path1, path2)
GetRelativePath(basePath, targetPath)
NormalizePath(path)

// Content helpers
FormatCode(content, language)
DetectLanguage(content)
AnalyzeImports(content)

// Name helpers
PascalCase(name)
CamelCase(name)
GetNamespace(path)
```

---

*Note: This document serves as a comprehensive reference for AI systems to understand and generate appropriate Handlebars templates for folder structure replication. The templates, patterns, and helpers mentioned are part of the EzDbCodeGen framework. When implementing template generation, always follow the best practices and patterns outlined in this document to ensure consistent, maintainable, and efficient code generation.*
