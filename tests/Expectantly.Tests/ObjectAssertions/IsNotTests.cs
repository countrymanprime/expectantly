namespace Expectantly.Tests.ObjectAssertions;

public class IsNotTests
{
    [Fact]
    public void IsNot_WhenValuesDiffer_Passes()
    {
        var chain = global::Expectantly.Expect.That("left").IsNot("right").And;

        Assert.Equal("left", chain.Actual);
    }

    [Fact]
    public void IsNot_WhenValuesAreEqual_ThrowsExpectationFailedException()
    {
        var answer = 42;

        var message = MessageOf(() => global::Expectantly.Expect.That(answer).IsNot(42));

        Assert.Equal("Expected answer not to be 42, but it was.", message);
    }

    [Fact]
    public void IsNot_WhenComparerEquatesDifferentText_ShowsWhatWasFound()
    {
        var name = "ABC";

        var message = MessageOf(() => global::Expectantly.Expect.That(name).IsNot("abc", StringComparer.OrdinalIgnoreCase));

        Assert.Equal("Expected name not to be \"abc\", but found \"ABC\", which the comparer considers equal.", message);
    }

    [Fact]
    public void IsNot_WithNullComparer_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => global::Expectantly.Expect.That("abc").IsNot("xyz", comparer: null!));

        Assert.Equal("comparer", exception.ParamName);
    }
}
