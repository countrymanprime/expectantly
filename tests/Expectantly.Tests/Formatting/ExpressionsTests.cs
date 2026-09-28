using Expectantly.Internal;

namespace Expectantly.Tests.Formatting;

public class ExpressionsTests
{
    [Theory]
    [InlineData("42")]
    [InlineData("-1.5e3")]
    [InlineData("0x1F")]
    [InlineData("1_000m")]
    [InlineData(".5")]
    [InlineData("\"text\"")]
    [InlineData("@\"C:\\temp\"")]
    [InlineData("$\"id {id}\"")]
    [InlineData("'c'")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("default")]
    public void IsLiteral_ForLiterals_ReturnsTrue(string expression) => Assert.True(Expressions.IsLiteral(expression));

    [Theory]
    [InlineData("answer")]
    [InlineData("order.Total")]
    [InlineData("1 + x")]
    [InlineData("GetValue()")]
    [InlineData("_count")]
    public void IsLiteral_ForOtherExpressions_ReturnsFalse(string expression) => Assert.False(Expressions.IsLiteral(expression));

    [Theory]
    [InlineData("order\n    .Total", "order.Total")]
    [InlineData("order?\n    .Total", "order?.Total")]
    [InlineData("items\n    ?.Count", "items?.Count")]
    [InlineData("a +\n    b", "a + b")]
    [InlineData("  total  ", "total")]
    public void Normalize_JoinsLinesSensibly(string expression, string expected) =>
        Assert.Equal(expected, Expressions.Normalize(expression));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("42")]
    public void Subject_WithoutAUsefulExpression_IsValue(string? expression) => Assert.Equal("value", Expressions.Subject(expression));

    [Theory]
    [InlineData("43", "43", "43")]
    [InlineData("43", "expectedTotal", "expectedTotal (43)")]
    [InlineData("[1, 2]", "[1, 2]", "[1, 2]")]
    [InlineData("43", null, "43")]
    public void Describe_NamesTheValueOnlyWhenTheExpressionAddsSomething(string formatted, string? expression, string expected) =>
        Assert.Equal(expected, Expressions.Describe(formatted, expression));
}
