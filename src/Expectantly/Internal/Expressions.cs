using System.Text;

namespace Expectantly.Internal;

/// <summary>
/// Turns source text captured by <c>[CallerArgumentExpression]</c> into words for a failure message.
/// </summary>
internal static class Expressions
{
    private const string DefaultSubject = "value";

    /// <summary>
    /// Returns the name of the checked value: its expression, or <c>value</c> when the expression is
    /// missing or is a literal such as <c>42</c>, where repeating it would read oddly.
    /// </summary>
    public static string Subject(string? expression)
    {
        var normalized = Normalize(expression);
        return normalized is null || IsLiteral(normalized) ? DefaultSubject : normalized;
    }

    /// <summary>
    /// Describes an expected value: its formatted value, prefixed by its expression when the
    /// expression names something, for example <c>expectedTotal (43)</c>.
    /// </summary>
    public static string Describe(string formattedValue, string? expression)
    {
        var normalized = Normalize(expression);
        return normalized is null || IsLiteral(normalized) || normalized == formattedValue
            ? formattedValue
            : $"{normalized} ({formattedValue})";
    }

    /// <summary>
    /// Joins multi-line source text into one line. Line breaks become single spaces, except before a
    /// member access, so <c>order⏎    .Total</c> reads <c>order.Total</c>.
    /// </summary>
    public static string? Normalize(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        var joined = new StringBuilder();
        foreach (var rawLine in expression!.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (joined.Length > 0 && line[0] is not '.' and not '?')
            {
                joined.Append(' ');
            }

            joined.Append(line);
        }

        return joined.ToString();
    }

    /// <summary>
    /// Returns <see langword="true"/> for source text that is a literal: a number, string, character,
    /// <c>true</c>, <c>false</c>, <c>null</c> or <c>default</c>.
    /// </summary>
    public static bool IsLiteral(string expression)
    {
        var text = expression.Trim();
        if (text.Length == 0 || text is "null" or "true" or "false" or "default")
        {
            return true;
        }

        if (text[0] is '"' or '\'' || text.StartsWith("@\"", StringComparison.Ordinal)
            || text.StartsWith("$", StringComparison.Ordinal) && text.IndexOf('"') is > 0 and <= 3)
        {
            return true;
        }

        var number = text[0] is '-' or '+' ? text.Substring(1) : text;
        return number.Length > 0
            && (char.IsDigit(number[0]) || (number[0] == '.' && number.Length > 1 && char.IsDigit(number[1])))
            && number.All(c => char.IsLetterOrDigit(c) || c is '.' or '_');
    }
}
