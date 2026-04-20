using Expectantly.Abstractions;

namespace Expectantly;

/// <summary>
/// First-pass assertions for object and scalar values.
/// </summary>
/// <typeparam name="TActual">The value type under test.</typeparam>
public class ObjectAssertions<TActual>(TActual actual, IAssertionContext context) : IAssertion<TActual>
{
    public TActual Actual { get; } = actual;

    public IAssertionContext Context { get; } = context;

    public AndConstraint<ObjectAssertions<TActual>> Is(TActual expected)
    {
        if (!Equals(Actual, expected))
        {
            throw new InvalidOperationException($"Expected {Display(expected)} but found {Display(Actual)}{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsNot(TActual unexpected)
    {
        if (Equals(Actual, unexpected))
        {
            throw new InvalidOperationException($"Did not expect {Display(unexpected)}{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsNull()
    {
        if (Actual is not null)
        {
            throw new InvalidOperationException($"Expected <null> but found {Display(Actual)}{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsNotNull()
    {
        if (Actual is null)
        {
            throw new InvalidOperationException($"Expected a non-null value{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsSameAs(TActual instance) where TActual : class
    {
        if (!ReferenceEquals(Actual, instance))
        {
            throw new InvalidOperationException($"Expected references to match{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsAssignableTo<TExpected>()
    {
        if (Actual is not TExpected)
        {
            throw new InvalidOperationException($"Expected {Display(Actual)} to be assignable to {typeof(TExpected).Name}{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsTrue()
    {
        if (Actual is not bool value || !value)
        {
            throw new InvalidOperationException($"Expected <true> but found {Display(Actual)}{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    public AndConstraint<ObjectAssertions<TActual>> IsFalse()
    {
        if (Actual is not bool value || value)
        {
            throw new InvalidOperationException($"Expected <false> but found {Display(Actual)}{Context.FormatBecauseClause()}.");
        }

        return new AndConstraint<ObjectAssertions<TActual>>(this);
    }

    private static string Display(object? value) => value is null ? "<null>" : $"<{value}>";
}
