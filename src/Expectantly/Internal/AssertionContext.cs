using System.Globalization;
using Expectantly.Abstractions;

namespace Expectantly.Internal;

internal sealed class AssertionContext(string? because, params object[] becauseArgs) : IAssertionContext
{
    public string? Because { get; } = because;

    public IReadOnlyList<object?> BecauseArgs { get; } = becauseArgs;

    public string FormatBecauseClause()
    {
        if (string.IsNullOrWhiteSpace(Because))
        {
            return string.Empty;
        }

        var formatted = BecauseArgs.Count > 0
            ? string.Format(CultureInfo.InvariantCulture, Because, BecauseArgs.ToArray())
            : Because;

        return string.IsNullOrWhiteSpace(formatted) ? string.Empty : $" because {formatted}";
    }
}
