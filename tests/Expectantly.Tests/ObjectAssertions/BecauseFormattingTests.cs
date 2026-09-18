namespace Expectantly.Tests.ObjectAssertions;

public class BecauseFormattingTests
{
    [Fact]
    public void Is_AppendsFormattedBecauseClauseToFailureMessage()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            global::Expectantly.Expect.That(42, "we expected {0}", 43).Is(43));

        Assert.Contains("because we expected 43", ex.Message);
    }
}
