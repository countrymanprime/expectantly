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
    public void That_WithVariable_CapturesItsExpression()
    {
        var answer = 42;

        var assertion = global::Expectantly.Expect.That(answer);

        Assert.Equal("answer", assertion.Expression);
    }

    [Fact]
    public void That_WithMemberAccess_NamesTheSubjectByItsExpression()
    {
        var order = new { Total = 42 };

        var message = MessageOf(() => global::Expectantly.Expect.That(order.Total).Is(43));

        Assert.Equal("Expected order.Total to be 43, but found 42.", message);
    }

    [Fact]
    public void That_WithLiteral_NamesTheSubjectValue()
    {
        var message = MessageOf(() => global::Expectantly.Expect.That(42).Is(43));

        Assert.Equal("Expected value to be 43, but found 42.", message);
    }

    [Fact]
    public void That_WithExpressionOverSeveralLines_NamesTheSubjectOnOneLine()
    {
        var order = new { Total = 42 };

        var message = MessageOf(() => global::Expectantly.Expect.That(
            order
                .Total).Is(43));

        Assert.Equal("Expected order.Total to be 43, but found 42.", message);
    }
}
