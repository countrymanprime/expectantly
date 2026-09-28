namespace Expectantly.Internal;

/// <summary>
/// Describes how a found string differs from the expected one, in words, with a pointer to the
/// first difference when it falls inside the string.
/// </summary>
internal static class StringDiff
{
    /// <summary>Characters shown on each side of the difference in the pointer excerpt.</summary>
    private const int Context = 20;

    /// <summary>Strings up to this many characters are shown whole in the pointer excerpt.</summary>
    private const int WholeStringLimit = 60;

    /// <summary>The most characters of a missing or extra ending that are quoted.</summary>
    private const int MaxEndingLength = 20;

    public static StringDifference Describe(string expected, string found)
    {
        var index = FirstDifference(expected, found);

        if (index == found.Length)
        {
            return new(index, $"is missing {QuoteEnding(expected, index)} at the end", []);
        }

        if (index == expected.Length)
        {
            return new(index, $"has an extra {QuoteEnding(found, index)} at the end", []);
        }

        var which = string.Equals(expected, found, StringComparison.OrdinalIgnoreCase)
            ? $"differs only in case, first at index {index}"
            : $"differs at index {index}";

        return new(index, which, Pointer(found, index));
    }

    public static int FirstDifference(string expected, string found)
    {
        var length = Math.Min(expected.Length, found.Length);
        var index = 0;
        while (index < length && expected[index] == found[index])
        {
            index++;
        }

        return index;
    }

    /// <summary>Returns two lines: an excerpt of the found string, and an arrow under the first difference.</summary>
    private static string[] Pointer(string found, int index)
    {
        var (start, end) = found.Length <= WholeStringLimit
            ? (0, found.Length)
            : Strings.Window(found, index, 2 * Context);

        var lead = start > 0 ? "\"…" : "\"";
        var excerpt = lead + Strings.Escape(found, start, end) + (end < found.Length ? "…\"" : "\"");
        var column = lead.Length + Strings.Escape(found, start, index).Length;

        return [excerpt, new string(' ', column) + "↑"];
    }

    private static string QuoteEnding(string value, int start)
    {
        var ending = value.Substring(start);
        return ending.Length <= MaxEndingLength
            ? "\"" + Strings.Escape(ending) + "\""
            : "\"" + Strings.Escape(ending, 0, Strings.Window(ending, 0, MaxEndingLength).End) + "…\"";
    }
}

/// <summary>The result of <see cref="StringDiff.Describe"/>.</summary>
/// <param name="Index">The index of the first differing character.</param>
/// <param name="Which">A phrase that follows <c>which</c> in the failure sentence.</param>
/// <param name="Details">Pointer lines, or none when the difference is at the end.</param>
internal sealed record StringDifference(int Index, string Which, IReadOnlyList<string> Details);
