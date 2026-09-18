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
    public void IsTrue_WhenValueIsFalse_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That(false).IsTrue());

        Assert.Contains("Expected <true> but found <False>", ex.Message);
    }

    [Fact]
    public void IsFalse_WhenValueIsFalse_Passes()
    {
        var chain = global::Expectantly.Expect.That(false).IsFalse().And;

        Assert.False(chain.Actual);
    }

    [Fact]
    public void IsFalse_WhenValueIsTrue_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => global::Expectantly.Expect.That(true).IsFalse());

        Assert.Contains("Expected <false> but found <True>", ex.Message);
    }
}
