# Architecture

How Expectantly is put together, from the solution layout down to the types. For member-level
detail, see the [API reference](api-reference.md).

## Solution layout

```text
Expectantly.sln
├── src/Expectantly/             # the library (net8.0, nullable enabled)
│   ├── Expect.cs                # static entry point
│   ├── ObjectAssertions.cs      # the only assertion surface today
│   ├── AndConstraint.cs         # fluent chaining wrapper
│   ├── Abstractions/            # public contracts
│   │   ├── IAssertion.cs
│   │   ├── IAssertionContext.cs
│   │   └── IAndConstraint.cs
│   └── Internal/
│       └── AssertionContext.cs  # internal because/becauseArgs formatter
├── tests/Expectantly.Tests/     # xUnit test project, organized by feature
└── docs/                        # this documentation set
```

Both projects target `net8.0` with `<Nullable>enable</Nullable>` and
`<ImplicitUsings>enable</ImplicitUsings>` (see
[`Expectantly.csproj`](../src/Expectantly/Expectantly.csproj) and
[`Expectantly.Tests.csproj`](../tests/Expectantly.Tests/Expectantly.Tests.csproj)). The test project
is `IsPackable=false` and references the library through a `ProjectReference`, not a package.

## Assertion flow

1. **Entry point.** [`Expect.That`](../src/Expectantly/Expect.cs) starts every assertion. One
   overload wraps a value; the other takes a `Func<T>` and invokes it once, immediately. Both
   build an internal `AssertionContext` from the optional `because` and `becauseArgs` arguments and
   return an `ObjectAssertions<T>`.

2. **Assertion object.** [`ObjectAssertions<TActual>`](../src/Expectantly/ObjectAssertions.cs)
   implements `IAssertion<TActual>`. Each check method:
   - evaluates a condition against `Actual`,
   - throws `InvalidOperationException` with a formatted message if the condition fails,
   - otherwise returns `new AndConstraint<ObjectAssertions<TActual>>(this)`.

3. **Chaining.** [`AndConstraint<TSelf>`](../src/Expectantly/AndConstraint.cs) is a thin wrapper
   whose `And` property returns the original assertion object, so
   `Expect.That(x).Is(1).And.IsNotNull()` works. Why checks return a wrapper rather than the
   assertion object itself is recorded in
   [ADR-0002](adr/0002-fluent-assertions-return-andconstraint-for-chaining.md).

4. **Failure messages.** [`AssertionContext`](../src/Expectantly/Internal/AssertionContext.cs)
   holds the `because` and `becauseArgs` pair. Its `FormatBecauseClause()` returns
   `" because <formatted reason>"`, or `""` when there's no reason. If `becauseArgs` is non-empty,
   the reason goes through `string.Format(CultureInfo.InvariantCulture, because, becauseArgs)`,
   which has a [known limitation](api-reference.md#known-limitations).

## Abstractions

The three interfaces under `Abstractions/` let future assertion types (collections, strings,
numbers, and so on) share a contract without depending on `ObjectAssertions<T>`:

- `IAssertion<out TActual>`: anything with an `Actual` value and an `IAssertionContext`.
- `IAssertionContext`: the `because`, `becauseArgs` and `FormatBecauseClause` contract.
- `IAndConstraint<out TSelf>`: the `.And` chaining contract.

`AssertionContext` is `internal sealed`, so code outside the library can't reuse it. The
constructors of `ObjectAssertions<TActual>` and `AndConstraint<TSelf>` are public, though, so a
caller can supply its own `IAssertionContext` implementation and skip `Expect.That`.

## Design conventions and decisions

- [Core API design notes](design/core-api.md): the naming (`IsX`, `HasX`, `ContainsX`),
  return-type and parameter-ordering conventions that new assertion methods follow.
- [Architecture decision records](adr/README.md): the reasons behind those conventions and other
  hard-to-reverse choices.
