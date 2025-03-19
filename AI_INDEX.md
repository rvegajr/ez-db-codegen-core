# EzDbCodeGen AI Index

This file serves as an index for AI assistants to quickly understand the structure and purpose of the EzDbCodeGen project.

## Project Overview

EzDbCodeGen is a code generation tool that uses database schema information to generate code based on Handlebars templates. It's designed to automate the creation of models, controllers, and other code artifacts from database schemas.

## Key Components

1. **EzDbCodeGen.Core**: Core library containing the code generation engine and utilities
   - Template processing
   - Schema handling
   - Configuration management

2. **EzDbCodeGen.Cli**: Command-line interface for the code generation tool
   - Commands for generating code
   - Project configuration
   - Template management

3. **Tests**: Test suite for validating code generation functionality
   - EF Core model generation tests
   - EF Core controller generation tests
   - Mock database schemas for testing

## Important Concepts

1. **Templates**: Handlebars templates (.hbs files) that define the structure of generated code
2. **Entity Security**: Configuration to control which entities are included in code generation
3. **Relationships**: Handling of entity relationships for navigation properties
4. **Code Generator**: The engine that processes templates and generates code files

## Key Files

- `TestCodeGenerator.cs`: Implementation of the code generator for tests
- `EFCoreModel.hbs`: Template for generating EF Core models
- `EFCoreController.hbs`: Template for generating EF Core controllers
- `MockDatabaseSchema.cs`: Utilities for creating mock database schemas

## Integration Points

- **EzDbSchema.Core**: Provides the database schema information
- **Handlebars.Net**: Used for template processing
- **System.Text.Json**: Used for serialization and debugging

## Common Usage Patterns

1. Create a database schema (using EzDbSchema or mock for testing)
2. Configure the code generator with namespace, output paths, etc.
3. Process templates for models and controllers
4. Generated code is written to the specified output directories

See the AI_USAGE.md file for detailed examples and usage patterns.
