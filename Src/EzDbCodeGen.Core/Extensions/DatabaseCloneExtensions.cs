using System;
using System.Collections.Generic;
using System.Linq;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;
using Newtonsoft.Json;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for cloning database objects.
    /// </summary>
    public static class DatabaseCloneExtensions
    {
        /// <summary>
        /// Creates a deep clone of the database using JSON serialization.
        /// </summary>
        /// <param name="database">The database to clone.</param>
        /// <returns>A deep clone of the database.</returns>
        public static IDatabase DeepClone(this IDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            // Use Newtonsoft.Json for serialization to avoid conflicts with System.Text.Json
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            var json = JsonConvert.SerializeObject(database, settings);
            return JsonConvert.DeserializeObject<EzDbSchema.Core.Interfaces.IDatabase>(json, settings);
        }
    }
}
