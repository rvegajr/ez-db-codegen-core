using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EzDbCodeGen.Core.Config;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for the Configuration class.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets the entity configuration for a specific entity name.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="entityName">The name of the entity.</param>
        /// <returns>The entity configuration if found; otherwise, null.</returns>
        public static EzDbCodeGen.Core.Config.Entity? GetEntityConfiguration(this Configuration configuration, string entityName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(entityName))
                return null;

            // First try to find an exact match
            var entity = configuration.Entities?.FirstOrDefault(e => 
                string.Equals(e.Name, entityName, StringComparison.OrdinalIgnoreCase));

            // If no exact match, try to find a match using the schema and table name
            if (entity == null && entityName.Contains('.'))
            {
                var parts = entityName.Split('.');
                var tableName = parts.Length > 1 ? parts[1] : parts[0];
                
                entity = configuration.Entities?.FirstOrDefault(e => 
                    string.Equals(e.Name, tableName, StringComparison.OrdinalIgnoreCase));
            }

            return entity;
        }
    }
}
