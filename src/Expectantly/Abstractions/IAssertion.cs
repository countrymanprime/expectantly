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
    /// Gets metadata used for message formatting.
    /// </summary>
    IAssertionContext Context { get; }
}
