using System;
using System.Collections.Generic;
using System.Linq;
using EzDbCodeGen.Core;

namespace EzDbCodeGen.Core.Extensions
{
    /// <summary>
    /// Extension methods for the EntityFileDictionary class.
    /// </summary>
    internal static class EntityFileDictionaryExtensions
    {
        /// <summary>
        /// Creates a clone of the primary keys in the dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to clone keys from.</param>
        /// <returns>A collection of the primary keys.</returns>
        public static IEnumerable<string> ClonePrimaryKeys(this EntityFileDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            return dictionary.Keys.ToList();
        }
    }
}
