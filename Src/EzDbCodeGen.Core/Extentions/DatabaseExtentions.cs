using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using EzDbSchema.Core.Enums;
using EzDbSchema.Core.Extentions;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Interfaces;
using CoreInterfaces = EzDbSchema.Core.Interfaces;
using DbConfig = EzDbCodeGen.Core.Config.Database;
using DbCore = EzDbSchema.Core.Objects.Database;
using EzDbCodeGen.Core.Extensions;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Extensions
{
    internal class SchemaObjectColumnName : SchemaObjectName
    {
        public string ColumnName { get; set; } = string.Empty;

        public SchemaObjectColumnName(CoreInterfaces.IProperty? property) : base()
        {
            if (property == null) return;

            var parentEntity = property.GetType().GetProperty("ParentEntity")?.GetValue(property) as CoreInterfaces.IEntity;
            if (parentEntity == null) return;

            SchemaName = parentEntity.GetType().GetProperty("DatabaseSchema")?.GetValue(parentEntity)?.ToString() ?? DefaultSchemaName;
            TableName = parentEntity.GetType().GetProperty("TableName")?.GetValue(parentEntity)?.ToString() ?? string.Empty;
            ColumnName = property.GetType().GetProperty("ColumnName")?.GetValue(property)?.ToString() ?? string.Empty;
        }

        public SchemaObjectColumnName(string schemaObjectName) : base(schemaObjectName)
        {
            this.Parse(schemaObjectName);
        }

        public override SchemaObjectName Parse(string schemaObjectName)
        {
            if (string.IsNullOrEmpty(schemaObjectName))
            {
                throw new ArgumentException("Schema object name cannot be null or empty");
            }

            SchemaName = DefaultSchemaName;
            if (string.IsNullOrEmpty(SchemaName))
            {
                SchemaName = "dbo";
            }

            var parts = schemaObjectName.Split('.');
            if (parts.Length >= 3)
            {
                SchemaName = parts[0];
                TableName = parts[1];
                ColumnName = parts[2];
            }
            else if (parts.Length == 2)
            {
                TableName = parts[0];
                ColumnName = parts[1];
            }
            else
            {
                throw new ArgumentException($"Cannot determine table for schema object: {schemaObjectName}");
            }
            return this;
        }

        public override string AsFullName()
        {
            return $"{SchemaName}.{TableName}.{ColumnName}";
        }
    }

    public class SchemaObjectName
    {
        protected string DefaultSchemaName = string.Empty;
        public string SchemaName = string.Empty;
        public string TableName = string.Empty;

        public SchemaObjectName()
        {
        }

        public SchemaObjectName(CoreInterfaces.IEntity? entity)
        {
            DefaultSchemaName = EzDbCodeGen.Internal.AppSettings.Instance.Configuration.Database.DefaultSchema;
            SchemaName = entity?.GetType().GetProperty("DatabaseSchema")?.GetValue(entity)?.ToString() ?? DefaultSchemaName;
            TableName = entity?.GetType().GetProperty("TableName")?.GetValue(entity)?.ToString() ?? string.Empty;
        }

        public SchemaObjectName(string schemaObjectName)
        {
            this.Parse(schemaObjectName);
        }

        public virtual SchemaObjectName Parse(string schemaObjectName)
        {
            if (string.IsNullOrEmpty(schemaObjectName))
            {
                throw new ArgumentException("Schema object name cannot be null or empty");
            }

            TableName = string.Empty;
            SchemaName = DefaultSchemaName;
            if (string.IsNullOrEmpty(SchemaName))
            {
                SchemaName = "dbo";
            }

            var parts = schemaObjectName.Split('.');
            if (parts.Length > 1)
            {
                SchemaName = parts[0];
                TableName = string.Join(".", parts.Skip(1));
            }
            else
            {
                TableName = schemaObjectName;
            }
            return this;
        }

        public virtual string AsFullName()
        {
            return $"{SchemaName}.{TableName}";
        }
    }

    public static class DatabaseExtensions
    {
        private static readonly JsonSerializerSettings DefaultJsonSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// Filters the specified database using the internal configuration file.  The config file will remove those objects 
        /// that the config marked as deleted, alter primary keys and rename Alias fields
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="config">Configuration file</param>
        /// <returns></returns>
        public static IDatabase Filter(this IDatabase database, Configuration config)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (!string.IsNullOrEmpty(config.SourceFileName) && config.SourceFileName != EzDbCodeGen.Internal.AppSettings.Instance.ConfigurationFileName)
            {
                EzDbCodeGen.Internal.AppSettings.Instance.ConfigurationFileName = config.SourceFileName;
            }

            // Use config settings to remove filtered entities
            var entitiesToDelete = new List<string>();
            foreach (var entityKey in database.Entities.Keys)
            {
                if (string.IsNullOrEmpty(entityKey)) continue;

                if (config.IsIgnoredEntity(entityKey))
                {
                    entitiesToDelete.Add(entityKey);
                }
                else if (database.Entities.TryGetValue(entityKey, out var entity) &&
                         entity?.PrimaryKeys?.Count == 0 &&
                         !config.IsEntityWithConfigKeyDeclaration(entityKey) &&
                         config.Database?.FilterEntitiesWithNoKey == true)
                {
                    entitiesToDelete.Add(entityKey);
                }
            }

            // Create a deep copy of the database
            var databaseCopy = DeepClone(database);

            // Remove filtered entities from the copy
            foreach (var entityKey in entitiesToDelete.Where(key => databaseCopy.ContainsKey(key)))
            {
                databaseCopy.Entities.Remove(entityKey);
            }

            // Process remaining entities
            foreach (var entityKey in databaseCopy.Entities.Keys.ToList())
            {
                if (string.IsNullOrEmpty(entityKey)) continue;

                var entity = databaseCopy.Entities[entityKey];
                if (entity == null) continue;

                // Apply configuration changes to the entity
                ApplyConfigurationToEntity(entity, config);
            }

            return databaseCopy;
        }

        private static void ApplyConfigurationToEntity(IEntity entity, Configuration config)
        {
            var entityConfig = EzDbCodeGen.Core.Extensions.ConfigurationExtensions.GetEntityConfiguration(config, entity.TableName);
            if (entityConfig == null) return;

            // Apply primary key overrides
            if (entityConfig.Overrides.PrimaryKey.Any())
            {
                entity.PrimaryKeys.Clear();
                foreach (var pkOverride in entityConfig.Overrides.PrimaryKey)
                {
                    if (entity.Properties.TryGetValue(pkOverride.FieldName, out var property))
                    {
                        entity.PrimaryKeys.Add(property);
                    }
                }
            }

            // Apply field overrides
            foreach (var fieldOverride in entityConfig.Overrides.Fields)
            {
                if (entity.Properties.TryGetValue(fieldOverride.FieldName, out var property))
                {
                    // Apply overrides to the property
                    ApplyFieldOverrides(property, fieldOverride);
                }
            }
        }

        private static void ApplyFieldOverrides(IProperty property, Field fieldOverride)
        {
            // Apply field overrides to the property
            if (!string.IsNullOrEmpty(fieldOverride.ColumnAttributeTypeName))
            {
                property.DataType = fieldOverride.ColumnAttributeTypeName;
            }

            if (fieldOverride.Nullable.HasValue)
            {
                property.IsNullable = fieldOverride.Nullable.Value;
            }

            // Additional field overrides can be applied here
        }

        private static IDatabase DeepClone(IDatabase database)
        {
            var json = JsonConvert.SerializeObject(database, DefaultJsonSettings);
            return JsonConvert.DeserializeObject<DbCore>(json, DefaultJsonSettings) ?? throw new InvalidOperationException("Failed to clone database");
        }

        /// <summary>
        /// Using a config filtered database object,  this will search for an entity in the database table.
        /// </summary>
        /// <param name="_database">The database.</param>
        /// <param name="SchemaObjectName">Name of the schema object to search for</param>
        /// <param name="entity">The entity to return</param>
        /// <returns></returns>
        public static bool EntityExists(this IDatabase database, string schemaObjectName, ref CoreInterfaces.IEntity? entity)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrEmpty(schemaObjectName)) throw new ArgumentException("Schema object name cannot be null or empty", nameof(schemaObjectName));

            entity = database.FindEntity(schemaObjectName);
            return entity != null;
        }

        /// <summary>
        /// Using a config filtered database object, this will search for an entity in the database table.
        /// </summary>
        /// <returns>The found entity or null if not found</returns>
        /// <param name="database">Database to search in</param>
        /// <param name="schemaObjectName">Schema object name to search for</param>
        public static CoreInterfaces.IEntity? FindEntity(this IDatabase database, string schemaObjectName)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrEmpty(schemaObjectName)) throw new ArgumentException("Schema object name cannot be null or empty", nameof(schemaObjectName));

            var parsedName = new SchemaObjectName(schemaObjectName);

            foreach (var entity in database.Entities.Values)
            {
                if (entity == null) continue;

                if (string.Equals(entity.DatabaseSchema, parsedName.SchemaName, StringComparison.OrdinalIgnoreCase)
                    && (entity.TableName.ToLower() == parsedName.TableName.ToLower()))
                {
                    return entity;
                }
            }
            return default;
        }

        /// <summary>
        /// Using a config filtered database object,  this will search for all entities that match a certain criteria in the database table.
        /// </summary>
        /// <returns>An altered copy of the database</returns>
        /// <param name="_database">Database.</param>
        /// <param name="schemaObjectNameSearchParm">Search parm that can be used to find a list of entities</param>
        public static List<CoreInterfaces.IEntity> FindEntities(this IDatabase database, string searchPattern)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentException("Search pattern cannot be null or empty", nameof(searchPattern));

            var matchedEntities = new List<CoreInterfaces.IEntity>();

            foreach (var entity in database.Entities.Values)
            {
                if (entity == null) continue;

                var entitySchemaObjectName = new SchemaObjectName(entity);
                var entityFullName = entitySchemaObjectName.AsFullName();
                if (string.IsNullOrEmpty(entityFullName)) continue;

                bool isMatch;
                if (searchPattern.Contains("*"))
                {
                    // Convert the wildcard pattern to a regex pattern
                    var regexPattern = $"^{Regex.Escape(searchPattern.ToLowerInvariant()).Replace("\\*", ".*")}$";
                    isMatch = Regex.IsMatch(entityFullName.ToLowerInvariant(), regexPattern);
                }
                else
                {
                    isMatch = string.Equals(entityFullName, searchPattern, StringComparison.OrdinalIgnoreCase);
                }

                if (isMatch)
                {
                    matchedEntities.Add(entity);
                }
            }

            return matchedEntities;
        }

        public static IDatabase ApplyFilters(this IDatabase database, IEnumerable<IFilter> filters)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            var clonedDatabase = database.DeepClone();

            foreach (var filter in filters)
            {
                if (filter == null) continue;

                try
                {
                    filter.Apply(clonedDatabase);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error applying filter {filter.GetType().Name}", ex);
                }
            }

            return clonedDatabase;
        }

        public static string ToJson(this IDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return JsonConvert.SerializeObject(database, DefaultJsonSettings);
        }

        public static void SaveToJson(this IDatabase database, string filePath)
        {
            var json = database.ToJson();
            File.WriteAllText(filePath, json);
        }

        public static IDatabase LoadFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Database schema file not found: {filePath}");

            var json = File.ReadAllText(filePath);
            var database = JsonConvert.DeserializeObject<EzDbSchema.Core.Objects.Database>(json, DefaultJsonSettings);

            if (database == null)
                throw new InvalidOperationException("Failed to deserialize database schema");

            return database;
        }

        public static IDictionary<string, object> ToDictionary(this IDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var jsonString = JsonConvert.SerializeObject(database, DefaultJsonSettings);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString, DefaultJsonSettings);

            if (dictionary == null)
                throw new InvalidOperationException("Failed to convert database to dictionary");

            return dictionary;
        }

        public static IDictionary<string, T> ToDictionary<T>(this IDatabase database, Func<IEntity, T> selector)
        {
            return database.Values.ToDictionary(
                entity => entity.DatabaseObjectName,
                entity => selector(entity)
            );
        }

        public static IDictionary<string, T> ToDictionary<T>(this IDatabase database, Func<IEntity, string> keySelector, Func<IEntity, T> valueSelector)
        {
            return database.Values.ToDictionary(
                entity => keySelector(entity),
                entity => valueSelector(entity)
            );
        }
    }
}
