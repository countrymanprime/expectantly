namespace Expectantly.Tests.ObjectAssertions;

public class NullAssertionsTests
{
    [Fact]
    public void IsNull_WhenValueIsNull_Passes()
    {
        string? name = null;

        var chain = global::Expectantly.Expect.That(name).IsNull().And;

        Assert.Null(chain.Actual);
    }

    [Fact]
    public void IsNull_WhenValueIsNotNull_ThrowsExpectationFailedException()
    {
        var name = "x";

        var message = MessageOf(() => global::Expectantly.Expect.That(name).IsNull());

        Assert.Equal("Expected name to be null, but found \"x\".", message);
    }

    [Fact]
    public void IsNotNull_WhenValueIsNotNull_ReturnsTheValueAsWhich()
    {
        var name = "x";

        var which = global::Expectantly.Expect.That(name).IsNotNull().Which;

        Assert.Same(name, which);
    }

    [Fact]
    public void IsNotNull_WhenValueIsNull_ThrowsExpectationFailedException()
    {
        string? name = null;

        var message = MessageOf(() => global::Expectantly.Expect.That(name).IsNotNull());

        Assert.Equal("Expected name not to be null, but it was.", message);
    }
}
