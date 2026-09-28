namespace Expectantly.Tests;

internal static class TestSupport
{
    /// <summary>Runs an assertion that must fail and returns its failure.</summary>
    public static Failure FailureOf(Action assertion) => Assert.Throws<ExpectationFailedException>(assertion).Failure;

    /// <summary>Runs an assertion that must fail and returns its message.</summary>
    public static string MessageOf(Action assertion) => FailureOf(assertion).Message;
}
