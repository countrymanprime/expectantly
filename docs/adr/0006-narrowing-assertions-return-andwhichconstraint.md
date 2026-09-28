# 0006. Narrowing assertions return `AndWhichConstraint<TSelf, TValue>`

- Status: Accepted
- Date: 2026-09-28

## Context

[ADR-0002](0002-fluent-assertions-return-andconstraint-for-chaining.md) makes checks return
`AndConstraint<TAssertion>` so `.And` continues on the same subject. Some checks also establish
something new about the value: `IsAssignableTo<T>()` proves it is a `T`, and `IsNotNull()` proves it
has a value. With only `.And`, a test had to cast or re-fetch the value to keep using it. The
[PRD](../prd/expectantly-v1.md) (CORE-6) asks for the narrowed value to be returned.

## Considered options

1. Keep returning `AndConstraint<TAssertion>`; callers cast the value themselves.
2. Return a new assertion object for the narrowed type.
3. Return `AndWhichConstraint<TSelf, TValue>`: `.And` as before, plus `.Which` holding the narrowed value.

## Decision

We choose option 3. `IsNotNull()` and `IsAssignableTo<TExpected>()` on `ObjectAssertions<TActual>`
return `AndWhichConstraint<TSelf, TValue>` (`src/Expectantly/AndWhichConstraint.cs`), which
implements `IAndConstraint<TSelf>`. `.Which` is a plain value, not an assertion object: callers
assert on it with a new `Expect.That(...)`, which names it by its own expression. This extends
ADR-0002 rather than replacing it; checks that don't narrow still return `AndConstraint`.

## Consequences

- Good, because `var text = Expect.That(value).IsAssignableTo<string>().Which;` needs no cast.
- Good, because it matches FluentAssertions' `.Which`, which eases migration.
- Bad, because `IsNotNull().Which` keeps the value's declared nullability, so the compiler still
  warns when dereferencing it. PRD requirement OBJ-3 (M1) addresses that.
- Bad, because once soft-assertion scopes exist, `.Which` after a failed check has no valid value;
  SOFT-3 must make reading it end the scope instead of returning a default.
