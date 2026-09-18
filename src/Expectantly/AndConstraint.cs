using Expectantly.Abstractions;

namespace Expectantly;

/// <summary>
/// Default implementation of <see cref="IAndConstraint{TSelf}"/>.
/// </summary>
/// <typeparam name="TSelf">Assertion type returned for continued chaining.</typeparam>
public sealed class AndConstraint<TSelf>(TSelf and) : IAndConstraint<TSelf>
{
    public TSelf And { get; } = and;
}
