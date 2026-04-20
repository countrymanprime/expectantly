namespace Expectantly.Tests.ObjectAssertions;

public class IsTests
{
    [Fact]
    public void Is_WhenValuesAreEqual_Passes()
    {
        var chain = global::Expectantly.Expect.That(42).Is(42).And;

        Assert.Equal(42, chain.Actual);
    }

    [Fact]
    public void Is_WhenValuesDiffer_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That(42).Is(43));

        Assert.Contains("Expected <43> but found <42>", ex.Message);
    }
}
