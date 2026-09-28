namespace Expectantly.Tests.Failures;

public class ReasonTests
{
    [Theory]
    [InlineData("tax is included")]
    [InlineData("because tax is included")]
    [InlineData("Because tax is included.")]
    [InlineData("  tax is included  ")]
    public void Because_IsWrittenOnceWithoutATrailingFullStop(string because)
    {
        var total = 42;

        var message = MessageOf(() => global::Expectantly.Expect.That(total).Is(43, because));

        Assert.Equal("Expected total to be 43 because tax is included, but found 42.", message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("because")]
    public void Because_WhenEmpty_IsLeftOut(string because)
    {
        var total = 42;

        var message = MessageOf(() => global::Expectantly.Expect.That(total).Is(43, because));

        Assert.Equal("Expected total to be 43, but found 42.", message);
    }

    [Fact]
    public void Because_WithBraces_IsWrittenAsIs()
    {
        var total = 42;

        var message = MessageOf(() => global::Expectantly.Expect.That(total).Is(43, "the payload is {\"a\": 1}"));

        Assert.Equal("Expected total to be 43 because the payload is {\"a\": 1}, but found 42.", message);
    }
}
