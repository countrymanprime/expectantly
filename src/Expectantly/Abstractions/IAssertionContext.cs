namespace Expectantly.Abstractions;

/// <summary>
/// Context data that accompanies assertions and augments failure messages.
/// </summary>
public interface IAssertionContext
{
    /// <summary>
    /// Optional unformatted reason text.
    /// </summary>
    string? Because { get; }

    /// <summary>
    /// Optional arguments used when formatting <see cref="Because"/>.
    /// </summary>
    IReadOnlyList<object?> BecauseArgs { get; }

    /// <summary>
    /// Returns a formatted reason segment suitable for appending to failure messages.
    /// </summary>
    string FormatBecauseClause();
}
