﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Interfaces;
using Newtonsoft.Json;

namespace EzDbCodeGen.Core
{
    public class SchemaObjectName
    {
        public string SchemaName = "";
        public string ObjectName = "";
        public SchemaObjectName(IEntity entity)
        {
            SchemaName = entity.Schema ?? Configuration.Instance.Database.DefaultSchema;
            ObjectName = entity.Name ?? "";
        }

        public SchemaObjectName(string schemaObjectName)
        {
            SchemaName = Configuration.Instance.Database.DefaultSchema;
            if (SchemaName.Length == 0) SchemaName = "dbo";
            if (schemaObjectName.Contains("."))
            {
                var arr = schemaObjectName.Split('.');
                SchemaName = arr[0];
                ObjectName = arr[1];
            } else
            {
                ObjectName = schemaObjectName;
            }
        }

        /// <summary>
        /// Will return this object as a fully qualified string SchemaName.ObjectName
        /// </summary>
        /// <returns></returns>
        public string AsFullName()
        {
            return SchemaName + "." + ObjectName;
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
        public static IDatabase Filter(this IDatabase database)
        {
            return database.Filter(Configuration.Instance);
        }

        /// <summary>
        /// This will filter a schema based on the a passed configuration file.  This will remove entites that will need to be ignored and alter primary keys based
        /// on the parameters passed 
        /// </summary>
        /// <returns>An altered copy of the database</returns>
        /// <param name="database">Database.</param>
        /// <param name="config">Configuration file</param>
        public static IDatabase Filter(this IDatabase database, Configuration config)
        {
            //Rename the aliases of each to the pattern specified in the AliasNamePattern
            foreach (var entity in database.Entities.Values)
            {
                entity.Alias = Configuration.ReplaceEx(config.Database.AliasNamePattern, new SchemaObjectName(entity));
            }

            //Now we have to make sure there are no Property.Alias fields that have the same name as their parent Entity Alias field (since these will be the column name)
            foreach (var entity in database.Entities.Values)
            {
                foreach (var property in entity.Properties.Values)
                {
                    if ((!string.IsNullOrEmpty(property.Alias)) && (property.Alias.Equals(entity.Alias)))
                    {
                        property.Alias += config.Database.PropertyObjectNameCollisionSuffix;
                    }
                    property.Alias = property.Alias.ToCodeFriendly();
                }
            }

            //Use config settings to remove those entities we want out filtered out,  the wild card can affect these selections
            var DeleteList = new List<string>();
            foreach (var entityKey in database.Entities.Keys)
            {
                if (config.IsIgnoredEntity(entityKey)) DeleteList.Add(entityKey);
            }
            foreach( var keyToDelete in DeleteList)
            {
                database.Entities.Remove(keyToDelete);
            }

            // go through each matching entity and delete all keys don't match the override
            foreach (var configEntity in config.Entities)
            {
                var entitiesMatched = database.FindEntities(configEntity.Name);
                if (entitiesMatched.Count>0)
                {
                    foreach (var entity in entitiesMatched)
                    {
                        if (entity.Name.ToLower().Contains("tempload"))
                        {
                            Console.Write("");
                        }

                        //if we have some primary 
                        if (configEntity.Overrides.PrimaryKey.Count > 0)
                        {
                            foreach (var pkCol in entity.PrimaryKeys)
                            {
                                pkCol.IsKey = false;
                                pkCol.KeyOrder = 0;
                            }
                            entity.PrimaryKeys.Clear();
                            var iOrder = 0;
                            foreach (var pkOverride in configEntity.Overrides.PrimaryKey)
                            {
                                iOrder++;
                                if (entity.Properties.ContainsKey(pkOverride.FieldName))
                                {
                                    IProperty p = entity.Properties[pkOverride.FieldName];
                                    p.IsKey = true;
                                    p.KeyOrder = iOrder;
                                    entity.PrimaryKeys.Add(p);
                                }
                                else
                                {
                                    throw new Exception(string.Format("Could not find a column with the name {0} in {1}", pkOverride.FieldName, configEntity.Name));
                                }
                            }
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
        public static bool EntityExists(this IDatabase _database, string SchemaObjectName, ref IEntity entity) {
            entity = _database.FindEntity(SchemaObjectName);
            return (entity != null);
        }

        /// <summary>
        /// Using a config filtered database object,  this will search for an entity in the database table.
        /// </summary>
        /// <returns>An altered copy of the database</returns>
        /// <param name="_database">Database.</param>
        /// <param name="config"></param>
        public static IEntity FindEntity(this IDatabase database, string _schemaObjectName)
        {
            var schemaObjectName = new SchemaObjectName(_schemaObjectName);

            //Use config settings to 
            foreach (var entity in database.Entities.Values)
            {
                if ((entity.Schema.ToLower() == schemaObjectName.SchemaName.ToLower()) 
                    && (entity.Name.ToLower() == schemaObjectName.ObjectName.ToLower()))
                {
                    return entity;
                }
            }
            return null;
        }

        /// <summary>
        /// Using a config filtered database object,  this will search for all entities that match a certain criteria in the database table.
        /// </summary>
        /// <returns>An altered copy of the database</returns>
        /// <param name="_database">Database.</param>
        /// <param name="schemaObjectNameSearchParm">Search parm that can be used to find a list of entities</param>
        public static List<IEntity> FindEntities(this IDatabase database, string schemaObjectNameSearchParm)
        {
            var listOfMatchedEntites = new List<IEntity>();
            var isMatch = false;
            //Use config settings to 
            foreach (var entity in database.Entities.Values)
            {
                if (entity.Name.ToLower().Contains("tempload"))
                {
                    entity.Name += "";
                }
                var entitySchemaObjectName = new SchemaObjectName(entity);
                if (schemaObjectNameSearchParm.Contains(@"*")) //contains wildcard?
                {
                    isMatch = Regex.IsMatch(entitySchemaObjectName.AsFullName().ToLower(), "^" + Regex.Escape(schemaObjectNameSearchParm.ToLower()).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                } else {
                    isMatch = (entitySchemaObjectName.AsFullName().ToLower().Equals(schemaObjectNameSearchParm.ToLower()));
                }
                if (isMatch) listOfMatchedEntites.Add(entity);
            }
            return listOfMatchedEntites;
        }

    }
}
