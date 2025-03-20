using EzDbCodeGen.Core.Enums;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.MsSql;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace EzDbCodeGen.Core.Services;

/// <summary>
/// SQL Server implementation of connection testing
/// </summary>
public class MsSqlConnectionTester : IConnectionTester
{
    private const string PreLoginError = "pre-login handshake";
    private const string TrustFailureError = "certificate chain was issued by an authority";
    private const string TimeoutError = "timeout period elapsed";
    private const string FormatError = "Format of the initialization string";
    private const int DefaultTimeout = 30;

    /// <summary>
    /// Tests a SQL Server connection string and provides detailed feedback
    /// </summary>
    public async Task<(ReturnCode Code, string Message)> TestConnectionAsync(string connectionString, int? timeoutSeconds = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return (ReturnCode.Error, "Connection string cannot be empty");
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            
            // Apply timeout if specified
            if (timeoutSeconds.HasValue)
            {
                builder.ConnectTimeout = timeoutSeconds.Value;
            }
            else if (builder.ConnectTimeout <= 0)
            {
                builder.ConnectTimeout = DefaultTimeout;
            }

            if (string.IsNullOrEmpty(builder.InitialCatalog))
            {
                return (ReturnCode.Error, "Database name not specified in connection string");
            }

            using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            // Test if we can actually access the database
            using var cmd = connection.CreateCommand();
            cmd.CommandTimeout = builder.ConnectTimeout;
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync();

            return (ReturnCode.Ok, $"Connection test successful (timeout: {builder.ConnectTimeout}s)");
        }
        catch (ArgumentException ex) when (ex.Message.Contains(FormatError))
        {
            return (ReturnCode.Error, "Invalid connection string format: The connection string is not properly formatted");
        }
        catch (SqlException ex)
        {
            var errorMessage = ex.Message.ToLowerInvariant();
            var timeout = timeoutSeconds ?? DefaultTimeout;

            return ex.Number switch
            {
                4060 => (ReturnCode.Error, $"Database does not exist or is not accessible: {ex.Message}"),
                18456 => (ReturnCode.Error, $"Login failed. Please check your credentials."),
                _ when errorMessage.Contains(PreLoginError) => 
                    (ReturnCode.Error, $"Connection failed: Unable to establish secure connection. Check if SQL Server is running and accepting connections (timeout: {timeout}s)"),
                _ when errorMessage.Contains(TrustFailureError) => 
                    (ReturnCode.Error, "Connection failed: SSL/TLS certificate validation failed. Consider using 'TrustServerCertificate=True' for development."),
                _ when errorMessage.Contains(TimeoutError) => 
                    (ReturnCode.Error, $"Connection timed out after {timeout}s. Check if the server is accessible and the port is correct."),
                _ => (ReturnCode.Error, $"Connection failed: {ex.Message}")
            };
        }
        catch (Exception ex)
        {
            return (ReturnCode.Error, $"Unexpected error: {ex.Message}");
        }
    }
}
