# Expectantly

Expectantly is a small, fluent assertion library for .NET, in the spirit of
[FluentAssertions](https://fluentassertions.com/): `Expect.That(actual).Is(expected)` instead of
`Assert.Equal(expected, actual)`.

It targets **.NET 8** (`net8.0`) and doesn't depend on a test framework: a failed check throws
`InvalidOperationException`, which any test runner reports as a failure. The tests here use xUnit.
It's pre-release. Today's assertion surface covers equality, nulls, booleans, reference identity
and type checks; see the [API reference](docs/api-reference.md) for the full list and its known
limitations.

## Install

It isn't published to NuGet yet. For now, reference the project directly:

```bash
dotnet add reference ../path/to/src/Expectantly/Expectantly.csproj
```

## Quick start

```csharp
using Expectantly;

var answer = 42;
object greeting = "hello";
var tenantId = "contoso";

Expect.That(answer).Is(42).And.IsNot(0);
Expect.That(greeting).IsNotNull().And.IsAssignableTo<string>();
Expect.That(tenantId.Length > 0, "tenant {0} should have an id", tenantId).IsTrue();
```

- `Expect.That(value)` asserts against a concrete value.
- `Expect.That(() => value)` asserts against a factory, which is invoked once, immediately.
- Every check returns `AndConstraint<T>`, so checks chain with `.And`.
- The optional `because` and `becauseArgs` arguments are appended to failure messages as
  `... because <reason>`.

## Documentation

- [Architecture](docs/architecture.md): how the pieces fit together.
- [API reference](docs/api-reference.md): every public type and member, plus known limitations.
- [Core API design notes](docs/design/core-api.md): naming and API design conventions for new
  assertions.
- [Architecture decision records](docs/adr/README.md): why things are the way they are.
- [Testing and CI](docs/testing-and-ci.md): test conventions and the CI pipeline.

## Building and testing

```bash
dotnet restore Expectantly.sln
dotnet build Expectantly.sln --configuration Release
dotnet test Expectantly.sln --configuration Release
```

## License

[MIT](LICENSE)
