using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Schema;
using EzDbCodeGen.Core.V2.Tests.Mocks;
using EzDbSchema.Core.V2.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// Compatibility extensions for SchemaDiff
    /// Following interface-first approach with proper null handling patterns
    /// </summary>
    public static class SchemaDiffCompat
    {
        /// <summary>
        /// Creates a SchemaDiff instance with proper dependency injection
        /// </summary>
        public static SchemaDiff CreateSchemaDiff(IFileSystem fileSystem)
        {
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            return new SchemaDiff(fileSystem);
        }
        
        /// <summary>
        /// V1 compatibility method for Compare that takes two databases
        /// </summary>
        public static ISchemaDiffResult Compare(this ISchemaDiff schemaDiff, IDatabase originalSchema, IDatabase newSchema)
        {
            ArgumentNullException.ThrowIfNull(schemaDiff, nameof(schemaDiff));
            ArgumentNullException.ThrowIfNull(originalSchema, nameof(originalSchema));
            ArgumentNullException.ThrowIfNull(newSchema, nameof(newSchema));
            
            // Call the V2 async method synchronously
            return schemaDiff.CompareDatabases(originalSchema, newSchema);
        }
        
        /// <summary>
        /// Gets the ChangedEntities from a ISchemaDiffResult
        /// </summary>
        public static IEnumerable<EntityDiff> ChangedEntities(this ISchemaDiffResult diffResult)
        {
            ArgumentNullException.ThrowIfNull(diffResult, nameof(diffResult));
            
            // Convert ISchemaDiffResult to V1 compatible EntityDiff objects
            // The entity property itself is what we care about
            return diffResult.ModifiedEntities
                .Select(entityDiff => new EntityDiff(entityDiff.Entity))
                .ToList();
        }
        
        /// <summary>
        /// Extension to provide an alias for IEntityDiffResult in case tests use IEntityDiff
        /// </summary>
        public class EntityDiff : IEntityDiffResult
        {
            private readonly IEntity _entity;
            
            public EntityDiff(IEntity entity)
            {
                _entity = entity;
            }
            
            public IEntity Entity => _entity;
            
            public IReadOnlyCollection<IProperty> AddedProperties => new List<IProperty>();
            
            public IReadOnlyCollection<IProperty> RemovedProperties => new List<IProperty>();
            
            public IReadOnlyCollection<IProperty> ModifiedProperties => new List<IProperty>();
            
            public IReadOnlyCollection<IRelationship> AddedRelationships => new List<IRelationship>();
            
            public IReadOnlyCollection<IRelationship> RemovedRelationships => new List<IRelationship>();
            
            public IReadOnlyCollection<IRelationship> ModifiedRelationships => new List<IRelationship>();
            
            public bool HasChanges => false;
        }
    }
}
