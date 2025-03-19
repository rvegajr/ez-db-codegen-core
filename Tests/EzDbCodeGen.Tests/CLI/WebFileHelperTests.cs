using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using EzDbCodeGen.Cli;
using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Threading;

namespace EzDbCodeGen.Tests.CLI
{
    public class WebFileHelperTests
    {
        private readonly string _testOutputPath;

        public WebFileHelperTests()
        {
            // Set up test output path
            string assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            _testOutputPath = Path.Combine(assemblyLocation, "TestOutput");
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(_testOutputPath);
        }

        [Fact]
        public void CopyTo_WithValidPaths_CopiesFile()
        {
            // Arrange
            string sourceFileName = "test.txt";
            string sourcePath = Path.Combine(_testOutputPath, "source") + Path.DirectorySeparatorChar;
            string targetPath = Path.Combine(_testOutputPath, "target") + Path.DirectorySeparatorChar;
            
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(targetPath);
            
            string sourceFilePath = Path.Combine(sourcePath, sourceFileName);
            string targetFilePath = Path.Combine(targetPath, sourceFileName);
            
            // Create a test file
            File.WriteAllText(sourceFilePath, "Test content");
            
            // Act
            WebFileHelper.CopyToPath(sourceFileName, sourcePath, targetPath);
            
            // Assert
            Assert.True(File.Exists(targetFilePath), "File should be copied to target path");
            Assert.Equal("Test content", File.ReadAllText(targetFilePath));
            
            // Clean up
            File.Delete(sourceFilePath);
            File.Delete(targetFilePath);
        }

        [Fact]
        public void CopyTo_WithCustomDestinationFileName_CopiesFileWithNewName()
        {
            // Arrange
            string sourceFileName = "test.txt";
            string destFileName = "renamed.txt";
            string sourcePath = Path.Combine(_testOutputPath, "source") + Path.DirectorySeparatorChar;
            string targetPath = Path.Combine(_testOutputPath, "target") + Path.DirectorySeparatorChar;
            
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(targetPath);
            
            string sourceFilePath = Path.Combine(sourcePath, sourceFileName);
            string targetFilePath = Path.Combine(targetPath, destFileName);
            
            // Create a test file
            File.WriteAllText(sourceFilePath, "Test content");
            
            // Act
            WebFileHelper.CopyToPath(sourceFileName, sourcePath, targetPath, destFileName);
            
            // Assert
            Assert.True(File.Exists(targetFilePath), "File should be copied to target path with new name");
            Assert.Equal("Test content", File.ReadAllText(targetFilePath));
            
            // Clean up
            File.Delete(sourceFilePath);
            File.Delete(targetFilePath);
        }

        [Fact]
        public void CopyTo_WithNonExistentSourceFile_DoesNotThrowException()
        {
            // Arrange
            string sourceFileName = "nonexistent.txt";
            string sourcePath = Path.Combine(_testOutputPath, "source") + Path.DirectorySeparatorChar;
            string targetPath = Path.Combine(_testOutputPath, "target") + Path.DirectorySeparatorChar;
            
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(targetPath);
            
            // Act & Assert - Should throw exception for non-existent file
            var exception = Record.Exception(() => 
                WebFileHelper.CopyToPath(sourceFileName, sourcePath, targetPath));
            
            Assert.NotNull(exception);
            Assert.IsType<FileNotFoundException>(exception);
        }

        [Fact]
        public void CopyTo_WithNullOrEmptyPaths_HandlesGracefully()
        {
            // Arrange
            string sourceFileName = "test.txt";
            string sourcePath = Path.Combine(_testOutputPath, "source") + Path.DirectorySeparatorChar;
            
            Directory.CreateDirectory(sourcePath);
            string sourceFilePath = Path.Combine(sourcePath, sourceFileName);
            File.WriteAllText(sourceFilePath, "Test content");
            
            // Act & Assert - Empty target path should throw ArgumentException
            var exception = Record.Exception(() => 
                WebFileHelper.CopyToPath(sourceFileName, sourcePath, ""));
            
            Assert.NotNull(exception);
            Assert.IsType<ArgumentException>(exception);
            
            // Clean up
            File.Delete(sourceFilePath);
        }

        [Fact]
        public async Task DownloadFile_WithValidUrl_DownloadsFile()
        {
            // Arrange - Use HttpClient testing pattern with mock handler
            string testContent = "Downloaded test content";
            
            // Create a mock HTTP handler that returns a successful response with test content
            var handlerMock = new Mock<HttpMessageHandler>();
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(testContent)
                });
            
            // Set up the HttpClient factory to use our mock handler
            WebFileHelper.SetHttpClientFactory(() => new HttpClient(handlerMock.Object));
            
            try
            {
                // Act
                string testUrl = "https://example.com/test.txt";
                string destPath = Path.Combine(_testOutputPath, "downloaded.txt");
                await WebFileHelper.DownloadFile(testUrl, destPath);
                
                // Assert
                Assert.True(File.Exists(destPath), "Downloaded file should exist");
                Assert.Equal(testContent, await File.ReadAllTextAsync(destPath));
                
                // Clean up
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }
            }
            finally
            {
                // Reset the HttpClient factory to its default implementation
                WebFileHelper.ResetHttpClientFactory();
            }
        }

        [Fact]
        public async Task DownloadFile_WithInvalidUrl_HandlesErrorGracefully()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });
            
            // Set up the HttpClient factory to use our mock handler
            WebFileHelper.SetHttpClientFactory(() => new HttpClient(handlerMock.Object));
            
            try
            {
                // Act & Assert - Should throw exception for not found
                string testUrl = "https://example.com/nonexistent.txt";
                string destPath = Path.Combine(_testOutputPath, "should-not-exist.txt");
                
                var exception = await Record.ExceptionAsync(() => 
                    WebFileHelper.DownloadFile(testUrl, destPath));
                
                Assert.NotNull(exception);
                Assert.IsType<HttpRequestException>(exception);
                Assert.False(File.Exists(destPath), "File should not be created for failed download");
            }
            finally
            {
                // Reset the HttpClient factory to its default implementation
                WebFileHelper.ResetHttpClientFactory();
            }
        }

        [Fact]
        public async Task DownloadFile_WithNetworkError_HandlesErrorGracefully()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));
            
            // Set up the HttpClient factory to use our mock handler
            WebFileHelper.SetHttpClientFactory(() => new HttpClient(handlerMock.Object));
            
            try
            {
                // Act & Assert - Should throw exception for network error
                string testUrl = "https://example.com/error.txt";
                string destPath = Path.Combine(_testOutputPath, "should-not-exist.txt");
                
                var exception = await Record.ExceptionAsync(() => 
                    WebFileHelper.DownloadFile(testUrl, destPath));
                
                Assert.NotNull(exception);
                Assert.IsType<HttpRequestException>(exception);
                Assert.False(File.Exists(destPath), "File should not be created for failed download");
            }
            finally
            {
                // Reset the HttpClient factory to its default implementation
                WebFileHelper.ResetHttpClientFactory();
            }
        }
    }
}
