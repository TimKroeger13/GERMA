using Xunit;

namespace GERMA.Tests;

public class ExampleTest
{
    [Fact]
    public void Addition_Works()
    {
        // Arrange
        int a = 2, b = 3;

        // Act
        int result = a + b;

        // Assert
        Assert.Equal(5, result);
    }
}
