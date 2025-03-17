using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
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
}
