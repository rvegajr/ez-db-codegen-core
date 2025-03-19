using Xunit;

namespace EzDbCodeGen.Tests;

public class CoreTests
{
    [Fact]
    public void BasicTest_ShouldPass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }
}
