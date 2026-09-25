# Testing and CI

## Test project

[`tests/Expectantly.Tests`](../tests/Expectantly.Tests) is an xUnit project (`xunit` 2.9.2,
`xunit.runner.visualstudio` 2.8.2, `Microsoft.NET.Test.Sdk` 17.11.1, `coverlet.collector` 6.0.2 for
coverage collection) referencing the library via `ProjectReference` and marked `IsPackable=false`.
[`GlobalUsings.cs`](../tests/Expectantly.Tests/GlobalUsings.cs) pulls in `Xunit` globally so test
files don't repeat the `using`.

Tests mirror the source by feature, not by class-per-file:

```
tests/Expectantly.Tests/
├── Expect/
│   └── ThatTests.cs                       # Expect.That(...) overloads
└── ObjectAssertions/
    ├── BecauseFormattingTests.cs          # because/becauseArgs message formatting
    ├── BooleanAssertionsTests.cs          # IsTrue / IsFalse
    ├── IsTests.cs / IsNotTests.cs         # Is / IsNot
    ├── NullAssertionsTests.cs             # IsNull / IsNotNull
    └── ReferenceAndTypeAssertionsTests.cs # IsSameAs / IsAssignableTo
```

Convention: one `[Fact]` per behavior, named `Method_ExpectedBehavior_WhenCondition` (e.g.
`That_WithNullFactory_ThrowsArgumentNullException`), calling through `global::Expectantly.Expect`
explicitly to avoid ambiguity with the `Expectantly.Tests.Expect` namespace.

Run locally:

```bash
dotnet test Expectantly.sln --configuration Release
```

There is no coverage threshold enforced yet, though `coverlet.collector` is already referenced —
see [`flaws-and-enhancements.md`](flaws-and-enhancements.md).

## CI pipeline

[`.github/workflows/ci.yml`](../.github/workflows/ci.yml) runs on every push to `main` and every
pull request targeting `main`:

1. Checkout (`actions/checkout@v4`).
2. Install .NET 8 SDK (`actions/setup-dotnet@v4`, `dotnet-version: 8.0.x`).
3. `dotnet restore Expectantly.sln`
4. `dotnet build Expectantly.sln --no-restore --configuration Release`
5. `dotnet test Expectantly.sln --no-build --configuration Release`

Single job, single OS (`ubuntu-latest`), no coverage report, no artifact publishing, no NuGet pack
step. See the flaws doc for what a more complete pipeline would add.
