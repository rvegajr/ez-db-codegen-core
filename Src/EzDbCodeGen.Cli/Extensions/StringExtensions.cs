using System;
using System.Text.RegularExpressions;

namespace EzDbCodeGen.Cli.Extensions
{
    /// <summary>
    /// Extension methods for string operations specific to CLI functionality
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Resolves settings placeholders in a string
        /// </summary>
        /// <param name="input">The input string containing placeholders</param>
        /// <returns>The string with placeholders replaced by their values</returns>
        public static string SettingResolution(this string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Replace environment variables in the format ${ENV_VAR}
            string result = Regex.Replace(input, @"\$\{([^}]+)\}", match =>
            {
                string envVarName = match.Groups[1].Value;
                string? envVarValue = Environment.GetEnvironmentVariable(envVarName);
                return envVarValue ?? match.Value;
            });

            return result;
        }
    }
}
