using System.Diagnostics;
using System.Runtime.CompilerServices;
using Expectantly.Abstractions;
using Expectantly.Internal;

namespace Expectantly;

/// <summary>
/// Assertions for object and scalar values.
/// </summary>
/// <remarks>
/// Every failed check reports a <see cref="Failure"/> and throws <see cref="ExpectationFailedException"/>.
/// Each check takes an optional <c>because</c> reason, which the failure message includes.
/// </remarks>
/// <typeparam name="TActual">The type of the value under test.</typeparam>
/// <param name="actual">The value under test.</param>
/// <param name="expression">The source text of <paramref name="actual"/>, used to name it in failure messages.</param>
[StackTraceHidden]
public class ObjectAssertions<TActual>(TActual actual, string? expression = null) : IAssertion<TActual>
{
    /// <inheritdoc />
    public TActual Actual { get; } = actual;

    /// <inheritdoc />
    public string? Expression { get; } = expression;

    /// <summary>
    /// Expects the value to equal <paramref name="expected"/>, using <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <param name="expectedExpression">The source text of <paramref name="expected"/>. The compiler supplies it.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value is not equal to <paramref name="expected"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> Is(
        TActual expected,
        string? because = null,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpression = null)
        => Is(expected, EqualityComparer<TActual>.Default, because, expectedExpression);

    /// <summary>
    /// Expects the value to equal <paramref name="expected"/>, using <paramref name="comparer"/>.
    /// </summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="comparer">The comparer that decides equality.</param>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <param name="expectedExpression">The source text of <paramref name="expected"/>. The compiler supplies it.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ExpectationFailedException">The value is not equal to <paramref name="expected"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> Is(
        TActual expected,
        IEqualityComparer<TActual> comparer,
        string? because = null,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpression = null)
    {
        if (comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (!comparer.Equals(Actual, expected))
        {
            var failure = NewFailure(because);

            if (Actual is string found && expected is string wanted)
            {
                // Long strings are shown around the first difference, not from their start.
                var difference = StringDiff.Describe(wanted, found);
                var wantedText = Strings.Quote(wanted, ValueFormatter.MaxStringLength, difference.Index);
                failure.Expected("to be " + Expressions.Describe(wantedText, expectedExpression))
                    .But("found " + Strings.Quote(found, ValueFormatter.MaxStringLength, difference.Index))
                    .Which(difference.Which)
                    .Details(difference.Details);
            }
            else
            {
                failure.Expected("to be " + Expressions.Describe(ValueFormatter.Format(expected), expectedExpression))
                    .Found(Actual)
                    .Which(LookAlike(expected));
            }

            FailureStrategy.Fail(failure.Build());
        }

        return new(this);
    }

    /// <summary>
    /// Expects the value not to equal <paramref name="unexpected"/>, using <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="unexpected">The value that must not be found.</param>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <param name="unexpectedExpression">The source text of <paramref name="unexpected"/>. The compiler supplies it.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value equals <paramref name="unexpected"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsNot(
        TActual unexpected,
        string? because = null,
        [CallerArgumentExpression(nameof(unexpected))] string? unexpectedExpression = null)
        => IsNot(unexpected, EqualityComparer<TActual>.Default, because, unexpectedExpression);

    /// <summary>
    /// Expects the value not to equal <paramref name="unexpected"/>, using <paramref name="comparer"/>.
    /// </summary>
    /// <param name="unexpected">The value that must not be found.</param>
    /// <param name="comparer">The comparer that decides equality.</param>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <param name="unexpectedExpression">The source text of <paramref name="unexpected"/>. The compiler supplies it.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ExpectationFailedException">The value equals <paramref name="unexpected"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsNot(
        TActual unexpected,
        IEqualityComparer<TActual> comparer,
        string? because = null,
        [CallerArgumentExpression(nameof(unexpected))] string? unexpectedExpression = null)
    {
        if (comparer is null)
        {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (comparer.Equals(Actual, unexpected))
        {
            var failure = NewFailure(because)
                .Expected("not to be " + Expressions.Describe(ValueFormatter.Format(unexpected), unexpectedExpression));

            // A custom comparer can equate values that print differently; show what was found then.
            var found = ValueFormatter.Format(Actual);
            if (found == ValueFormatter.Format(unexpected))
            {
                failure.But("it was");
            }
            else
            {
                failure.But("found " + found).Which("the comparer considers equal");
            }

            FailureStrategy.Fail(failure.Build());
        }

        return new(this);
    }

    /// <summary>
    /// Expects the value to be <see langword="null"/>.
    /// </summary>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value is not <see langword="null"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsNull(string? because = null)
    {
        if (Actual is not null)
        {
            FailureStrategy.Fail(NewFailure(because).Expected("to be null").Found(Actual).Build());
        }

        return new(this);
    }

    /// <summary>
    /// Expects the value not to be <see langword="null"/>.
    /// </summary>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <returns>A constraint whose <see cref="AndWhichConstraint{TSelf, TValue}.Which"/> is the value.</returns>
    /// <exception cref="ExpectationFailedException">The value is <see langword="null"/>.</exception>
    public AndWhichConstraint<ObjectAssertions<TActual>, TActual> IsNotNull(string? because = null)
    {
        if (Actual is null)
        {
            FailureStrategy.Fail(NewFailure(because).Expected("not to be null").But("it was").Build());
        }

        return new(this, Actual);
    }

    /// <summary>
    /// Expects the value to be the same instance as <paramref name="expected"/>.
    /// </summary>
    /// <remarks>
    /// Value types are copied whenever they are boxed, so a value-type subject is never the same
    /// instance as anything; use <see cref="Is(TActual, string?, string?)"/> to compare values.
    /// </remarks>
    /// <param name="expected">The instance the value must be.</param>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <param name="expectedExpression">The source text of <paramref name="expected"/>. The compiler supplies it.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value is not the same instance as <paramref name="expected"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsSameInstanceAs(
        TActual expected,
        string? because = null,
        [CallerArgumentExpression(nameof(expected))] string? expectedExpression = null)
    {
        if (typeof(TActual).IsValueType)
        {
            FailureStrategy.Fail(NewFailure(because)
                .Expected("to be the same instance as " + Expressions.Describe(ValueFormatter.Format(expected), expectedExpression))
                .But("found " + TypeNames.WithArticle(typeof(TActual)))
                .Which("is a value type, so it is copied when boxed and is never the same instance; use Is to compare values")
                .Build());
        }
        else if (!ReferenceEquals(Actual, expected))
        {
            var failure = NewFailure(because)
                .Expected("to be the same instance as " + Expressions.Describe(ValueFormatter.Format(expected), expectedExpression))
                .Found(Actual);

            if (Actual is not null && expected is not null)
            {
                failure.Which("is a different instance");
            }

            FailureStrategy.Fail(failure.Build());
        }

        return new(this);
    }

    /// <summary>
    /// Expects the value not to be the same instance as <paramref name="unexpected"/>.
    /// </summary>
    /// <param name="unexpected">The instance the value must not be.</param>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <param name="unexpectedExpression">The source text of <paramref name="unexpected"/>. The compiler supplies it.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value is the same instance as <paramref name="unexpected"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsNotSameInstanceAs(
        TActual unexpected,
        string? because = null,
        [CallerArgumentExpression(nameof(unexpected))] string? unexpectedExpression = null)
    {
        if (!typeof(TActual).IsValueType && ReferenceEquals(Actual, unexpected))
        {
            FailureStrategy.Fail(NewFailure(because)
                .Expected("not to be the same instance as " + Expressions.Describe(ValueFormatter.Format(unexpected), unexpectedExpression))
                .But("it was")
                .Build());
        }

        return new(this);
    }

    /// <summary>
    /// Expects the value to be assignable to <typeparamref name="TExpected"/>.
    /// </summary>
    /// <typeparam name="TExpected">The type the value must be assignable to.</typeparam>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <returns>
    /// A constraint whose <see cref="AndWhichConstraint{TSelf, TValue}.Which"/> is the value as a
    /// <typeparamref name="TExpected"/>.
    /// </returns>
    /// <exception cref="ExpectationFailedException">The value is <see langword="null"/> or not a <typeparamref name="TExpected"/>.</exception>
    public AndWhichConstraint<ObjectAssertions<TActual>, TExpected> IsAssignableTo<TExpected>(string? because = null)
    {
        if (Actual is TExpected narrowed)
        {
            return new(this, narrowed);
        }

        var failure = NewFailure(because)
            .Expected("to be assignable to " + TypeNames.Of(typeof(TExpected)))
            .Found(Actual);

        if (Actual is not null)
        {
            failure.Which("is " + TypeNames.WithArticle(Actual.GetType()));
        }

        FailureStrategy.Fail(failure.Build());
        return new(this, default!);
    }

    /// <summary>
    /// Expects the value to be the <see cref="bool"/> value <see langword="true"/>.
    /// </summary>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value is not <see langword="true"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsTrue(string? because = null) => IsBoolean(true, because);

    /// <summary>
    /// Expects the value to be the <see cref="bool"/> value <see langword="false"/>.
    /// </summary>
    /// <param name="because">An optional reason, included in the failure message.</param>
    /// <returns>A constraint for chaining further checks on the same value.</returns>
    /// <exception cref="ExpectationFailedException">The value is not <see langword="false"/>.</exception>
    public AndConstraint<ObjectAssertions<TActual>> IsFalse(string? because = null) => IsBoolean(false, because);

    private AndConstraint<ObjectAssertions<TActual>> IsBoolean(bool expected, string? because)
    {
        if (Actual is not bool value || value != expected)
        {
            var failure = NewFailure(because)
                .Expected(expected ? "to be true" : "to be false")
                .Found(Actual);

            if (Actual is not null and not bool)
            {
                failure.Which("is " + TypeNames.WithArticle(Actual.GetType()) + ", not a bool");
            }

            FailureStrategy.Fail(failure.Build());
        }

        return new(this);
    }

    private FailureBuilder NewFailure(string? because) => new(Expression, because);

    /// <summary>
    /// Explains a failure where both values print the same, so the message doesn't look contradictory.
    /// </summary>
    private string? LookAlike(TActual expected)
    {
        if (ValueFormatter.Format(Actual) != ValueFormatter.Format(expected))
        {
            return null;
        }

        var foundType = Actual?.GetType();
        var expectedType = expected?.GetType();

        return foundType is not null && expectedType is not null && foundType != expectedType
            ? "looks the same but is " + TypeNames.WithArticle(foundType) + ", not " + TypeNames.WithArticle(expectedType)
            : "looks the same but is not equal";
    }
}
