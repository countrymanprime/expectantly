namespace Expectantly.Tests.Failures;

public class StackTraceTests
{
    [Fact]
    public void FailedAssertion_StackTraceStartsInTheTest()
    {
        var answer = 42;

        var exception = Assert.Throws<ExpectationFailedException>(() => global::Expectantly.Expect.That(answer).Is(43));

        var firstFrame = exception.StackTrace!.Split('\n')[0];
        Assert.Contains(nameof(FailedAssertion_StackTraceStartsInTheTest), firstFrame);
    }
}
