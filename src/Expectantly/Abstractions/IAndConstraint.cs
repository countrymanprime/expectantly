namespace Expectantly.Abstractions;

/// <summary>
/// Fluent chain helper returned by terminal assertion methods.
/// </summary>
/// <typeparam name="TSelf">Assertion type exposed for continued chaining.</typeparam>
public interface IAndConstraint<out TSelf>
{
    /// <summary>
    /// Returns the same assertion object to continue fluent chaining.
    /// </summary>
    TSelf And { get; }
}
