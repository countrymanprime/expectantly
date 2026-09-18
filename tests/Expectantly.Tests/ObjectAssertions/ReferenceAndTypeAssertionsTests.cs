namespace Expectantly.Tests.ObjectAssertions;

public class ReferenceAndTypeAssertionsTests
{
    [Fact]
    public void IsSameAs_WhenReferenceMatches_Passes()
    {
        var list = new List<int>();

        var chain = global::Expectantly.Expect.That(list).IsSameAs(list).And;

        Assert.Same(list, chain.Actual);
    }

    [Fact]
    public void IsSameAs_WhenReferenceDiffers_ThrowsInvalidOperationException()
    {
        var list = new List<int>();

        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That(list).IsSameAs(new List<int>()));

        Assert.Contains("Expected references to match", ex.Message);
    }

    [Fact]
    public void IsAssignableTo_WhenTypeIsCompatible_Passes()
    {
        var chain = global::Expectantly.Expect.That("value").IsAssignableTo<object>().And;

        Assert.Equal("value", chain.Actual);
    }

    [Fact]
    public void IsAssignableTo_WhenTypeIsNotCompatible_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That("value").IsAssignableTo<int>());

        Assert.Contains("to be assignable to Int32", ex.Message);
    }
}
