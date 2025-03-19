using System;
using System.Collections.Generic;
using EzDbCodeGen.Core.Config;
using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Tests.CLI
{
    /// <summary>
    /// Extension methods for the IDatabase interface
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Filters the database based on the configuration
        /// </summary>
        /// <param name="database">The database to filter</param>
        /// <param name="config">The configuration to use for filtering</param>
        /// <returns>The filtered database</returns>
        public static IDatabase Filter(this IDatabase database, Configuration config)
        {
            // In a real implementation, this would filter the database based on the configuration
            // For testing, we'll just return the original database
            return database;
        }
    }
}
