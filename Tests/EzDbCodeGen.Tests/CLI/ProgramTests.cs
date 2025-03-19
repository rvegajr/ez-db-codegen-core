using System;
using System.IO;
using System.Reflection;
using System.Text;
using EzDbCodeGen.Core.Enums;
using Xunit;
using Moq;
using McMaster.Extensions.CommandLineUtils;

namespace EzDbCodeGen.Tests.CLI
{
    public class ProgramTests
    {
        private readonly string _testOutputPath;
        private readonly string _testTemplatesPath;

        public ProgramTests()
        {
            // Set up test paths
            string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            _testOutputPath = Path.Combine(assemblyLocation, "TestOutput");
            _testTemplatesPath = Path.Combine(assemblyLocation, "TestTemplates");
            
            // Create directories if they don't exist
            Directory.CreateDirectory(_testOutputPath);
            Directory.CreateDirectory(_testTemplatesPath);
        }

        [Fact]
        public void Main_WithInvalidArguments_ReturnsErrorCode()
        {
            // Arrange - Create a TextWriter to capture console output
            var consoleOutput = new StringWriter();
            Console.SetError(consoleOutput);

            // Act - Call the Main method with invalid arguments
            var result = ExecuteMainMethod(new[] { "--invalid-argument" });

            // Assert - Check that the return code indicates an error
            Assert.Equal((int)ReturnCode.Error, result);
            
            // Restore console
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        }

        [Fact]
        public void Main_WithHelpArgument_ReturnsSuccessCode()
        {
            // Arrange - Create a TextWriter to capture console output
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act - Call the Main method with help argument
            var result = ExecuteMainMethod(new[] { "--help" });

            // Assert - Check that the return code indicates success
            Assert.Equal((int)ReturnCode.Ok, result);
            
            // Restore console
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        [Fact]
        public void Main_WithVersionArgument_ReturnsSuccessCode()
        {
            // Arrange - Create a TextWriter to capture console output
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act - Call the Main method with version argument
            var result = ExecuteMainMethod(new[] { "--version" });

            // Assert - Check that the return code indicates success
            Assert.Equal((int)ReturnCode.Ok, result);
            
            // Restore console
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        /// <summary>
        /// Helper method to execute the Main method using reflection
        /// </summary>
        private int ExecuteMainMethod(string[] args)
        {
            // Get the Program type from the EzDbCodeGen.Cli assembly
            var programAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "EzDbCodeGen.Cli");
            
            if (programAssembly == null)
            {
                throw new InvalidOperationException("EzDbCodeGen.Cli assembly not found");
            }
            
            var programType = programAssembly.GetType("EzDbCodeGen.Cli.Program");
            if (programType == null)
            {
                throw new InvalidOperationException("Program class not found in EzDbCodeGen.Cli assembly");
            }
            
            // Get the Main method
            var mainMethod = programType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            if (mainMethod == null)
            {
                throw new InvalidOperationException("Main method not found in Program class");
            }
            
            // Invoke the Main method
            return (int)mainMethod.Invoke(null, new object[] { args })!;
        }
    }
}
