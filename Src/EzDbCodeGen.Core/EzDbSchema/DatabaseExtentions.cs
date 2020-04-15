using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extentions.Objects;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using Newtonsoft.Json;

namespace EzDbCodeGen.Core
{     
    public static class DatabaseExtentions
    {

        public static IEntity IsIgnored(this IEntity entity, bool ValueToSetTo)
        {
            var database = entity.Parent;
            var keyToDelete = entity.Name;
            if (entity.CustomAttributes == null) entity.CustomAttributes = new CustomAttributes();
            if (!entity.CustomAttributes.ContainsKey("IsIgnored"))
                entity.CustomAttributes.Add("IsIgnored", ValueToSetTo);
            else
                entity.CustomAttributes["IsIgnored"] = ValueToSetTo;
            return entity;
        }

        public static bool IsIgnored(this IEntity entity)
        {
            var database = entity.Parent;
            var keyToDelete = entity.Name;
            if (entity.CustomAttributes == null) return false;
            if (entity.CustomAttributes.ContainsKey("IsIgnored"))
                return entity.CustomAttributes["IsIgnored"].AsBoolean();
            return false;
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
            //Use config settings to remove those entities we want out filtered out,  the wild card can affect these selections
            var DeleteList = new List<string>();
            foreach (var entityKey in database.Entities.Keys)
            {
                if (config.IsIgnoredEntity(entityKey)) DeleteList.Add(entityKey);
            }
            foreach (var keyToDelete in DeleteList)
            {
                if (database.Entities[keyToDelete].CustomAttributes == null) database.Entities[keyToDelete].CustomAttributes = new CustomAttributes();
                if (!database.Entities[keyToDelete].CustomAttributes.ContainsKey("IsIgnored"))
                    database.Entities[keyToDelete].CustomAttributes.Add("IsIgnored", true);
                else
                    database.Entities[keyToDelete].CustomAttributes["IsIgnored"] = true;
            }

            //Rename the aliases of each to the pattern specified in the AliasNamePattern
            foreach (var entity in database.Entities.Values)
            {
                entity.Alias = Configuration.ReplaceEx(config.Database.AliasNamePattern, new SchemaObjectName(entity)).ToCodeFriendly();
                foreach (var propertyKey in entity.Properties.Keys)
                {
                    if (config.IsIgnoredColumn(entity.Properties[propertyKey]))
                    {
                        if (entity.Properties[propertyKey].CustomAttributes == null) entity.Properties[propertyKey].CustomAttributes = new CustomAttributes();
                        if (!entity.Properties[propertyKey].CustomAttributes.ContainsKey("IsIgnored"))
                            entity.Properties[propertyKey].CustomAttributes.Add("IsIgnored", true);
                        else
                            entity.Properties[propertyKey].CustomAttributes["IsIgnored"] = true;
                    }
                }
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
                    && (entity.Name.ToLower() == schemaObjectName.TableName.ToLower()))
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
