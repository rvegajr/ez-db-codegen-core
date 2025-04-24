using EzDbCodeGen.Core.V2.Interfaces;

namespace EzDbCodeGen.Core.V2.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IFileSystem for testing purposes
    /// </summary>
    public class MockFileSystem : IFileSystem
    {
        private readonly Dictionary<string, string> _mockFiles = new();
        
        /// <summary>
        /// Gets or sets the base directory
        /// </summary>
        public string BaseDirectory { get; set; } = "/mock/base/dir";
        
        /// <summary>
        /// Checks if a file exists
        /// </summary>
        public bool FileExists(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return _mockFiles.ContainsKey(path);
        }
        
        /// <summary>
        /// Checks if a directory exists
        /// </summary>
        public bool DirectoryExists(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return path.StartsWith("/mock/");
        }
        
        /// <summary>
        /// Creates a directory
        /// </summary>
        public void CreateDirectory(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            // No actual implementation needed for mock
        }
        
        /// <summary>
        /// Gets files in a directory
        /// </summary>
        public string[] GetFiles(string directoryPath, string searchPattern = "*", bool recursive = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(directoryPath, nameof(directoryPath));
            return _mockFiles.Keys.Where(k => k.StartsWith(directoryPath)).ToArray();
        }
        
        /// <summary>
        /// Gets subdirectories in a directory
        /// </summary>
        public string[] GetDirectories(string directoryPath, string searchPattern = "*", bool recursive = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(directoryPath, nameof(directoryPath));
            return new[] { "/mock/subdir1", "/mock/subdir2" };
        }
        
        /// <summary>
        /// Gets subdirectories in a directory with just directory path and recursive flag
        /// </summary>
        public string[] GetDirectories(string directoryPath, bool recursive = false)
        {
            return GetDirectories(directoryPath, "*", recursive);
        }
        
        /// <summary>
        /// Reads text from a file asynchronously
        /// </summary>
        public Task<string> ReadAllTextAsync(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            
            if (!FileExists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }
            
            return Task.FromResult(_mockFiles[path]);
        }
        
        /// <summary>
        /// Writes text to a file asynchronously
        /// </summary>
        public Task WriteAllTextAsync(string path, string contents)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            _mockFiles[path] = contents;
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Gets the directory name from a path
        /// </summary>
        public string GetDirectoryName(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return Path.GetDirectoryName(path) ?? string.Empty;
        }
        
        /// <summary>
        /// Gets the file name from a path
        /// </summary>
        public string GetFileName(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return Path.GetFileName(path);
        }
        
        /// <summary>
        /// Gets the full path
        /// </summary>
        public string GetFullPath(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return path.StartsWith("/") ? path : $"/{path}";
        }
        
        /// <summary>
        /// Gets the file name without extension
        /// </summary>
        public string GetFileNameWithoutExtension(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return Path.GetFileNameWithoutExtension(path);
        }
        
        /// <summary>
        /// Gets the extension from a path
        /// </summary>
        public string GetExtension(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            return Path.GetExtension(path);
        }
        
        /// <summary>
        /// Combines multiple paths into a single path
        /// </summary>
        public string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return string.Empty;
            }
            
            return string.Join("/", paths).Replace("//", "/");
        }
        
        /// <summary>
        /// Adds a mock file to the system
        /// </summary>
        public void AddMockFile(string path, string content)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            _mockFiles[path] = content;
        }
        
        /// <summary>
        /// Reads all text from a file
        /// </summary>
        public string ReadAllText(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            
            if (!FileExists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }
            
            return _mockFiles[path];
        }
        
        /// <summary>
        /// Writes all text to a file
        /// </summary>
        public void WriteAllText(string path, string contents)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            _mockFiles[path] = contents;
        }
    }
}
