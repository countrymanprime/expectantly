using Expectantly.Internal;

namespace Expectantly.Tests.Failures;

public class FailureTests
{
    [Fact]
    public void Message_WithAllParts_ReadsAsOneSentence()
    {
        var failure = new FailureBuilder("order.Total", "tax is included")
            .Expected("to be 43")
            .But("found 42")
            .Which("is one short")
            .Build();

        Assert.Equal("Expected order.Total to be 43 because tax is included, but found 42, which is one short.", failure.Message);
    }

    [Fact]
    public void Message_WithDetails_EndsTheSentenceWithAColonAndIndentsEachLine()
    {
        var failure = new FailureBuilder("text", because: null)
            .Expected("to be \"ab\"")
            .But("found \"ax\"")
            .Details(["\"ax\"", " ↑"])
            .Build();

        Assert.Equal("Expected text to be \"ab\", but found \"ax\":\n    \"ax\"\n     ↑", failure.Message);
    }

    [Fact]
    public void Failure_ExposesEachPartOfTheSentence()
    {
        var total = 42;

        var failure = FailureOf(() => global::Expectantly.Expect.That(total).Is(43, "tax is included"));

        Assert.Equal("total", failure.Subject);
        Assert.Equal("to be 43", failure.Expectation);
        Assert.Equal("tax is included", failure.Because);
        Assert.Equal("found 42", failure.Outcome);
        Assert.Null(failure.Which);
        Assert.Empty(failure.Details);
    }

    [Fact]
    public void ExpectationFailedException_UsesTheFailureMessage()
    {
        var failure = new FailureBuilder("x", because: null).Expected("to be 1").Found(2).Build();

        var exception = new ExpectationFailedException(failure);

        Assert.Same(failure, exception.Failure);
        Assert.Equal("Expected x to be 1, but found 2.", exception.Message);
    }

    [Fact]
    public void ExpectationFailedException_WithNullFailure_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ExpectationFailedException(null!));

        Assert.Equal("failure", exception.ParamName);
    }
}
