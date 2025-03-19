using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Extensions;
using EzDbSchema.Core.Extentions;
using EzDbSchema.Core.Interfaces;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Core
{
    /// <summary>
    /// A dictionary that maps entity names to file names, using a hash-based key lookup for efficient retrieval.
    /// </summary>
    internal class EntityFileDictionary : Dictionary<string, string>
    {
        private readonly Dictionary<int, string> _hashToKey = new();

        /// <summary>
        /// Adds a new entity-file pair to the dictionary.
        /// </summary>
        /// <param name="key">The entity name.</param>
        /// <param name="value">The file name.</param>
        /// <exception cref="ArgumentException">Thrown if the key is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if a key with the same hash already exists.</exception>
        public new void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            var hashCode = key.GetStableHashCode();
            if (_hashToKey.ContainsKey(hashCode))
            {
                throw new ArgumentException($"A key with hash {hashCode} already exists: {_hashToKey[hashCode]}", nameof(key));
            }

            _hashToKey[hashCode] = key;
            base.Add(key, value);
        }

        /// <summary>
        /// Retrieves the file name associated with the given entity name hash.
        /// </summary>
        /// <param name="hashCode">The hash code of the entity name.</param>
        /// <returns>The file name associated with the entity name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no key is found for the given hash.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no value is found for the key.</exception>
        public string GetByHash(int hashCode)
        {
            if (!_hashToKey.TryGetValue(hashCode, out var key))
            {
                throw new KeyNotFoundException($"No key found for hash {hashCode}");
            }

            if (!TryGetValue(key, out var value))
            {
                throw new KeyNotFoundException($"No value found for key {key}");
            }

            return value;
        }

        /// <summary>
        /// Attempts to retrieve the file name associated with the given entity name hash.
        /// </summary>
        /// <param name="hashCode">The hash code of the entity name.</param>
        /// <param name="value">The file name associated with the entity name, or null if not found.</param>
        /// <returns>True if the file name was found, false otherwise.</returns>
        public bool TryGetByHash(int hashCode, out string? value)
        {
            value = null;
            if (!_hashToKey.TryGetValue(hashCode, out var key))
            {
                return false;
            }

            return TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes the entity-file pair with the given entity name.
        /// </summary>
        /// <param name="key">The entity name.</param>
        /// <returns>True if the pair was removed, false otherwise.</returns>
        public new bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var hashCode = key.GetStableHashCode();
            _hashToKey.Remove(hashCode);
            return base.Remove(key);
        }

        /// <summary>
        /// Removes the entity-file pair with the given entity name hash.
        /// </summary>
        /// <param name="hashCode">The hash code of the entity name.</param>
        /// <returns>True if the pair was removed, false otherwise.</returns>
        public bool RemoveByHash(int hashCode)
        {
            if (!_hashToKey.TryGetValue(hashCode, out var key))
            {
                return false;
            }

            _hashToKey.Remove(hashCode);
            return base.Remove(key);
        }

        /// <summary>
        /// Clears all entity-file pairs from the dictionary.
        /// </summary>
        public new void Clear()
        {
            _hashToKey.Clear();
            base.Clear();
        }

        /// <summary>
        /// Adds a new file with its entity key and contents to the dictionary.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="entityKey">The entity key.</param>
        /// <param name="fileContents">The file contents.</param>
        public void Add(string fileName, string entityKey, string fileContents)
        {
            // Add the file name and entity key to the dictionary
            Add(entityKey, fileName);
            
            // Store the file contents in a separate collection or process it as needed
            // This implementation depends on how the file contents are used in the application
            // For now, we'll just add the entity key and file name to the dictionary
        }
    }

    public class FileName
    {
        readonly string _value;
        public FileName(string value)
        {
            this._value = value;
        }
        public static implicit operator string(FileName d)
        {
            return d._value;
        }
        public static implicit operator FileName(string d)
        {
            return new FileName(d);
        }
        public override int GetHashCode()
        {
            return _value.GetStableHashCode();
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as FileName);
        }
        public bool Equals(FileName? obj)
        {
            return obj != null && obj.GetHashCode() == this.GetHashCode();
        }
    }

    internal class EntityName
    {
        readonly string _value;
        public EntityName(string value)
        {
            this._value = value;
        }
        public static implicit operator string(EntityName d)
        {
            return d._value;
        }
        public static implicit operator EntityName(string d)
        {
            return new EntityName(d);
        }

        public override int GetHashCode()
        {
            return _value.GetStableHashCode();
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as EntityName);
        }
        public bool Equals(EntityName? obj)
        {
            return obj != null && obj.GetHashCode() == this.GetHashCode();
        }

    }
}
