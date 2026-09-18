namespace Expectantly.Tests.ObjectAssertions;

public class IsNotTests
{
    [Fact]
    public void IsNot_WhenValuesDiffer_Passes()
    {
        var chain = global::Expectantly.Expect.That(42).IsNot(7).And;

        Assert.Equal(42, chain.Actual);
    }

    [Fact]
    public void IsNot_WhenValuesMatch_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That(42).IsNot(42));

        Assert.Contains("Did not expect <42>", ex.Message);
    }
}
