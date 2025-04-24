using System;
using System.Collections.Generic;
using EzDbSchema.Core.V2.Interfaces;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat
{
    /// <summary>
    /// V1 compatibility implementation of IEntityDiff
    /// </summary>
    public class EntityDiff
    {
        /// <summary>
        /// Create a new EntityDiff for the entity
        /// </summary>
        public EntityDiff(IEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            Entity = entity;
        }
        
        /// <summary>
        /// The entity that has changed
        /// </summary>
        public IEntity Entity { get; }
        
        /// <summary>
        /// Returns the name of the entity for convenient access
        /// </summary>
        public string Name => Entity.Name;
    }
}
