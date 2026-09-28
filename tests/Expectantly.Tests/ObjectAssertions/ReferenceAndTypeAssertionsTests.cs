namespace Expectantly.Tests.ObjectAssertions;

public class ReferenceAndTypeAssertionsTests
{
    [Fact]
    public void IsSameInstanceAs_WhenReferenceMatches_Passes()
    {
        var list = new List<int>();

        var chain = global::Expectantly.Expect.That(list).IsSameInstanceAs(list).And;

        Assert.Same(list, chain.Actual);
    }

    [Fact]
    public void IsSameInstanceAs_WhenReferenceDiffers_ThrowsExpectationFailedException()
    {
        var list = new List<int>();
        var other = new List<int>();

        var message = MessageOf(() => global::Expectantly.Expect.That(list).IsSameInstanceAs(other));

        Assert.Equal("Expected list to be the same instance as other ([]), but found [], which is a different instance.", message);
    }

    [Fact]
    public void IsSameInstanceAs_WhenValueIsNull_SaysSo()
    {
        List<int>? list = null;
        var other = new List<int>();

        var message = MessageOf(() => global::Expectantly.Expect.That(list).IsSameInstanceAs(other));

        Assert.Equal("Expected list to be the same instance as other ([]), but found null.", message);
    }

    [Fact]
    public void IsSameInstanceAs_WhenSubjectIsAValueType_ExplainsBoxing()
    {
        var count = 5;
        var other = 5;

        var message = MessageOf(() => global::Expectantly.Expect.That(count).IsSameInstanceAs(other));

        Assert.Equal(
            "Expected count to be the same instance as other (5), but found an int, which is a value type, "
            + "so it is copied when boxed and is never the same instance; use Is to compare values.",
            message);
    }

    [Fact]
    public void IsNotSameInstanceAs_WhenReferenceDiffers_Passes()
    {
        var list = new List<int>();

        global::Expectantly.Expect.That(list).IsNotSameInstanceAs(new List<int>());
    }

    [Fact]
    public void IsNotSameInstanceAs_WhenReferenceMatches_ThrowsExpectationFailedException()
    {
        var list = new List<int>();

        var message = MessageOf(() => global::Expectantly.Expect.That(list).IsNotSameInstanceAs(list));

        Assert.Equal("Expected list not to be the same instance as list ([]), but it was.", message);
    }

    [Fact]
    public void IsAssignableTo_WhenTypeIsCompatible_ReturnsTheCastValueAsWhich()
    {
        object value = "text";

        string which = global::Expectantly.Expect.That(value).IsAssignableTo<string>().Which;

        Assert.Equal("text", which);
    }

    [Fact]
    public void IsAssignableTo_WhenTypeIsNotCompatible_ThrowsExpectationFailedException()
    {
        object value = "text";

        var message = MessageOf(() => global::Expectantly.Expect.That(value).IsAssignableTo<int>());

        Assert.Equal("Expected value to be assignable to int, but found \"text\", which is a string.", message);
    }

    [Fact]
    public void IsAssignableTo_WhenValueIsNull_ThrowsExpectationFailedException()
    {
        object? missing = null;

        var message = MessageOf(() => global::Expectantly.Expect.That(missing).IsAssignableTo<List<string>>());

        Assert.Equal("Expected missing to be assignable to List<string>, but found null.", message);
    }
}
