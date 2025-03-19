using System;
using System.Collections.Generic;
using System.IO;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbSchema.Core.Interfaces;
using HandlebarsDotNet;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Extentions;
using System.Text.Json;

namespace EzDbCodeGen.Tests.EFCore
{
    /// <summary>
    /// Test code generator that doesn't rely on AppSettings
    /// </summary>
    public class TestCodeGenerator : CodeGenBase
    {
        private readonly Configuration _configuration;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCodeGenerator"/> class
        /// </summary>
        /// <param name="configuration">The configuration</param>
        public TestCodeGenerator(Configuration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            HandlebarsExtensions.RegisterHelpers();
        }
        
        /// <summary>
        /// Processes a template for models
        /// </summary>
        /// <param name="templatePath">Path to the template file</param>
        /// <param name="templateDataInput">Template data input</param>
        /// <param name="outputPath">Output path</param>
        public void ProcessModelTemplate(string templatePath, ITemplateDataInput templateDataInput, string outputPath)
        {
            // Check if the template file exists
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            
            // Read the template content
            var templateContent = File.ReadAllText(templatePath);
            
            // Register helpers
            Handlebars.RegisterHelper("eq", (writer, context, parameters) =>
            {
                if (parameters.Length != 2)
                {
                    throw new HandlebarsException("{{eq}} helper requires exactly two arguments");
                }
                
                var left = parameters[0];
                var right = parameters[1];
                
                writer.Write(left.Equals(right));
            });
            
            // Compile the template
            var template = Handlebars.Compile(templateContent);
            
            // Process each entity in the database
            var database = templateDataInput.Schema;
            foreach (var entity in database.Entities.Values)
            {
                // Skip entities marked as secured
                var entitySecurity = _configuration.EntitySecurity();
                if (entitySecurity != null && entitySecurity.TryGetValue(entity.TableName, out var security) && security.Secured)
                {
                    Console.WriteLine($"Skipping secured entity: {entity.TableName}");
                    continue;
                }
                
                // Create the output directory if it doesn't exist
                var modelOutputPath = Path.Combine(outputPath, "Models");
                if (!Directory.Exists(modelOutputPath))
                {
                    Directory.CreateDirectory(modelOutputPath);
                }
                
                // Normalize entity name for proper casing in model name
                string normalizedEntityName = char.ToUpper(entity.TableName[0]) + entity.TableName.Substring(1);
                
                // Set the output file path
                var outputFilePath = Path.Combine(modelOutputPath, $"{normalizedEntityName}.cs");
                
                // Create the template data
                var templateData = new
                {
                    Namespace = _configuration.Namespace(),
                    TableName = entity.TableName,
                    EntityName = normalizedEntityName,
                    SchemaName = entity.DatabaseSchema,
                    PrimaryKeyName = GetPrimaryKeyName(entity),
                    Properties = entity.Properties.Values,
                    Relationships = GetRelationships(database, entity),
                    Entity = entity,
                    Database = database,
                    entityNameLower = char.ToLowerInvariant(normalizedEntityName[0]) + normalizedEntityName.Substring(1)
                };
                
                // Debug output
                Console.WriteLine($"Template data for {normalizedEntityName}:");
                Console.WriteLine($"  Namespace: {_configuration.Namespace()}");
                Console.WriteLine($"  TableName: {entity.TableName}");
                Console.WriteLine($"  EntityName: {normalizedEntityName}");
                Console.WriteLine($"  SchemaName: {entity.DatabaseSchema}");
                Console.WriteLine($"  PrimaryKeyName: {GetPrimaryKeyName(entity)}");
                Console.WriteLine($"  Properties count: {entity.Properties.Values.Count()}");
                
                var relationships = GetRelationships(database, entity).ToList();
                Console.WriteLine($"  Relationships count: {relationships.Count}");
                foreach (dynamic rel in relationships)
                {
                    Console.WriteLine($"    Relationship: {rel.FromTableName} -> {rel.ToTableName}");
                    Console.WriteLine($"      IsCollection: {rel.IsCollection}");
                    Console.WriteLine($"      RelatedEntity: {rel.RelatedEntity}");
                    Console.WriteLine($"      NavigationProperty: {rel.NavigationProperty}");
                }
                
                // Apply the template
                var result = template(templateData);
                
                // Write debug output to a separate file for inspection
                try
                {
                    var debugOutputPath = Path.Combine(modelOutputPath, $"DEBUG_{normalizedEntityName}.cs");
                    File.WriteAllText(debugOutputPath, result);
                    Console.WriteLine($"Debug output written to: {debugOutputPath}");
                    
                    // Also write the template data as JSON for debugging
                    var jsonData = System.Text.Json.JsonSerializer.Serialize(templateData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    var jsonOutputPath = Path.Combine(modelOutputPath, $"DEBUG_DATA_{normalizedEntityName}.json");
                    File.WriteAllText(jsonOutputPath, jsonData);
                    Console.WriteLine($"Debug template data written to: {jsonOutputPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing debug output: {ex.Message}");
                }
                
                // Write the result to the output file
                try
                {
                    File.WriteAllText(outputFilePath, result);
                    Console.WriteLine($"Generated model file: {outputFilePath}");
                    
                    // Output the file content for debugging
                    Console.WriteLine("Generated file content:");
                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to file {outputFilePath}: {ex.Message}");
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Processes a template for controllers
        /// </summary>
        /// <param name="templatePath">Path to the template file</param>
        /// <param name="templateDataInput">Template data input</param>
        /// <param name="outputPath">Output path</param>
        public void ProcessControllerTemplate(string templatePath, ITemplateDataInput templateDataInput, string outputPath)
        {
            // Check if the template file exists
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            
            // Read the template content
            var templateContent = File.ReadAllText(templatePath);
            
            // Compile the template
            var template = Handlebars.Compile(templateContent);
            
            // Process each entity in the database
            var database = templateDataInput.Schema;
            foreach (var entity in database.Entities.Values)
            {
                // Skip entities marked as secured
                var entitySecurity = _configuration.EntitySecurity();
                if (entitySecurity != null && entitySecurity.TryGetValue(entity.TableName, out var security) && security.Secured)
                {
                    Console.WriteLine($"Skipping secured entity: {entity.TableName}");
                    continue;
                }
                
                // Create the output directory if it doesn't exist
                var controllerOutputPath = Path.Combine(outputPath, "Controllers");
                if (!Directory.Exists(controllerOutputPath))
                {
                    Directory.CreateDirectory(controllerOutputPath);
                }
                
                // Normalize entity name for proper casing in controller name
                string normalizedEntityName = char.ToUpper(entity.TableName[0]) + entity.TableName.Substring(1);
                
                // Set the output file path
                var outputFilePath = Path.Combine(controllerOutputPath, $"{normalizedEntityName}Controller.cs");
                
                // Create the template data
                var templateData = new
                {
                    Namespace = _configuration.Namespace(),
                    TableName = entity.TableName,
                    EntityName = normalizedEntityName,
                    SchemaName = entity.DatabaseSchema,
                    PrimaryKeyName = GetPrimaryKeyName(entity),
                    Properties = entity.Properties.Values,
                    Relationships = GetRelationships(database, entity),
                    Entity = entity,
                    Database = database,
                    entityNameLower = char.ToLowerInvariant(normalizedEntityName[0]) + normalizedEntityName.Substring(1)
                };
                
                // Debug output
                Console.WriteLine($"Template data for {normalizedEntityName}Controller:");
                Console.WriteLine($"  Namespace: {_configuration.Namespace()}");
                Console.WriteLine($"  TableName: {entity.TableName}");
                Console.WriteLine($"  EntityName: {normalizedEntityName}");
                Console.WriteLine($"  SchemaName: {entity.DatabaseSchema}");
                Console.WriteLine($"  PrimaryKeyName: {GetPrimaryKeyName(entity)}");
                Console.WriteLine($"  Properties count: {entity.Properties.Values.Count()}");
                
                var relationships = GetRelationships(database, entity).ToList();
                Console.WriteLine($"  Relationships count: {relationships.Count}");
                foreach (dynamic rel in relationships)
                {
                    Console.WriteLine($"    Relationship: {rel.FromTableName} -> {rel.ToTableName}");
                    Console.WriteLine($"      IsCollection: {rel.IsCollection}");
                    Console.WriteLine($"      RelatedEntity: {rel.RelatedEntity}");
                    Console.WriteLine($"      NavigationProperty: {rel.NavigationProperty}");
                }
                
                // Apply the template
                var result = template(templateData);
                
                // Write debug output to a separate file for inspection
                try
                {
                    var debugOutputPath = Path.Combine(controllerOutputPath, $"DEBUG_{normalizedEntityName}Controller.cs");
                    File.WriteAllText(debugOutputPath, result);
                    Console.WriteLine($"Debug output written to: {debugOutputPath}");
                    
                    // Also write the template data as JSON for debugging
                    var jsonData = System.Text.Json.JsonSerializer.Serialize(templateData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    var jsonOutputPath = Path.Combine(controllerOutputPath, $"DEBUG_DATA_{normalizedEntityName}Controller.json");
                    File.WriteAllText(jsonOutputPath, jsonData);
                    Console.WriteLine($"Debug template data written to: {jsonOutputPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing debug output: {ex.Message}");
                }
                
                // Write the result to the output file
                try
                {
                    File.WriteAllText(outputFilePath, result);
                    Console.WriteLine($"Generated controller file: {outputFilePath}");
                    
                    // Output the file content for debugging
                    Console.WriteLine("Generated file content:");
                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to file {outputFilePath}: {ex.Message}");
                    throw;
                }
            }
        }
        
        private string GetPrimaryKeyName(IEntity entity)
        {
            // Find the primary key property
            foreach (var prop in entity.Properties.Values)
            {
                if (prop.IsPrimaryKey)
                {
                    return prop.PropertyName;
                }
            }
            
            // Default to "Id" if no primary key is found
            return "Id";
        }
        
        private IEnumerable<dynamic> GetRelationships(IDatabase database, IEntity entity)
        {
            var relationships = new List<dynamic>();
            
            if (entity.Relationships != null)
            {
                foreach (var relationship in entity.Relationships)
                {
                    // Get the related entity
                    var relatedEntity = relationship.ToEntity;
                    
                    // Determine if this is a collection based on the relationship type
                    bool isCollection = relationship.MultiplicityType == RelationshipMultiplicityType.OneToMany;
                    
                    // Add the relationship to the list
                    relationships.Add(new
                    {
                        FromTableName = relationship.FromTableName,
                        ToTableName = relationship.ToTableName,
                        IsCollection = isCollection,
                        RelatedEntity = relatedEntity.TableName,
                        NavigationProperty = relatedEntity.TableName
                    });
                }
            }
            
            return relationships;
        }
    }
}
