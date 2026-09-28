using System.Runtime.CompilerServices;

namespace Expectantly;

/// <summary>
/// Entry point for creating fluent assertions.
/// </summary>
public static class Expect
{
    /// <summary>
    /// Creates an assertion object for a value.
    /// </summary>
    /// <typeparam name="T">The static type of <paramref name="actual"/>.</typeparam>
    /// <param name="actual">The value under test.</param>
    /// <param name="expression">
    /// The source text of <paramref name="actual"/>, used to name it in failure messages. The compiler
    /// supplies it; don't pass it yourself.
    /// </param>
    /// <returns>An assertion object for <paramref name="actual"/>.</returns>
    public static ObjectAssertions<T> That<T>(
        T actual,
        [CallerArgumentExpression(nameof(actual))] string? expression = null)
        => new(actual, expression);
}
