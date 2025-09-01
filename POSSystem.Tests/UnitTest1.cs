namespace POSSystem.Tests;

public class BasicTests
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
