using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extensions;
using EzDbCodeGen.Core.Extensions;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Internal;
[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core.Extentions
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


        /// <summary>
        /// Will return this object as a fully qualified string SchemaName.ObjectName
        /// </summary>
        /// <returns></returns>
        public override string AsFullName()
        {
            return SchemaName + "." + TableName + ColumnName;
        }
    }

    public class SchemaObjectName
    {
        public SchemaObjectName()
        {

        }
        public string SchemaName = "";
        public string TableName = "";
        protected string DefaultSchemaName = "";
        public SchemaObjectName(CoreInterfaces.IEntity entity)
        {
            DefaultSchemaName = Internal.AppSettings.Instance.Configuration.Database.DefaultSchema;
            SchemaName = entity?.GetType().GetProperty("DatabaseSchema")?.GetValue(entity)?.ToString() ?? DefaultSchemaName;
            TableName = entity?.GetType().GetProperty("TableName")?.GetValue(entity)?.ToString() ?? "";
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


        /// <summary>
        /// Will return this object as a fully qualified string SchemaName.ObjectName
        /// </summary>
        /// <returns></returns>
        public virtual string AsFullName()
        {
            return SchemaName + "." + TableName;
        }
    }

    public static class DatabaseExtentions
    {
        /// <summary>
        /// Filters the specified database using the internal configuration file.  The config file will remove those objects 
        /// that the config marked as deleted, alter primary keys and rename Alias fields
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns></returns>
        /// 
        /*
        public static IDatabase Filter(this IDatabase database)
        {
            return database.Filter(Configuration.Instance);
        }
        */
        /// <summary>
        /// This will filter a schema based on the a passed configuration file.  This will remove entites that will need to be ignored and alter primary keys based
        /// on the parameters passed 
        /// </summary>
        /// <returns>An altered copy of the database</returns>
        /// <param name="database">Database.</param>
        /// <param name="config">Configuration file</param>
        public static IDatabase Filter(this IDatabase database, Configuration config)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (!string.IsNullOrEmpty(config.SourceFileName) && config.SourceFileName != AppSettings.Instance.ConfigurationFileName)
            {
                AppSettings.Instance.ConfigurationFileName = config.SourceFileName;
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

            // Process entities to delete
            foreach (var keyToDelete in entitiesToDelete)
            {
                if (database.Entities.TryGetValue(keyToDelete, out var entity))
                {
                    if (config.Database?.DeleteObjectOnFilter == true)
                    {
                        entity.Properties?.Clear();
                        entity.Relationships?.Clear();
                        entity.RelationshipGroups?.Clear();
                        entity.PrimaryKeys?.Clear();
                        database.Entities.Remove(keyToDelete);
                    }
                    else
                    {
                        entity.IsEnabled = false;
                    }
                }
            }

            // Update entity aliases based on pattern
            foreach (var entitySchemaName in database.Keys)
            {
                if (string.IsNullOrEmpty(entitySchemaName)) continue;

                if (database.Entities.TryGetValue(entitySchemaName, out var entity) && entity != null)
                {
                    var aliasPattern = config.Database?.AliasNamePattern ?? string.Empty;
                    var databaseObjectName = entity.DatabaseObjectName ?? string.Empty;
                    entity.TableAlias = StringExtensions.ToCodeFriendly(Configuration.ReplaceEx(aliasPattern, databaseObjectName));
                    if (entity.Properties == null) continue;

                    foreach (var propertyKey in entity.Properties.Keys.ToList())
                    {
                        if (string.IsNullOrEmpty(propertyKey)) continue;

                        if (entity.Properties.TryGetValue(propertyKey, out var property) && property != null)
                        {
                            if (config.IsNotMappedColumn(property))
                            {
                                property.Set("NotMapped", true);
                            }

                            if (config.IsComputedColumn(property))
                            {
                                property.Set("Computed", true);
                            }

                            if (config.IsIgnoredColumn(property) || config.IsObjectNameFiltered(new SchemaObjectName(entity).AsFullName(), propertyKey))
                            {
                                if (config.Database?.DeleteObjectOnFilter == true)
                                {
                                    entity.Properties.Remove(propertyKey);
                                }
                                else
                                {
                                    property.IsEnabled = false;
                                }
                            }
                        }
                    }
                }
            }

            // Process entity overrides
            if (config.Entities != null)
            {
                foreach (var configEntity in config.Entities)
                {
                    if (configEntity == null || string.IsNullOrEmpty(configEntity.Name)) continue;

                    var entitiesMatched = database.FindEntities(configEntity.Name);
                    if (entitiesMatched?.Count > 0)
                    {
                        foreach (var entity in entitiesMatched)
                        {
                            if (entity == null) continue;

                            // Process primary key overrides
                            if (configEntity.Overrides?.PrimaryKey?.Count > 0)
                            {
                                if (entity.PrimaryKeys != null)
                                {
                                    foreach (var pkCol in entity.PrimaryKeys)
                                    {
                                        if (pkCol != null)
                                        {
                                            pkCol.IsPrimaryKey = false;
                                            pkCol.PrimaryKeyOrder = 0;
                                        }
                                    }
                                    entity.PrimaryKeys.Clear();

                                    var order = 0;
                                    foreach (var pkOverride in configEntity.Overrides.PrimaryKey)
                                    {
                                        if (pkOverride == null || string.IsNullOrEmpty(pkOverride.FieldName)) continue;

                                        order++;
                                        if (entity.Properties?.ContainsKey(pkOverride.FieldName) == true)
                                        {
                                            var property = entity.Properties[pkOverride.FieldName];
                                            if (property != null)
                                            {
                                                property.IsPrimaryKey = true;
                                                property.PrimaryKeyOrder = order;
                                                entity.PrimaryKeys.Add(property);
                                            }
                                        }
                                        else
                                        {
                                            throw new ArgumentException($"Column '{pkOverride.FieldName}' not found in entity '{configEntity.Name}'.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Process relationships
            foreach (var entity in database.Entities.Values)
            {
                if (entity == null) continue;
                if (entity.Relationships == null) continue;

                var relationshipsToDelete = new List<string>();
                foreach (var relationship in entity.Relationships)
                {
                    if (relationship == null) continue;

                    // Disable relationships for disabled entities
                    if (!entity.IsEnabled && !config.Database.DeleteObjectOnFilter)
                    {
                        relationship.IsEnabled = false;
                        continue;
                    }

                    // Process enabled relationships
                    if (relationship.IsEnabled)
                    {
                        var constraintName = relationship.ConstraintName;
                        if (!string.IsNullOrEmpty(constraintName) && 
                            config.IsObjectNameFiltered(new SchemaObjectName(entity).AsFullName(), constraintName))
                        {
                            relationship.IsEnabled = false;

                            if (entity.RelationshipGroups?.ContainsKey(constraintName) == true)
                            {
                                entity.RelationshipGroups.IsEnabled = false;
                            }

                            relationshipsToDelete.Add(constraintName);
                        }
                    }
                }

                // Process relationships to delete
                if (config.Database?.DeleteObjectOnFilter == true && relationshipsToDelete.Any())
                {
                    foreach (var relToDelete in relationshipsToDelete)
                    {
                        if (string.IsNullOrEmpty(relToDelete)) continue;

                        // Remove relationships
                        var relationsToRemove = entity.Relationships
                            .Where(r => r != null && relToDelete.Equals(r.ConstraintName))
                            .ToList();

                        foreach (var rel in relationsToRemove)
                        {
                            entity.Relationships.Remove(rel);
                        }

                        // Remove relationship groups
                        if (entity.RelationshipGroups?.ContainsKey(relToDelete) == true)
                        {
                            entity.RelationshipGroups.Remove(relToDelete);
                        }
                    }
                }
            }

            return database;
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
    }
}
