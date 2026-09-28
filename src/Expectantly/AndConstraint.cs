using Expectantly.Abstractions;

namespace Expectantly;

/// <summary>
/// Default implementation of <see cref="IAndConstraint{TSelf}"/>.
/// </summary>
/// <typeparam name="TSelf">Assertion type returned for continued chaining.</typeparam>
/// <param name="and">The assertion object to continue chaining on.</param>
public sealed class AndConstraint<TSelf>(TSelf and) : IAndConstraint<TSelf>
{
    /// <inheritdoc />
    public TSelf And { get; } = and;
}
