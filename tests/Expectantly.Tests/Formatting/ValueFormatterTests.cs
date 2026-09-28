using System.Collections;
using System.Globalization;
using Expectantly.Internal;

namespace Expectantly.Tests.Formatting;

public class ValueFormatterTests
{
    [Flags]
    public enum Access
    {
        Read = 1,
        Write = 2,
    }

    [Fact]
    public void Format_Null_PrintsNull() => Assert.Equal("null", ValueFormatter.Format(null));

    [Fact]
    public void Format_String_QuotesAndEscapes() =>
        Assert.Equal("\"say \\\"hi\\\"\\n\\t\\u0001\\\\\"", ValueFormatter.Format("say \"hi\"\n\t\u0001\\"));

    [Fact]
    public void Format_LongString_ShowsTheStartAndTheLength() =>
        Assert.Equal("\"" + new string('a', 100) + "…\" (1,500 characters)", ValueFormatter.Format(new string('a', 1500)));

    [Fact]
    public void Format_Char_UsesSingleQuotes()
    {
        Assert.Equal("'x'", ValueFormatter.Format('x'));
        Assert.Equal("'\\''", ValueFormatter.Format('\''));
    }

    [Fact]
    public void Format_Bool_IsLowercase() => Assert.Equal("true", ValueFormatter.Format(true));

    [Fact]
    public void Format_Enum_IncludesTheTypeName()
    {
        Assert.Equal("DayOfWeek.Monday", ValueFormatter.Format(DayOfWeek.Monday));
        Assert.Equal("Access.Read | Access.Write", ValueFormatter.Format(Access.Read | Access.Write));
        Assert.Equal("(DayOfWeek)42", ValueFormatter.Format((DayOfWeek)42));
    }

    [Theory]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(int?), "int?")]
    [InlineData(typeof(string[]), "string[]")]
    [InlineData(typeof(int[,]), "int[,]")]
    [InlineData(typeof(List<int>), "List<int>")]
    [InlineData(typeof(Dictionary<string, int[]>), "Dictionary<string, int[]>")]
    public void Format_Type_UsesCSharpNames(Type type, string expected) => Assert.Equal(expected, ValueFormatter.Format(type));

    [Fact]
    public void Format_Numbers_IgnoreTheCurrentCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.Equal("1.5", ValueFormatter.Format(1.5));
            Assert.Equal("1234.5", ValueFormatter.Format(1234.5m));
            Assert.Equal("\"" + new string('a', 100) + "…\" (1,500 characters)", ValueFormatter.Format(new string('a', 1500)));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Format_DateTime_UsesRoundTripFormat() =>
        Assert.Equal("2026-09-28T10:00:00.0000000Z", ValueFormatter.Format(new DateTime(2026, 9, 28, 10, 0, 0, DateTimeKind.Utc)));

    [Fact]
    public void Format_Collection_ListsItsItems() => Assert.Equal("[1, 2, 3]", ValueFormatter.Format(new[] { 1, 2, 3 }));

    [Fact]
    public void Format_LongCollection_ShowsTenItemsAndHowManyMore() =>
        Assert.Equal("[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, … (5 more)]", ValueFormatter.Format(Enumerable.Range(1, 15).ToList()));

    [Fact]
    public void Format_LongSequenceWithoutACount_CountsTheRest() =>
        Assert.Equal("[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, … (5 more)]", ValueFormatter.Format(Yield(15)));

    [Fact]
    public void Format_EndlessSequence_StopsCounting() =>
        Assert.Equal("[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, …]", ValueFormatter.Format(Yield(int.MaxValue)));

    [Fact]
    public void Format_Dictionary_ListsKeysAndValues() =>
        Assert.Equal("{\"a\": 1}", ValueFormatter.Format(new Dictionary<string, int> { ["a"] = 1 }));

    [Fact]
    public void Format_DeeplyNestedCollection_StopsAtThreeLevels() =>
        Assert.Equal("[[[[…]]]]", ValueFormatter.Format(new[] { new[] { new[] { new[] { 1 } } } }));

    [Fact]
    public void Format_ObjectWithoutToString_UsesItsTypeName() => Assert.Equal("Plain", ValueFormatter.Format(new Plain()));

    [Fact]
    public void Format_WhenToStringThrows_DoesNotThrow() =>
        Assert.Equal("<Boom: formatter failed>", ValueFormatter.Format(new Boom()));

    [Fact]
    public void Format_WhenEnumerationThrows_DoesNotThrow() =>
        Assert.Equal("<BrokenSequence: formatter failed>", ValueFormatter.Format(new BrokenSequence()));

    private static IEnumerable<int> Yield(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            yield return i;
        }
    }

    private sealed class Plain;

    private sealed class Boom
    {
        public override string ToString() => throw new InvalidOperationException("boom");
    }

    private sealed class BrokenSequence : IEnumerable
    {
        public IEnumerator GetEnumerator() => throw new InvalidOperationException("broken");
    }
}
