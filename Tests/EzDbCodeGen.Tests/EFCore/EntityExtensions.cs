using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Tests.EFCore
{
    /// <summary>
    /// Extension methods for IEntity to provide compatibility with EzDbSchema.Core
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Gets the schema name for an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The database schema name, or "dbo" if not set</returns>
        public static string GetSchemaName(this IEntity entity)
        {
            return entity.DatabaseSchema ?? "dbo";
        }
    }
}
