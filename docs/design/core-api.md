# Core API design notes

> See also: [architecture](../architecture.md), [API reference](../api-reference.md),
> [architecture decision records](../adr/README.md), and the [PRD](../prd/expectantly-v1.md).

## Naming conventions

Expectantly follows a predictable fluent naming pattern:

- `IsX` for state/value predicates (`Is`, `IsNull`, `IsTrue`).
- `HasX` for structural or property ownership (for collections/entities: `HasCount`, `HasId`, `HasItem`).
- `ContainsX`/`Contains` for membership/content checks where a containment verb reads best.
- Names state their semantics exactly, even when longer: `IsSameInstanceAs`, not `IsSameAs`.
- Negative checks are separate methods (`IsNot`, `IsNotNull`), not a generic `.Not` modifier.

## Return types and fluent chaining

Assertion methods that represent a check and then continue the same assertion surface return
`AndConstraint<TAssertion>`. This keeps IntelliSense focused on discoverable follow-up methods while
making chain intent explicit (`.Is(...).And.IsNot(...)`).

Checks that narrow the value (prove it has a type, or isn't null) return
`AndWhichConstraint<TAssertion, TValue>`, which adds `.Which` for the narrowed value
([ADR-0006](../adr/0006-narrowing-assertions-return-andwhichconstraint.md)).

Methods may return assertion types directly only when changing to a different assertion surface
(for example, projecting to a dedicated collection assertion object).

## Overload and parameter consistency

- Parameter order is: the check's own arguments, then `string? because = null`, then any
  `[CallerArgumentExpression]` parameters for the check's value arguments
  ([ADR-0004](../adr/0004-capture-expressions-and-take-because-per-assertion.md)).
- Don't use `params` arrays: a `params` parameter must come last, which blocks caller-expression
  parameters. Take an `IEnumerable<T>`, which works with collection expressions (`[1, 2, 3]`).
- `because` is used as written, never passed to `string.Format`. Callers use interpolated strings.
- Overloads that add a comparer take it right after the value (`Is(expected, comparer, because)`).
- Generic constraints should be used when they improve compile-time guidance, but only where C#
  allows them: a method cannot narrow the constraint of a type parameter already bound by its
  declaring generic type (for example, `ObjectAssertions<TActual>.IsSameInstanceAs` cannot require
  `TActual : class`, since `TActual` is fixed at the class level and shared with value-type
  assertions like `IsTrue`).
- Nullable annotations should always reflect the intended contract for null acceptance and null return.

## Failure messages

- Never throw from a check. Build the failure with the internal `FailureBuilder` and pass it to
  `FailureStrategy.Fail` ([ADR-0003](../adr/0003-report-failures-through-a-failure-pipeline.md)).
- Messages are one sentence: `Expected <subject> <expectation> [because <reason>], but
  <outcome>[, which <difference>].` Each value appears once. Add detail lines only for what a
  sentence can't hold ([ADR-0005](../adr/0005-write-failure-messages-as-one-sentence.md)).
- Format values with `ValueFormatter`, never `ToString()` directly, so they are quoted, bounded and
  culture-invariant.
- Say what differs, not just that something differs: an index, a missing item, a type.
