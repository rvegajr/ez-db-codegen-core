using Xunit;
using EzDbCodeGen.Core.Extensions;

namespace EzDbCodeGen.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("dbo.Customer", "dbo", "Customer")]
    [InlineData("Sales.Order", "Sales", "Order")]
    [InlineData("HR.Employee", "HR", "Employee")]
    public void ExtractDatabaseObject_WithValidInput_ExtractsCorrectly(string input, string expectedSchema, string expectedTable)
    {
        // Act
        var schema = input.ExtractSchemaName();
        var table = input.ExtractTableName();
        
        // Assert
        Assert.Equal(expectedSchema, schema);
        Assert.Equal(expectedTable, table);
    }
}
