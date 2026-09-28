# Expectantly

Expectantly is a small, fluent assertion library for .NET, in the spirit of
[FluentAssertions](https://fluentassertions.com/): `Expect.That(actual).Is(expected)` instead of
`Assert.Equal(expected, actual)`. When a check fails, the message reads as one plain sentence that
names what was checked:

```text
Expected order.Total to be 43 because tax is included, but found 42.
```

It targets `netstandard2.0`, `net8.0` and `net10.0`, is trimming- and Native AOT-safe on .NET 8 and
later, and doesn't depend on a test framework: a failed check throws `ExpectationFailedException`,
which any test runner reports as a failure. It's pre-release. Today's assertion surface covers
equality, nulls, booleans, instance identity and type checks; see the
[API reference](docs/api-reference.md) for the full list and its known limitations, and the
[PRD](docs/prd/expectantly-v1.md) for where it's heading.

## Install

It isn't on nuget.org yet. Preview versions are published to
[GitHub Packages](https://github.com/countrymanprime/expectantly/packages) when a version is tagged.
GitHub Packages needs you to sign in, even for public packages: create a personal access token with
the `read:packages` scope, then add the feed once:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/countrymanprime/index.json" \
  --name expectantly --username <your-github-username> --password <token> --store-password-in-clear-text
dotnet add package Expectantly --prerelease
```

Or reference the project directly:

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
string text = Expect.That(greeting).IsAssignableTo<string>().Which;
Expect.That(tenantId.Length > 0).IsTrue(because: $"tenant {tenantId} should have an id");
```

- `Expect.That(value)` starts an assertion. The compiler passes the source text of `value` along,
  so failures name it: `Expected answer to be 43, but found 42.`
- Every check takes an optional `because` reason, which the message includes:
  `... to be 43 because tax is included, but found 42.`
- Checks return `AndConstraint<T>`, so they chain with `.And`. Checks that narrow the value, such
  as `IsAssignableTo<T>()` and `IsNotNull()`, also expose it as `.Which`.
- String mismatches point at the first difference:

  ```text
  Expected name to be "Victoria", but found "Vic toria", which differs at index 3:
      "Vic toria"
          ↑
  ```

## Documentation

- [PRD](docs/prd/expectantly-v1.md): requirements and milestones for 1.0.
- [Architecture](docs/architecture.md): how the pieces fit together.
- [API reference](docs/api-reference.md): every public type and member, plus known limitations.
- [Core API design notes](docs/design/core-api.md): naming and API design conventions for new
  assertions.
- [Architecture decision records](docs/adr/README.md): why things are the way they are.
- [Testing and CI](docs/testing-and-ci.md): test conventions and the CI pipeline.

## Building and testing

You need the .NET 10 SDK (pinned in [`global.json`](global.json)) and the .NET 8 runtime, which the
`net8.0` test run uses.

```bash
dotnet restore Expectantly.sln
dotnet build Expectantly.sln --configuration Release
dotnet test Expectantly.sln --configuration Release
dotnet pack src/Expectantly/Expectantly.csproj --configuration Release --output artifacts
```

## License

[MIT](LICENSE)
