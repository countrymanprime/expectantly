namespace Expectantly.Tests.ObjectAssertions;

public class NullAssertionsTests
{
    [Fact]
    public void IsNull_WhenValueIsNull_Passes()
    {
        string? value = null;

        var chain = global::Expectantly.Expect.That(value).IsNull().And;

        Assert.Null(chain.Actual);
    }

    [Fact]
    public void IsNull_WhenValueIsNotNull_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That("value").IsNull());

        Assert.Contains("Expected <null> but found <value>", ex.Message);
    }

    [Fact]
    public void IsNotNull_WhenValueIsNotNull_Passes()
    {
        var chain = global::Expectantly.Expect.That("value").IsNotNull().And;

        Assert.Equal("value", chain.Actual);
    }

    [Fact]
    public void IsNotNull_WhenValueIsNull_ThrowsInvalidOperationException()
    {
        string? value = null;

        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That(value).IsNotNull());

        Assert.Contains("Expected a non-null value", ex.Message);
    }
}
