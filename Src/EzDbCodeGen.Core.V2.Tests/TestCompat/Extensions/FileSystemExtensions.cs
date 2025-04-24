using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;

namespace EzDbCodeGen.Core.V2.Tests.TestCompat.Extensions
{
    /// <summary>
    /// Extension methods for IFileSystem compatibility with V1 tests
    /// Following interface-first approach and proper null handling practices
    /// </summary>
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Writes text to a file (V1 compatibility)
        /// </summary>
        public static void WriteAllText(this IFileSystem fileSystem, string path, string content)
        {
            // Follow proper null handling practices
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            
            // Call the V2 async method synchronously
            fileSystem.WriteAllTextAsync(path, content).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Writes text to a file with encoding (V1 compatibility)
        /// </summary>
        public static void WriteAllText(this IFileSystem fileSystem, string path, string content, Encoding encoding)
        {
            // Follow proper null handling practices
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
            
            // Use standard File class to ensure encoding is preserved
            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write the file with encoding
            File.WriteAllText(path, content, encoding);
        }
        
        /// <summary>
        /// Reads all text from a file (V1 compatibility)
        /// </summary>
        public static string ReadAllText(this IFileSystem fileSystem, string path)
        {
            // Follow proper null handling practices
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            
            // Call the V2 async method synchronously
            return fileSystem.ReadAllTextAsync(path).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Reads all text from a file with encoding (V1 compatibility)
        /// </summary>
        public static string ReadAllText(this IFileSystem fileSystem, string path, Encoding encoding)
        {
            // Follow proper null handling practices
            ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
            
            // Use standard File class to ensure encoding is preserved
            return File.ReadAllText(path, encoding);
        }
    }
}
