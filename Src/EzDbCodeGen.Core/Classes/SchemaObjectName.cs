using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzDbCodeGen.Core
{
    /// <summary>
    /// Represents a database object with schema and object name components
    /// </summary>
    public class SchemaObjectName
    {
        /// <summary>
        /// Gets or sets the schema name
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the object name
        /// </summary>
        public string ObjectName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the fully qualified name (schema.object)
        /// </summary>
        public string FullName => $"{SchemaName}.{ObjectName}";

        /// <summary>
        /// Gets the fully qualified name (schema.object) - alias for FullName for compatibility
        /// </summary>
        public string AsFullName() => FullName;

        /// <summary>
        /// Creates a new instance of SchemaObjectName
        /// </summary>
        public SchemaObjectName()
        {
        }

        /// <summary>
        /// Creates a new instance of SchemaObjectName with the specified schema and object names
        /// </summary>
        /// <param name="schemaName">The schema name</param>
        /// <param name="objectName">The object name</param>
        public SchemaObjectName(string schemaName, string objectName)
        {
            SchemaName = schemaName;
            ObjectName = objectName;
        }

        /// <summary>
        /// Creates a new instance of SchemaObjectName from a fully qualified name (schema.object)
        /// </summary>
        /// <param name="fullName">The fully qualified name in the format schema.object</param>
        public SchemaObjectName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return;

            var parts = fullName.Split(new[] { '.' }, 2);
            if (parts.Length == 2)
            {
                SchemaName = parts[0];
                ObjectName = parts[1];
            }
            else
            {
                // If no schema is specified, use the object name only
                ObjectName = fullName;
            }
        }

        /// <summary>
        /// Returns the fully qualified name
        /// </summary>
        public override string ToString()
        {
            return FullName;
        }
    }
}
