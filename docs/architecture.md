# Architecture

Top-to-bottom map of the repository, from solution down to individual types.

## Solution layout

```
Expectantly.sln
├── src/Expectantly/            # the library (net8.0, nullable enabled)
│   ├── Expect.cs                # static entry point
│   ├── ObjectAssertions.cs      # the only assertion surface today
│   ├── AndConstraint.cs         # fluent chaining wrapper
│   ├── Abstractions/            # public contracts
│   │   ├── IAssertion.cs
│   │   ├── IAssertionContext.cs
│   │   └── IAndConstraint.cs
│   └── Internal/
│       └── AssertionContext.cs  # internal because/becauseArgs formatter
├── tests/Expectantly.Tests/    # xUnit test project, mirrors src/ by feature
└── docs/                        # this documentation set
```

Both projects target `net8.0` with `<Nullable>enable</Nullable>` and
`<ImplicitUsings>enable</ImplicitUsings>` (see
[`Expectantly.csproj`](../src/Expectantly/Expectantly.csproj) and
[`Expectantly.Tests.csproj`](../tests/Expectantly.Tests/Expectantly.Tests.csproj)). The test project
is `IsPackable=false` and references the library via `ProjectReference`, not a package.

## Request flow

1. **Entry point** — [`Expect.That`](../src/Expectantly/Expect.cs) is the only way to start an
   assertion. It has two overloads:
   - `That<T>(T actual, string? because, params object[] becauseArgs)` — wraps a value directly.
   - `That<T>(Func<T> actualFactory, ...)` — invokes the factory exactly once and wraps the result;
     throws `ArgumentNullException` if the factory is null.

   Both overloads construct an internal `AssertionContext` and hand back an `ObjectAssertions<T>`.

2. **Assertion object** — [`ObjectAssertions<TActual>`](../src/Expectantly/ObjectAssertions.cs)
   implements `IAssertion<TActual>` and exposes `Actual` and `Context`. Its methods (`Is`, `IsNot`,
   `IsNull`, `IsNotNull`, `IsSameAs`, `IsAssignableTo<TExpected>`, `IsTrue`, `IsFalse`) each:
   - evaluate a condition against `Actual`,
   - throw `InvalidOperationException` with a formatted message (via the private `Display` helper)
     if the condition fails,
   - otherwise return `new AndConstraint<ObjectAssertions<TActual>>(this)`.

3. **Chaining** — [`AndConstraint<TSelf>`](../src/Expectantly/AndConstraint.cs) is a thin wrapper
   whose `And` property returns the original assertion object, enabling
   `Expect.That(x).Is(1).And.IsNotNull()`.

4. **Failure messages** — [`AssertionContext`](../src/Expectantly/Internal/AssertionContext.cs)
   (internal, behind `IAssertionContext`) holds the optional `because`/`becauseArgs` pair and
   exposes `FormatBecauseClause()`, which returns `" because <formatted reason>"` or `""`. When
   `becauseArgs` is non-empty, the reason is run through
   `string.Format(CultureInfo.InvariantCulture, because, becauseArgs)` — see
   [`docs/flaws-and-enhancements.md`](flaws-and-enhancements.md) for a caveat about this.

## Abstractions

The three interfaces under `Abstractions/` exist so that future assertion types (collections,
strings, numbers, …) can share a contract without depending on `ObjectAssertions<T>` directly:

- `IAssertion<out TActual>` — anything with an `Actual` value and an `IAssertionContext`.
- `IAssertionContext` — the `because`/`becauseArgs`/`FormatBecauseClause` contract.
- `IAndConstraint<out TSelf>` — the `.And` chaining contract.

`AssertionContext` itself is `internal sealed`; only the interface is public, so future assertion
types can't construct their own context — they must go through `Expect.That`.

## Design conventions

See [`docs/design/core-api.md`](design/core-api.md) for the naming (`IsX`/`HasX`/`ContainsX`),
return-type (`AndConstraint<T>` for same-surface chaining), and parameter-ordering conventions new
assertion methods are expected to follow.
