using EzDbCodeGen.Core.Enums;
using EzDbCodeGen.Core.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EzDbCodeGen.Tests.Services;

public class MsSqlConnectionTesterTests
{
    private readonly MsSqlConnectionTester _tester;

    public MsSqlConnectionTesterTests()
    {
        _tester = new MsSqlConnectionTester();
    }

    [Fact]
    public async Task TestConnectionAsync_EmptyConnectionString_ReturnsError()
    {
        // Arrange
        var connectionString = string.Empty;

        // Act
        var result = await _tester.TestConnectionAsync(connectionString);

        // Assert
        Assert.Equal(ReturnCode.Error, result.Code);
        Assert.Contains("Connection string cannot be empty", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_NullConnectionString_ReturnsError()
    {
        // Arrange
        string? connectionString = null;

        // Act
        var result = await _tester.TestConnectionAsync(connectionString!);

        // Assert
        Assert.Equal(ReturnCode.Error, result.Code);
        Assert.Contains("Connection string cannot be empty", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_MissingDatabase_ReturnsError()
    {
        // Arrange
        var connectionString = "Server=localhost;User Id=sa;Password=test";

        // Act
        var result = await _tester.TestConnectionAsync(connectionString);

        // Assert
        Assert.Equal(ReturnCode.Error, result.Code);
        Assert.Contains("Database name not specified", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_InvalidConnectionStringFormat_ReturnsError()
    {
        // Arrange
        var connectionString = "This is not a valid connection string";

        // Act
        var result = await _tester.TestConnectionAsync(connectionString);

        // Assert
        Assert.Equal(ReturnCode.Error, result.Code);
        Assert.Contains("Invalid connection string format", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_ServerNotReachable_ReturnsError()
    {
        // Arrange
        var connectionString = "Server=nonexistentserver;Database=TestDb;User Id=sa;Password=test;Connection Timeout=1";

        // Act
        var result = await _tester.TestConnectionAsync(connectionString);

        // Assert
        Assert.Equal(ReturnCode.Error, result.Code);
        Assert.Contains("Connection failed", result.Message);
    }

    [Fact]
    public async Task TestConnectionAsync_InvalidCredentials_ReturnsError()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=TestDb;User Id=sa;Password=wrongpassword;TrustServerCertificate=True";

        // Act
        var result = await _tester.TestConnectionAsync(connectionString);

        // Assert
        Assert.Equal(ReturnCode.Error, result.Code);
        Assert.Contains("Login failed", result.Message);
    }
}
