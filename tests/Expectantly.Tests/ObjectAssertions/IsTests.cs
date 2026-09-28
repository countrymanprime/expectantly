namespace Expectantly.Tests.ObjectAssertions;

public class IsTests
{
    [Fact]
    public void Is_WhenValuesAreEqual_Passes()
    {
        var chain = global::Expectantly.Expect.That(42).Is(42).And;

        Assert.Equal(42, chain.Actual);
    }

    [Fact]
    public void Is_WhenValuesDiffer_ThrowsExpectationFailedException()
    {
        var answer = 42;

        var message = MessageOf(() => global::Expectantly.Expect.That(answer).Is(43));

        Assert.Equal("Expected answer to be 43, but found 42.", message);
    }

    [Fact]
    public void Is_WhenExpectedIsAVariable_NamesItBesideItsValue()
    {
        var total = 42;
        var expectedTotal = 43;

        var message = MessageOf(() => global::Expectantly.Expect.That(total).Is(expectedTotal));

        Assert.Equal("Expected total to be expectedTotal (43), but found 42.", message);
    }

    [Fact]
    public void Is_WithReason_IncludesTheReason()
    {
        var total = 42;

        var message = MessageOf(() => global::Expectantly.Expect.That(total).Is(43, because: "tax is included"));

        Assert.Equal("Expected total to be 43 because tax is included, but found 42.", message);
    }

    [Fact]
    public void Is_WhenStringsDifferInTheMiddle_PointsAtTheFirstDifference()
    {
        var name = "Vic toria";

        var message = MessageOf(() => global::Expectantly.Expect.That(name).Is("Victoria"));

        Assert.Equal(
            "Expected name to be \"Victoria\", but found \"Vic toria\", which differs at index 3:\n"
            + "    \"Vic toria\"\n"
            + "        ↑",
            message);
    }

    [Fact]
    public void Is_WhenFoundStringIsShorter_NamesTheMissingEnding()
    {
        var name = "Vic";

        var message = MessageOf(() => global::Expectantly.Expect.That(name).Is("Victoria"));

        Assert.Equal("Expected name to be \"Victoria\", but found \"Vic\", which is missing \"toria\" at the end.", message);
    }

    [Fact]
    public void Is_WhenFoundStringIsLonger_NamesTheExtraEnding()
    {
        var name = "Victoria!";

        var message = MessageOf(() => global::Expectantly.Expect.That(name).Is("Victoria"));

        Assert.Equal("Expected name to be \"Victoria\", but found \"Victoria!\", which has an extra \"!\" at the end.", message);
    }

    [Fact]
    public void Is_WhenStringsDifferOnlyInCase_SaysSo()
    {
        var name = "victoria";

        var message = MessageOf(() => global::Expectantly.Expect.That(name).Is("Victoria"));

        Assert.Equal(
            "Expected name to be \"Victoria\", but found \"victoria\", which differs only in case, first at index 0:\n"
            + "    \"victoria\"\n"
            + "     ↑",
            message);
    }

    [Fact]
    public void Is_WhenStringsDifferByAnEscapedCharacter_AlignsThePointerWithTheEscapedText()
    {
        var text = "a\tb";

        var failure = FailureOf(() => global::Expectantly.Expect.That(text).Is("a b"));

        Assert.Equal(["\"a\\tb\"", "  ↑"], failure.Details);
    }

    [Fact]
    public void Is_WhenLongStringsDiffer_ShowsBothAroundTheDifference()
    {
        var expected = new string('a', 201);
        var found = new string('a', 100) + "X" + new string('a', 100);

        var failure = FailureOf(() => global::Expectantly.Expect.That(found).Is(expected));

        var excerpt = "\"…" + new string('a', 25) + "X" + new string('a', 74) + "…\" (201 characters)";
        Assert.Equal("to be expected (\"…" + new string('a', 100) + "…\" (201 characters))", failure.Expectation);
        Assert.Equal("found " + excerpt, failure.Outcome);
        Assert.Equal("differs at index 100", failure.Which);
        Assert.Equal(["\"…" + new string('a', 10) + "X" + new string('a', 29) + "…\"", new string(' ', 12) + "↑"], failure.Details);
    }

    [Fact]
    public void Is_WhenValuesLookTheSameButHaveDifferentTypes_NamesBothTypes()
    {
        object found = 1L;
        object expected = 1;

        var message = MessageOf(() => global::Expectantly.Expect.That(found).Is(expected));

        Assert.Equal("Expected found to be expected (1), but found 1, which looks the same but is a long, not an int.", message);
    }

    [Fact]
    public void Is_WhenValuesLookTheSameButAreNotEqual_SaysSo()
    {
        var found = new Plain();
        var expected = new Plain();

        var message = MessageOf(() => global::Expectantly.Expect.That(found).Is(expected));

        Assert.Equal("Expected found to be expected (Plain), but found Plain, which looks the same but is not equal.", message);
    }

    [Fact]
    public void Is_WithComparer_UsesTheComparer()
    {
        var name = "ABC";

        global::Expectantly.Expect.That(name).Is("abc", StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Is_WithNullComparer_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => global::Expectantly.Expect.That("abc").Is("abc", comparer: null!));

        Assert.Equal("comparer", exception.ParamName);
    }

    private sealed class Plain;
}
