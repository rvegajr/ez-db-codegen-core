using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.MsSql;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Objects;
using System.Linq;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extentions;
using EzDbCodeGen.Core.Extentions.Objects;
using EzDbCodeGen.Core.Classes;

namespace EzDbCodeGen.Core
{
    /// <summary>
    /// Templates class that handles template data input
    /// </summary>
    public class Templates : ITemplateDataInput
    {
        private ITemplateDataInput _templateDataInput;

        /// <summary>
        /// Initializes a new instance of the Templates class.
        /// </summary>
        public Templates(string connectionString, string templateDataInput, string outputPath)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                var schema = new EzDbSchema.MsSql.Database();
                _templateDataInput = new TemplateInputDatabaseConnecton
                {
                    SchemaName = "Default",
                    VerboseMessages = true,
                    Schema = schema,
                    Server = connectionString,
                    Database = "Default"
                };
            }
            else if (!string.IsNullOrEmpty(templateDataInput))
            {
                var schema = new EzDbSchema.MsSql.Database();
                _templateDataInput = new TemplateInputFileSource
                {
                    DatabaseSchemaDumpFileName = templateDataInput,
                    SchemaName = Path.GetFileNameWithoutExtension(templateDataInput),
                    VerboseMessages = true,
                    Schema = schema
                };
            }
            else
            {
                var schema = new EzDbSchema.MsSql.Database();
                _templateDataInput = new TemplateInputDirectObject(schema)
                {
                    VerboseMessages = true,
                    SchemaName = "Default",
                    DatabaseSchemaDumpFileName = "",
                    Schema = schema
                };
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether verbose messages are enabled.
        /// </summary>
        public bool VerboseMessages 
        { 
            get { return _templateDataInput.VerboseMessages; }
            set { _templateDataInput.VerboseMessages = value; }
        }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string SchemaName
        {
            get { return _templateDataInput.SchemaName; }
            set { _templateDataInput.SchemaName = value; }
        }

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        public IDatabase Schema
        {
            get { return _templateDataInput.Schema; }
            set { _templateDataInput.Schema = value; }
        }

        /// <summary>
        /// Loads the schema.
        /// </summary>
        public IDatabase LoadSchema<T>(Configuration config) where T : new()
        {
            return _templateDataInput.LoadSchema<T>(config);
        }

        /// <summary>
        /// Loads the schema.
        /// </summary>
        public IDatabase LoadSchema(Configuration config)
        {
            return _templateDataInput.LoadSchema(config);
        }
    }

    public class TemplatesClass<T> where T : class
    {
        private static readonly JsonSerializerOptions DefaultJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string DatabaseSchemaDumpFileName;
        private readonly string ConnectionString;
        private readonly string SchemaName;

        public TemplatesClass(string databaseSchemaDumpFileName, string connectionString, string schemaName)
        {
            DatabaseSchemaDumpFileName = databaseSchemaDumpFileName ?? throw new ArgumentNullException(nameof(databaseSchemaDumpFileName));
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            SchemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
        }

        public T? GetDatabaseSchema()
        {
            if (!File.Exists(DatabaseSchemaDumpFileName))
            {
                throw new FileNotFoundException($"Database schema dump file not found: {DatabaseSchemaDumpFileName}");
            }

            try
            {
                var jsonString = File.ReadAllText(DatabaseSchemaDumpFileName);
                var schema = JsonSerializer.Deserialize<T>(jsonString, DefaultJsonOptions);

                if (schema == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize schema from {DatabaseSchemaDumpFileName}");
                }

                return schema;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing schema file {DatabaseSchemaDumpFileName}: {ex.Message}", ex);
            }
        }

        public void SaveDatabaseSchema(T schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            try
            {
                var jsonString = JsonSerializer.Serialize(schema, DefaultJsonOptions);
                File.WriteAllText(DatabaseSchemaDumpFileName, jsonString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving schema to {DatabaseSchemaDumpFileName}: {ex.Message}", ex);
            }
        }

        public IDatabase? GetDatabaseSchemaAsDatabase()
        {
            if (!File.Exists(DatabaseSchemaDumpFileName))
            {
                throw new FileNotFoundException($"Database schema dump file not found: {DatabaseSchemaDumpFileName}");
            }

            try
            {
                var jsonString = File.ReadAllText(DatabaseSchemaDumpFileName);
                var database = JsonSerializer.Deserialize<IDatabase>(jsonString, DefaultJsonOptions);

                if (database == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize database schema from {DatabaseSchemaDumpFileName}");
                }

                return database;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing database schema file {DatabaseSchemaDumpFileName}: {ex.Message}", ex);
            }
        }
    }
}
