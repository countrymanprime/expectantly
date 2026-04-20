namespace Expectantly.Tests.Expect;

public class ThatTests
{
    [Fact]
    public void That_WithValue_ReturnsAssertionForProvidedValue()
    {
        var assertion = global::Expectantly.Expect.That(42);

        Assert.Equal(42, assertion.Actual);
    }

    [Fact]
    public void That_WithFactory_EvaluatesFactoryOnce()
    {
        var calls = 0;

        var assertion = global::Expectantly.Expect.That(() =>
        {
            calls++;
            return "hello";
        });

        Assert.Equal("hello", assertion.Actual);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void That_WithNullFactory_ThrowsArgumentNullException()
    {
        Func<string>? factory = null;

        Assert.Throws<ArgumentNullException>(() => global::Expectantly.Expect.That(factory!));
    }
}
