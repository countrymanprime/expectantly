# 0002. Fluent assertion methods return `AndConstraint<TAssertion>` for chaining

- Status: accepted
- Date: 2026-09-18

## Context

`docs/design/core-api.md` states that "assertion methods that represent a check and
then continue the same assertion surface return `AndConstraint<TAssertion>`", to keep
IntelliSense focused on discoverable follow-up methods and make chain intent explicit
(`.Is(...).And.IsNot(...)`), and that a method should return an assertion type
directly only when it intentionally switches to a *different* assertion surface (for
example, projecting to a dedicated collection assertion object). This is already
applied consistently in the code: every terminal check on `ObjectAssertions<TActual>`
(`Is`, `IsNot`, `IsNull`, `IsNotNull`, `IsSameAs`, `IsAssignableTo`, `IsTrue`,
`IsFalse`, in `src/Expectantly/ObjectAssertions.cs`) returns
`AndConstraint<ObjectAssertions<TActual>>` rather than `this`.

## Decision

We return `AndConstraint<TAssertion>` — a thin wrapper exposing the same assertion
object via its `And` property (`AndConstraint<TSelf>` in
`src/Expectantly/AndConstraint.cs`, implementing `IAndConstraint<TSelf>` in
`src/Expectantly/Abstractions/IAndConstraint.cs`) — from every terminal assertion
method that continues checking the *same* assertion surface, instead of returning the
assertion object directly. A method returns a different assertion type directly only
when it deliberately hands off to a different assertion surface (e.g. a projection to
a collection-specific assertion type), not as a shortcut for continued chaining on the
current surface.

## Consequences

- Good, because IntelliSense at the end of a chained call shows only the single `.And`
  hop plus the next check, rather than mixing "continue chaining" with "this is the
  check you just called," making `.Is(x).And.IsNot(y)` read unambiguously as check-then-
  continue.
- Good, because the pattern is uniform across every assertion class today (see
  `ObjectAssertions<TActual>`), so new assertion methods have one convention to follow
  rather than a per-method judgment call.
- Bad, because every terminal method allocates a new `AndConstraint<TAssertion>`
  wrapper, even for a call that nothing chains after.
- Bad, because the rule isn't compiler-enforced: a new assertion method that returns
  `TAssertion` directly instead of wrapping it will compile fine while silently
  breaking the convention, so it has to be caught in review.
