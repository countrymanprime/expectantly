using System.Diagnostics;

namespace Expectantly.Internal;

/// <summary>
/// Decides what happens to a failed expectation. Every assertion reports through
/// <see cref="FailureStrategy.Current"/> instead of throwing directly, so soft-assertion scopes and
/// test-framework adapters can later change the outcome in one place.
/// </summary>
internal interface IFailureStrategy
{
    void Fail(Failure failure);
}

[StackTraceHidden]
internal static class FailureStrategy
{
    // Only the throwing strategy exists until soft-assertion scopes arrive (PRD SOFT-1).
    public static IFailureStrategy Current => ThrowingFailureStrategy.Instance;

    public static void Fail(Failure failure) => Current.Fail(failure);
}

[StackTraceHidden]
internal sealed class ThrowingFailureStrategy : IFailureStrategy
{
    public static readonly ThrowingFailureStrategy Instance = new();

    public void Fail(Failure failure) => throw new ExpectationFailedException(failure);
}
