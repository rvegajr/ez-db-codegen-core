﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using EzDbSchema.Core.Interfaces;
using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Objects;

namespace EzDbCodeGen.Core
{
    public interface ITemplateInput
    {
        bool VerboseMessages { get; set; }
        string SchemaName { get; set; }
        IDatabase Schema { get; set; }
		IDatabase LoadSchema<T>() where T : new();
        IDatabase LoadSchema();
    }

    /// <summary>
    /// Enumeration that dictates what to do with the path.  This only applies to those templates that point to a path (and not those that affect one file).  So, bascailly,
    /// this templates with a ##FILE insturction in it. 
    /// </summary>
    public enum TemplatePathOption
    {
        /// <summary>
        /// Clear - If you only pass the originalTemplateInputSource parameter
        /// SyncDiff - If you pass both originalTemplateInputSource and compareToTemplateInputSource parameters and template has <ENTITY_KEY>
        /// </summary>
        Auto,
        /// <summary>
        /// This will update changed existing files of the same name, 
        /// add files that do not exist, and delete files that are note in the differences between the 2 schemas
        /// This is the Default if you call ProcessTemplate with both originalTemplateInputSource and compareToTemplateInputSource parameters
        /// and there is an <ENTITY_KEY> specifier in the template.  If ENTITY_KEY does not exist, then 'Clear' will be assumed.
        /// </summary>
        SyncDiff,
        /// <summary>
        /// This will update changed existing files of the same name, 
        /// add files that do not exist, and delete files that do not exist as inserts or updates
        /// </summary>
        SyncForce,
        /// <summary>
        /// This will be the default of you only pass the originalTemplateInputSource parameter.  
        /// This will remove everything previously existing in the path before adding every file generated by the template. 
        /// </summary>
        Clear,
        /// <summary>
        /// Update will leave previous files alone and only update those files that changed.
        /// </summary>
        Update
    }
    public interface ITemplateRenderer
    {
        TemplatePathOption TemplatePathOption { get; set; }
        bool VerboseMessages { get; set; }
        ReturnCode ProcessTemplate(string TemplateFileName, ITemplateInput templateInput, string OutputPath);
        ReturnCode ProcessTemplate(string TemplateFileName, ITemplateInput originalTemplateInputSource, ITemplateInput compareToTemplateInputSource, string OutputPath);
    }

    public class TemplateInputDirectObject : ITemplateInput
    {
        public string SchemaName { get; set; }
        public bool VerboseMessages { get; set; } = true;
        public string DatabaseSchemaDumpFileName { get; set; }
        public TemplateInputDirectObject()
        {

        }
        public TemplateInputDirectObject(IDatabase schema)
        {
            Schema = schema;

        }
		public IDatabase Schema { get; set; }

        /// <summary>
        /// Will Load the schema, however,  it is kind of pointless here since you are setting the schema directly.  This is mainly used to satisfy the interface.
        /// </summary>
        /// <returns>SchemaData - Loads the schema that was rendered if it completes</returns>
        public IDatabase LoadSchema<T>() where T : new()
        {
            try
            {
                return this.Schema.Filter();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Not sure how could fail, but ther have been wierder stuff :)"), ex);
            }
        }

        public IDatabase LoadSchema()
        {
            try
            {
                return this.Schema.Filter();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Not sure how could fail, but ther have been wierder stuff :)"), ex);
            }
        }
    }

    public class TemplateInputFileSource : ITemplateInput
    {
        public string SchemaName { get; set; }
        public bool VerboseMessages { get; set; } = true;
        public string DatabaseSchemaDumpFileName { get; set; }
        public TemplateInputFileSource()
        {

        }
        public TemplateInputFileSource(string databaseSchemaDumpFileNameToLoad)
        {
            DatabaseSchemaDumpFileName = databaseSchemaDumpFileNameToLoad;

        }
		public IDatabase Schema { get; set; }

        /// <summary>
        /// Loads the schema filtered by Configuration 
        /// </summary>
        /// <returns>SchemaData - Loads the schema that was rendered if it completes</returns>
        public IDatabase LoadSchema<T>() where T : new()
        {
            try
            {
				this.Schema = (IDatabase)JsonConvert.DeserializeObject<T>(File.ReadAllText(DatabaseSchemaDumpFileName),
                    new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.All,
                        TypeNameHandling = TypeNameHandling.All
                    });
                return this.Schema.Filter();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed on reading Schema Data File '{0}'.  Please make sure this file exists or is the proper format.  {1}", DatabaseSchemaDumpFileName, ex.Message), ex);
            }
        }

        public IDatabase LoadSchema()
        {
            try
            {
                this.Schema = (IDatabase)JsonConvert.DeserializeObject<Database>(File.ReadAllText(DatabaseSchemaDumpFileName),
                    new JsonSerializerSettings {
                        PreserveReferencesHandling = PreserveReferencesHandling.All,
                        TypeNameHandling = TypeNameHandling.All
                    });
                return this.Schema.Filter();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed on reading Schema Data File '{0}'.  Please make sure this file exists or is the proper format.  {1}", DatabaseSchemaDumpFileName, ex.Message), ex);
            }
        }
    }

    /// <summary>
    /// A database connection type of Database Input
    /// </summary>
	public class TemplateInputDatabaseConnecton : ITemplateInput
    {
        public string SchemaName { get; set; }
        public bool VerboseMessages { get; set; } = true;
        /// <summary>
        /// Initializes a new instance of the TemplateInputDatabaseConnecton" class.
        /// </summary>
        public TemplateInputDatabaseConnecton()
        {

        }
        /// <summary>
        /// Initializes a new instance of the TemplateInputDatabaseConnecton" class.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        public TemplateInputDatabaseConnecton(string connectionString)
        {
            this.ParseConnectionString(connectionString);
        }

        /// <summary>
        /// Gets or sets the schema
        /// </summary>
        /// <value>The schema object</value>
		public IDatabase Schema { get; set; }
        /// <summary>
        /// Loads the schema based on the connection string provided
        /// </summary>
        /// <returns><c>true</c>, if schema was loaded, <c>false</c> otherwise.</returns>
		public IDatabase LoadSchema<T>() where T : new()
        {
            try
            {
                this.Schema = (IDatabase)(new T());
                this.Schema.ShowWarnings = this.VerboseMessages;
                this.Schema.Render(this.SchemaName, this.AsConnectionString());
                return this.Schema.Filter();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not load schema with connection string '{0}'.  " + ex.Message, this.AsConnectionString()), ex);
            }
        }

        /// <summary></summary>
        public const string DB_MSSQL = "MSSQL";
        /// <summary></summary>
        public const string DB_ORACLE = "ORACLE";
        /// <summary>Database Server or Data source for ODBC or Oracle data sources</summary>
        public string Server { get; set; } = "";
        /// <summary>Database to access</summary>
        public string Database { get; set; } = "";
        /// <summary></summary>
        public string UserId { get; set; } = "";
        /// <summary></summary>
        public string Password { get; set; } = "";

        /// <summary>Rneder the connection string.  the Default format will be MSSQL</summary>
        public string AsConnectionString()
        {
            return AsConnectionString(DB_MSSQL);
        }
        /// <summary>Render the connection string</summary>
        public string AsConnectionString(string db)
        {
            switch (db)
            {
                case DB_MSSQL:
                    return string.Format("Server={0};Database={1};user id={2};password={3}", Server, Database, UserId, Password);
                case DB_ORACLE:
                    return string.Format("User Id={0};Password={1};Data Source={2};", UserId, Password, Server);
            }
            return "";
        }
        /// <summary>
        /// Parses the connection string.
        /// </summary>
        /// <returns><c>true</c>, if connection string was parsed, <c>false</c> otherwise.</returns>
        /// <param name="connectionString">Connection string.</param>
        public bool ParseConnectionString(string connectionString)
        {
            try
            {
                var arr = connectionString.Split(';');
                foreach (var str in arr)
                {
                    var strAsUpper = str.ToUpper();
                    if (strAsUpper.Contains("SERVER"))
                        this.Server = (str).Pluck("=");
                    else if (strAsUpper.Contains("DATA SOURCE"))
                        this.Server = (str).Pluck("=");
                    else if (strAsUpper.Contains("DATABASE"))
                        this.Database = (str).Pluck("=");
                    else if (strAsUpper.Contains("USER"))
                        this.UserId = (str).Pluck("=");
                    else if (strAsUpper.Contains("PASS"))
                        this.Password = (str).Pluck("=");
                }
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public IDatabase LoadSchema()
        {
            try
            {
                this.Schema = (new EzDbSchema.MsSql.Database() { ShowWarnings = this.VerboseMessages }).Render(this.SchemaName, this.AsConnectionString());
                return this.Schema.Filter();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not load schema with connection string '{0}'.  " + ex.Message, this.AsConnectionString()), ex);
            }
        }
    }
}
