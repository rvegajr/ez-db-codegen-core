using System;
using System.Collections.Generic;
using EzDbCodeGen.Core;
using EzDbSchema.Core.Interfaces;
using EzDbCodeGen.Core.Config;

namespace EzDbCodeGen.Tests.CLI
{
    /// <summary>
    /// Template data input for testing
    /// </summary>
    public class TemplateDataInput : ITemplateDataInput
    {
        /// <summary>
        /// Creates a new instance of the TemplateDataInput class
        /// </summary>
        /// <param name="database">The database to use as the data source</param>
        public TemplateDataInput(IDatabase database)
        {
            Schema = database;
            SchemaName = database.Name;
        }

        /// <summary>
        /// Gets or sets whether verbose messages are enabled
        /// </summary>
        public bool VerboseMessages { get; set; } = false;

        /// <summary>
        /// Gets or sets the schema name
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the database schema
        /// </summary>
        public IDatabase Schema { get; set; }

        /// <summary>
        /// Loads the schema from the configuration
        /// </summary>
        /// <typeparam name="T">The type of database to load</typeparam>
        /// <param name="config">The configuration to use</param>
        /// <returns>The loaded database schema</returns>
        public IDatabase LoadSchema<T>(Configuration config) where T : new()
        {
            // For testing, we just return the schema that was passed in the constructor
            return Schema?.Filter(config) ?? throw new InvalidOperationException("Schema is null");
        }

        /// <summary>
        /// Loads the schema from the configuration
        /// </summary>
        /// <param name="config">The configuration to use</param>
        /// <returns>The loaded database schema</returns>
        public IDatabase LoadSchema(Configuration config)
        {
            // For testing, we just return the schema that was passed in the constructor
            return Schema?.Filter(config) ?? throw new InvalidOperationException("Schema is null");
        }

        /// <summary>
        /// Gets the template data as a dictionary
        /// </summary>
        public Dictionary<string, object> GetTemplateData()
        {
            return new Dictionary<string, object>
            {
                { "Schema", Schema }
            };
        }
    }
}
