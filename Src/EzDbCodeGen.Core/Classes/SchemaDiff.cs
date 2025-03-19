using EzDbCodeGen.Core.Enums;

namespace EzDbCodeGen.Core.Classes
{
    /// <summary>
    /// Represents a difference between two database schemas.
    /// </summary>
    public class SchemaDiff
    {
        /// <summary>
        /// Gets or sets the name of the entity that has changed.
        /// </summary>
        public required string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the file action to take (Add, Update, Delete).
        /// </summary>
        public TemplateFileAction FileAction { get; set; }

        /// <summary>
        /// Gets or sets the reason for the difference.
        /// </summary>
        public required string Reason { get; set; }
    }
}
