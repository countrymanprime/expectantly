using Expectantly.Abstractions;

namespace Expectantly;

/// <summary>
/// Returned by assertions that narrow the value under test, such as
/// <see cref="ObjectAssertions{TActual}.IsAssignableTo{TExpected}(string?)"/>. <see cref="And"/> continues
/// on the same subject; <see cref="Which"/> is the narrowed value.
/// </summary>
/// <typeparam name="TSelf">Assertion type returned for continued chaining.</typeparam>
/// <typeparam name="TValue">The type of the narrowed value.</typeparam>
/// <param name="and">The assertion object to continue chaining on.</param>
/// <param name="which">The narrowed value.</param>
public sealed class AndWhichConstraint<TSelf, TValue>(TSelf and, TValue which) : IAndConstraint<TSelf>
{
    /// <inheritdoc />
    public TSelf And { get; } = and;

    /// <summary>
    /// Gets the narrowed value, for example the value cast to the type an assertion checked for.
    /// </summary>
    public TValue Which { get; } = which;
}
