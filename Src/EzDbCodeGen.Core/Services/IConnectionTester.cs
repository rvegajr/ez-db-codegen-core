using EzDbCodeGen.Core.Enums;
using System.Threading.Tasks;

namespace EzDbCodeGen.Core.Services;

/// <summary>
/// Interface for database connection testing
/// </summary>
public interface IConnectionTester
{
    /// <summary>
    /// Tests a database connection string and provides detailed feedback
    /// </summary>
    /// <param name="connectionString">The connection string to test</param>
    /// <param name="timeoutSeconds">Optional timeout in seconds. If not specified, uses default timeout from connection string</param>
    /// <returns>A tuple containing the return code and a detailed message</returns>
    Task<(ReturnCode Code, string Message)> TestConnectionAsync(string connectionString, int? timeoutSeconds = null);
}
