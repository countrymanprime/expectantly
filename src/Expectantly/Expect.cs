using Expectantly.Internal;

namespace Expectantly;

/// <summary>
/// Entry point for creating fluent assertions.
/// </summary>
public static class Expect
{
    /// <summary>
    /// Creates an assertion object for a concrete value.
    /// </summary>
    /// <typeparam name="T">The runtime type of <paramref name="actual"/>.</typeparam>
    /// <param name="actual">The value under test.</param>
    /// <param name="because">Optional reason that will be included in failure messages.</param>
    /// <param name="becauseArgs">Optional arguments used to format <paramref name="because"/>.</param>
    public static ObjectAssertions<T> That<T>(T actual, string? because = null, params object[] becauseArgs)
        => new(actual, new AssertionContext(because, becauseArgs));

    /// <summary>
    /// Creates an assertion object from a deferred value factory.
    /// </summary>
    /// <typeparam name="T">The type produced by <paramref name="actualFactory"/>.</typeparam>
    /// <param name="actualFactory">Factory that will be invoked once when assertions are created.</param>
    /// <param name="because">Optional reason that will be included in failure messages.</param>
    /// <param name="becauseArgs">Optional arguments used to format <paramref name="because"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="actualFactory"/> is <see langword="null"/>.</exception>
    public static ObjectAssertions<T> That<T>(Func<T> actualFactory, string? because = null, params object[] becauseArgs)
    {
        ArgumentNullException.ThrowIfNull(actualFactory);
        return new ObjectAssertions<T>(actualFactory(), new AssertionContext(because, becauseArgs));
    }
}
