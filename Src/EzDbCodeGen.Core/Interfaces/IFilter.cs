using EzDbSchema.Core.Interfaces;

namespace EzDbCodeGen.Core.Interfaces
{
    /// <summary>
    /// Interface for database filtering operations
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Applies the filter to the specified database
        /// </summary>
        /// <param name="database">The database to filter</param>
        void Apply(IDatabase database);
    }
}
