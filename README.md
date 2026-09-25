# Expectantly

Expectantly is a small, fluent assertion library for .NET, in the spirit of
[FluentAssertions](https://fluentassertions.com/): `Expect.That(actual).Is(expected)` instead of
`Assert.Equal(expected, actual)`.

It targets **.NET 8**, is framework-agnostic on the test-runner side (used here with xUnit), and is
currently pre-release: the assertion surface covers objects, booleans, nulls, and reference/type
checks. See [`docs/flaws-and-enhancements.md`](docs/flaws-and-enhancements.md) for what's still
missing.

## Install

Not yet published to NuGet. For now, reference the project directly:

```bash
dotnet add reference ../path/to/src/Expectantly/Expectantly.csproj
```

## Quick start

```csharp
using Expectantly;

Expect.That(result).Is(42);
Expect.That(user).IsNotNull().And.IsAssignableTo<IUser>();
Expect.That(flag, "the feature should be enabled for {0}", tenantId).IsTrue();
```

- `Expect.That(value)` — assert against a concrete value.
- `Expect.That(() => value)` — assert against a deferred factory, evaluated once.
- Every terminal check returns `AndConstraint<T>`, so checks chain via `.And`.
- The optional `because`/`becauseArgs` pair is appended to failure messages as `... because <reason>`.

## Documentation

- [`docs/architecture.md`](docs/architecture.md) — how the pieces fit together, top to bottom.
- [`docs/api-reference.md`](docs/api-reference.md) — every public type and member.
- [`docs/testing-and-ci.md`](docs/testing-and-ci.md) — test conventions and the CI pipeline.
- [`docs/design/core-api.md`](docs/design/core-api.md) — naming and API design conventions.
- [`docs/adr/`](docs/adr/README.md) — architecture decision records: why, not just what.
- [`docs/flaws-and-enhancements.md`](docs/flaws-and-enhancements.md) — known gaps and quick wins.

## Building and testing

```bash
dotnet restore Expectantly.sln
dotnet build Expectantly.sln --configuration Release
dotnet test Expectantly.sln --configuration Release
```

## License

[MIT](LICENSE)
