using System.Globalization;
using System.Text;

namespace Expectantly.Internal;

/// <summary>Quotes and escapes strings for failure messages.</summary>
internal static class Strings
{
    /// <summary>Returns the string's characters with quotes, backslashes and control characters escaped.</summary>
    public static string Escape(string value) => Escape(value, 0, value.Length);

    public static string Escape(string value, int start, int end)
    {
        var builder = new StringBuilder(end - start);
        for (var i = start; i < end; i++)
        {
            AppendEscaped(builder, value[i], '"');
        }

        return builder.ToString();
    }

    /// <summary>
    /// Quotes a string. When its escaped form is longer than <paramref name="maxLength"/>, only a part is
    /// shown, centred on <paramref name="focus"/>, with <c>…</c> marking the cut ends and the full length
    /// after the quotes.
    /// </summary>
    public static string Quote(string value, int maxLength, int focus = 0)
    {
        var escaped = Escape(value);
        if (escaped.Length <= maxLength)
        {
            return "\"" + escaped + "\"";
        }

        var (start, end) = Window(value, focus, maxLength);
        return "\""
            + (start > 0 ? "…" : string.Empty)
            + Escape(value, start, end)
            + (end < value.Length ? "…" : string.Empty)
            + "\" ("
            + value.Length.ToString("N0", CultureInfo.InvariantCulture)
            + " characters)";
    }

    public static string QuoteChar(char value)
    {
        var builder = new StringBuilder("'");
        AppendEscaped(builder, value, '\'');
        return builder.Append('\'').ToString();
    }

    /// <summary>
    /// Chooses a range of about <paramref name="length"/> characters around <paramref name="focus"/>,
    /// keeping a quarter of the window before it and never splitting a surrogate pair.
    /// </summary>
    public static (int Start, int End) Window(string value, int focus, int length)
    {
        var start = Math.Max(0, Math.Min(focus - length / 4, value.Length - length));
        var end = Math.Min(value.Length, start + length);

        if (start > 0 && char.IsLowSurrogate(value[start]))
        {
            start--;
        }

        if (end < value.Length && char.IsLowSurrogate(value[end]))
        {
            end++;
        }

        return (start, end);
    }

    private static void AppendEscaped(StringBuilder builder, char c, char quote)
    {
        switch (c)
        {
            case '\\': builder.Append("\\\\"); break;
            case '\n': builder.Append("\\n"); break;
            case '\r': builder.Append("\\r"); break;
            case '\t': builder.Append("\\t"); break;
            case '\0': builder.Append("\\0"); break;
            default:
                if (c == quote)
                {
                    builder.Append('\\').Append(c);
                }
                else if (char.IsControl(c))
                {
                    builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append(c);
                }

                break;
        }
    }
}
