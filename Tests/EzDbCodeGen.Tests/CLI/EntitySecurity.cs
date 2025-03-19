using System;

namespace EzDbCodeGen.Tests.CLI
{
    /// <summary>
    /// Represents security settings for an entity
    /// </summary>
    public class EntitySecurity
    {
        /// <summary>
        /// Gets or sets whether the entity is secured
        /// </summary>
        public bool Secured { get; set; }
        
        /// <summary>
        /// Gets or sets the security level
        /// </summary>
        public string SecurityLevel { get; set; } = "Default";
        
        /// <summary>
        /// Gets or sets whether the entity is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
