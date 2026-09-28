using System.Collections;
using System.Globalization;
using System.Text;

namespace Expectantly.Internal;

/// <summary>
/// Formats values for failure messages: culture-invariant, quoted where it matters, bounded in size,
/// and never throwing.
/// </summary>
internal static class ValueFormatter
{
    /// <summary>Strings longer than this, once escaped, are shortened.</summary>
    public const int MaxStringLength = 100;

    /// <summary>Collections show at most this many items.</summary>
    public const int MaxItems = 10;

    private const int MaxDepth = 3;

    private const int MaxCountedItems = 1_000;

    public static string Format(object? value) => Format(value, depth: 0);

    private static string Format(object? value, int depth)
    {
        try
        {
            return FormatCore(value, depth);
        }
        catch (Exception)
        {
            // A formatter must never replace the real failure with its own exception.
            return "<" + TypeNames.Of(value!.GetType()) + ": formatter failed>";
        }
    }

    private static string FormatCore(object? value, int depth)
    {
        switch (value)
        {
            case null:
                return "null";
            case string text:
                return Strings.Quote(text, MaxStringLength);
            case char character:
                return Strings.QuoteChar(character);
            case bool flag:
                return flag ? "true" : "false";
            case Enum member:
                return FormatEnum(member);
            case Type type:
                return TypeNames.Of(type);
            case DateTime dateTime:
                return dateTime.ToString("o", CultureInfo.InvariantCulture);
            case DateTimeOffset dateTimeOffset:
                return dateTimeOffset.ToString("o", CultureInfo.InvariantCulture);
            case IDictionary dictionary:
                return FormatDictionary(dictionary, depth);
            case IEnumerable sequence:
                return FormatSequence(sequence, depth);
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        var rendered = value.ToString();
        var runtimeType = value.GetType();

        // Object.ToString() returns the full type name; the C# spelling reads better.
        return rendered is null || rendered == runtimeType.ToString() ? TypeNames.Of(runtimeType) : rendered;
    }

    private static string FormatEnum(Enum member)
    {
        var typeName = TypeNames.Of(member.GetType());
        var name = member.ToString();

        if (name.Length > 0 && (char.IsDigit(name[0]) || name[0] == '-'))
        {
            return "(" + typeName + ")" + name;
        }

        var parts = name.Split([", "], StringSplitOptions.None);
        return string.Join(" | ", parts.Select(part => typeName + "." + part));
    }

    private static string FormatSequence(IEnumerable sequence, int depth)
    {
        if (depth >= MaxDepth)
        {
            return "[…]";
        }

        var builder = new StringBuilder("[");
        var shown = 0;
        var truncated = false;
        int? total = (sequence as ICollection)?.Count;

        var enumerator = sequence.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                if (shown == MaxItems)
                {
                    truncated = true;
                    total ??= MaxItems + CountRemaining(enumerator);
                    break;
                }

                if (shown > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(Format(enumerator.Current, depth + 1));
                shown++;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        AppendTruncation(builder, truncated, total);
        return builder.Append(']').ToString();
    }

    /// <summary>
    /// Counts the items left in a sequence that doesn't know its own count, giving up (and returning
    /// <see langword="null"/>) past <see cref="MaxCountedItems"/> so an endless sequence can't hang.
    /// </summary>
    private static int? CountRemaining(IEnumerator enumerator)
    {
        // The item the caller just moved to hasn't been shown, so it counts.
        var remaining = 1;
        while (enumerator.MoveNext())
        {
            if (++remaining > MaxCountedItems)
            {
                return null;
            }
        }

        return remaining;
    }

    private static string FormatDictionary(IDictionary dictionary, int depth)
    {
        if (depth >= MaxDepth)
        {
            return "{…}";
        }

        var builder = new StringBuilder("{");
        var shown = 0;
        var truncated = false;

        foreach (DictionaryEntry entry in dictionary)
        {
            if (shown == MaxItems)
            {
                truncated = true;
                break;
            }

            if (shown > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Format(entry.Key, depth + 1)).Append(": ").Append(Format(entry.Value, depth + 1));
            shown++;
        }

        AppendTruncation(builder, truncated, dictionary.Count);
        return builder.Append('}').ToString();
    }

    private static void AppendTruncation(StringBuilder builder, bool truncated, int? total)
    {
        if (!truncated)
        {
            return;
        }

        builder.Append(", …");
        if (total is { } count)
        {
            builder.Append(" (")
                .Append((count - MaxItems).ToString("N0", CultureInfo.InvariantCulture))
                .Append(" more)");
        }
    }
}
