using System;
using System.IO;
using System.Text;
using FluentAssertions;
using EzDbCodeGen.Core.V2.Interfaces;
using EzDbCodeGen.Core.V2.Models;
using Moq;
using Xunit;

namespace EzDbCodeGen.Core.V2.Tests.Models
{
    public class FileSystemTests
    {
        private readonly string _testDirectory;
        private readonly string _testFile;

        public FileSystemTests()
        {
            // Setup a test directory and file path
            _testDirectory = Path.Combine(Path.GetTempPath(), "EzDbCodeGen_Tests", Guid.NewGuid().ToString());
            _testFile = Path.Combine(_testDirectory, "test.txt");
            
            // Ensure test directory exists
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public void Constructor_ShouldInitializeBaseDirectoryProperly()
        {
            // Arrange & Act
            var fileSystem = new FileSystem(_testDirectory);

            // Assert
            fileSystem.BaseDirectory.Should().Be(_testDirectory);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_WithInvalidBaseDirectory_ShouldThrowArgumentException(string? invalidDirectory)
        {
            // Arrange & Act & Assert
            Action act = () => new FileSystem(invalidDirectory!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("baseDirectory");
        }

        [Fact]
        public void Constructor_WithNonExistentDirectory_ShouldCreateDirectory()
        {
            // Arrange
            var nonExistentDir = Path.Combine(_testDirectory, "NonExistent");
            if (Directory.Exists(nonExistentDir))
            {
                Directory.Delete(nonExistentDir, true);
            }

            // Act
            var fileSystem = new FileSystem(nonExistentDir);

            // Assert
            Directory.Exists(nonExistentDir).Should().BeTrue();
            fileSystem.BaseDirectory.Should().Be(nonExistentDir);
        }

        [Fact]
        public void GetFullPath_ShouldCombineBaseDirectoryAndRelativePath()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var relativePath = "subfolder/file.txt";

            // Act
            var fullPath = fileSystem.GetFullPath(relativePath);

            // Assert
            fullPath.Should().Be(Path.Combine(_testDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFullPath_WithInvalidRelativePath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.GetFullPath(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("relativePath");
        }

        [Fact]
        public void WriteAllText_ShouldCreateFileWithContent()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var content = "Test content";

            // Act
            fileSystem.WriteAllText(_testFile, content);

            // Assert
            File.Exists(_testFile).Should().BeTrue();
            File.ReadAllText(_testFile).Should().Be(content);
        }

        [Fact]
        public void WriteAllText_WithNonexistentDirectory_ShouldCreateDirectoryAndFile()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var subDir = Path.Combine(_testDirectory, "subdir");
            var filePath = Path.Combine(subDir, "test.txt");
            var content = "Test content";

            if (Directory.Exists(subDir))
            {
                Directory.Delete(subDir, true);
            }

            // Act
            fileSystem.WriteAllText(filePath, content);

            // Assert
            Directory.Exists(subDir).Should().BeTrue();
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Be(content);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WriteAllText_WithInvalidFilePath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var content = "Test content";

            // Act & Assert
            Action act = () => fileSystem.WriteAllText(invalidPath!, content);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        [Fact]
        public void ReadAllText_ShouldReturnFileContent()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var expectedContent = "Test content";
            File.WriteAllText(_testFile, expectedContent);

            // Act
            var actualContent = fileSystem.ReadAllText(_testFile);

            // Assert
            actualContent.Should().Be(expectedContent);
        }

        [Fact]
        public void ReadAllText_WithNonexistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");

            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }

            // Act & Assert
            Action act = () => fileSystem.ReadAllText(nonExistentFile);
            act.Should().Throw<FileNotFoundException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ReadAllText_WithInvalidFilePath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.ReadAllText(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        [Fact]
        public void FileExists_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            File.WriteAllText(_testFile, "Test content");

            // Act
            var exists = fileSystem.FileExists(_testFile);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public void FileExists_WithNonexistentFile_ShouldReturnFalse()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");

            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }

            // Act
            var exists = fileSystem.FileExists(nonExistentFile);

            // Assert
            exists.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void FileExists_WithInvalidFilePath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.FileExists(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        [Fact]
        public void DirectoryExists_WithExistingDirectory_ShouldReturnTrue()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act
            var exists = fileSystem.DirectoryExists(_testDirectory);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public void DirectoryExists_WithNonexistentDirectory_ShouldReturnFalse()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var nonExistentDir = Path.Combine(_testDirectory, "nonexistent_dir");

            if (Directory.Exists(nonExistentDir))
            {
                Directory.Delete(nonExistentDir, true);
            }

            // Act
            var exists = fileSystem.DirectoryExists(nonExistentDir);

            // Assert
            exists.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DirectoryExists_WithInvalidDirectoryPath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.DirectoryExists(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("directoryPath");
        }

        [Fact]
        public void CreateDirectory_ShouldCreateNewDirectory()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var newDir = Path.Combine(_testDirectory, "new_dir");

            if (Directory.Exists(newDir))
            {
                Directory.Delete(newDir, true);
            }

            // Act
            fileSystem.CreateDirectory(newDir);

            // Assert
            Directory.Exists(newDir).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateDirectory_WithInvalidDirectoryPath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.CreateDirectory(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("directoryPath");
        }

        [Fact]
        public void GetFiles_ShouldReturnAllFilesInDirectory()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var file1 = Path.Combine(_testDirectory, "file1.txt");
            var file2 = Path.Combine(_testDirectory, "file2.txt");

            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");

            // Act
            var files = fileSystem.GetFiles(_testDirectory);

            // Assert
            files.Should().NotBeNull();
            files.Should().HaveCount(2);
            files.Should().Contain(file1);
            files.Should().Contain(file2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFiles_WithInvalidDirectoryPath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.GetFiles(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("directoryPath");
        }

        [Fact]
        public void GetFiles_WithNonexistentDirectory_ShouldThrowDirectoryNotFoundException()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var nonExistentDir = Path.Combine(_testDirectory, "nonexistent_dir");

            if (Directory.Exists(nonExistentDir))
            {
                Directory.Delete(nonExistentDir, true);
            }

            // Act & Assert
            Action act = () => fileSystem.GetFiles(nonExistentDir);
            act.Should().Throw<DirectoryNotFoundException>();
        }

        [Fact]
        public void GetFiles_WithSearchPattern_ShouldReturnMatchingFiles()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var file1 = Path.Combine(_testDirectory, "file1.txt");
            var file2 = Path.Combine(_testDirectory, "file2.json");

            File.WriteAllText(file1, "Content 1");
            File.WriteAllText(file2, "Content 2");

            // Act
            var txtFiles = fileSystem.GetFiles(_testDirectory, "*.txt");
            var jsonFiles = fileSystem.GetFiles(_testDirectory, "*.json");

            // Assert
            txtFiles.Should().NotBeNull();
            txtFiles.Should().HaveCount(1);
            txtFiles.Should().Contain(file1);
            txtFiles.Should().NotContain(file2);

            jsonFiles.Should().NotBeNull();
            jsonFiles.Should().HaveCount(1);
            jsonFiles.Should().Contain(file2);
            jsonFiles.Should().NotContain(file1);
        }

        [Fact]
        public void DeleteFile_ShouldRemoveFile()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            File.WriteAllText(_testFile, "Content");

            // Act
            fileSystem.DeleteFile(_testFile);

            // Assert
            File.Exists(_testFile).Should().BeFalse();
        }

        [Fact]
        public void DeleteFile_WithNonexistentFile_ShouldNotThrowException()
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);
            var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.txt");

            if (File.Exists(nonExistentFile))
            {
                File.Delete(nonExistentFile);
            }

            // Act & Assert
            Action act = () => fileSystem.DeleteFile(nonExistentFile);
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DeleteFile_WithInvalidFilePath_ShouldThrowArgumentException(string? invalidPath)
        {
            // Arrange
            var fileSystem = new FileSystem(_testDirectory);

            // Act & Assert
            Action act = () => fileSystem.DeleteFile(invalidPath!);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("filePath");
        }

        // Additional cleanup after all tests
        ~FileSystemTests()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
