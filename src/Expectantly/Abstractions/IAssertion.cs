namespace Expectantly.Abstractions;

/// <summary>
/// Defines the minimum contract for assertion objects.
/// </summary>
/// <typeparam name="TActual">The asserted value type.</typeparam>
public interface IAssertion<out TActual>
{
    /// <summary>
    /// Gets the value currently under test.
    /// </summary>
    TActual Actual { get; }

    /// <summary>
    /// Gets the source text of the value under test, as captured by <see cref="Expect.That{T}(T, string?)"/>,
    /// or <see langword="null"/> when it is not known. Failure messages use it to name the value.
    /// </summary>
    string? Expression { get; }
}
