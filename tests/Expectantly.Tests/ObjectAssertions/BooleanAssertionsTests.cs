namespace Expectantly.Tests.ObjectAssertions;

public class BooleanAssertionsTests
{
    [Fact]
    public void IsTrue_WhenValueIsTrue_Passes()
    {
        var chain = global::Expectantly.Expect.That(true).IsTrue().And;

        Assert.True(chain.Actual);
    }

    [Fact]
    public void IsTrue_WhenValueIsFalse_ThrowsExpectationFailedException()
    {
        var ready = false;

        var message = MessageOf(() => global::Expectantly.Expect.That(ready).IsTrue());

        Assert.Equal("Expected ready to be true, but found false.", message);
    }

    [Fact]
    public void IsTrue_WhenValueIsNotABool_NamesItsType()
    {
        var answer = "yes";

        var message = MessageOf(() => global::Expectantly.Expect.That(answer).IsTrue());

        Assert.Equal("Expected answer to be true, but found \"yes\", which is a string, not a bool.", message);
    }

    [Fact]
    public void IsFalse_WhenValueIsFalse_Passes()
    {
        var chain = global::Expectantly.Expect.That(false).IsFalse().And;

        Assert.False(chain.Actual);
    }

    [Fact]
    public void IsFalse_WhenValueIsTrue_ThrowsExpectationFailedException()
    {
        var done = true;

        var message = MessageOf(() => global::Expectantly.Expect.That(done).IsFalse());

        Assert.Equal("Expected done to be false, but found true.", message);
    }

    [Fact]
    public void IsFalse_WhenNullableValueIsNull_SaysSo()
    {
        bool? done = null;

        var message = MessageOf(() => global::Expectantly.Expect.That(done).IsFalse());

        Assert.Equal("Expected done to be false, but found null.", message);
    }
}
