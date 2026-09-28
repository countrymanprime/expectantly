# Testing and CI

## Test project

[`tests/Expectantly.Tests`](../tests/Expectantly.Tests) is an xUnit project that runs on `net8.0`
and `net10.0`. Package versions live in [`Directory.Packages.props`](../Directory.Packages.props):
`xunit` 2.9.2, `xunit.runner.visualstudio` 2.8.2, `Microsoft.NET.Test.Sdk` 17.11.1, and
`coverlet.collector` 6.0.2 for coverage collection. It references the library via
`ProjectReference`, is marked `IsPackable=false`, and can see the library's internals through
`InternalsVisibleTo`.

[`GlobalUsings.cs`](../tests/Expectantly.Tests/GlobalUsings.cs) pulls in `Xunit` and the helpers in
[`TestSupport.cs`](../tests/Expectantly.Tests/TestSupport.cs):

- `FailureOf(() => …)` runs an assertion that must fail and returns its `Failure`.
- `MessageOf(() => …)` does the same and returns the message.

Tests mirror the source by feature, not by class-per-file:

```text
tests/Expectantly.Tests/
├── Expect/
│   └── ThatTests.cs                       # Expect.That and expression capture
├── Failures/
│   ├── FailureTests.cs                    # sentence composition, ExpectationFailedException
│   ├── ReasonTests.cs                     # because handling
│   └── StackTraceTests.cs                 # library frames hidden from stack traces
├── Formatting/
│   ├── ExpressionsTests.cs                # literal detection, multi-line expressions
│   └── ValueFormatterTests.cs             # how values print in messages
└── ObjectAssertions/
    ├── BooleanAssertionsTests.cs          # IsTrue / IsFalse
    ├── IsTests.cs / IsNotTests.cs         # Is / IsNot, string differences, comparers
    ├── NullAssertionsTests.cs             # IsNull / IsNotNull
    └── ReferenceAndTypeAssertionsTests.cs # IsSameInstanceAs / IsNotSameInstanceAs / IsAssignableTo
```

Conventions:

- One `[Fact]` per behavior, named `Method_Condition_ExpectedResult`, for example
  `IsNull_WhenValueIsNotNull_ThrowsExpectationFailedException`.
- **Every failure path has an exact expected-message test.** Messages are the product, so tests
  compare the whole message with `Assert.Equal`, not a fragment with `Assert.Contains`. Use a
  named variable as the subject (`var answer = 42;`) so the message shows how expressions are named.
- Tests call `global::Expectantly.Expect` explicitly, because inside the test project `Expect`
  also names the `Expectantly.Tests.Expect` namespace.

Run locally (needs the .NET 10 SDK and the .NET 8 runtime):

```bash
dotnet test Expectantly.sln --configuration Release
```

`coverlet.collector` is referenced, but nothing collects coverage by default and no threshold is
enforced. To collect it locally, add `--collect:"XPlat Code Coverage"`.

## Build checks

The build itself enforces several rules, because [`Directory.Build.props`](../Directory.Build.props)
sets `TreatWarningsAsErrors`:

- **XML docs:** every public member of the library needs a doc comment (CS1591).
- **Public API:** any change to the public surface must be recorded in
  [`PublicAPI.Unshipped.txt`](../src/Expectantly/PublicAPI.Unshipped.txt) (RS0016/RS0017). To
  update it after an intended change, run
  `dotnet format analyzers src/Expectantly/Expectantly.csproj --diagnostics RS0016 --severity info`.
- **Trimming and AOT:** the `net8.0` and `net10.0` builds are `IsTrimmable` and `IsAotCompatible`,
  so trim and AOT analyzer warnings fail the build.

## CI pipeline

[`.github/workflows/ci.yml`](../.github/workflows/ci.yml) runs on every push to `main` and every
pull request targeting `main`, and the release workflow calls it before publishing. Both jobs check
out full history, because MinVer computes the package version from git tags (`v*`).

| Job | Runs on | Steps |
| --- | --- | --- |
| `build-test` | `ubuntu-latest` and `windows-latest` | Install the SDK from `global.json` plus the .NET 8 runtime; restore; `dotnet format --verify-no-changes` (Linux only); build Release; test on `net8.0` and `net10.0`. |
| `pack` | `ubuntu-latest` | `dotnet pack`, which runs package validation. On pushes to `main` only, upload the `.nupkg` as the `packages` artifact, kept for 7 days. |

Nothing runs the tests on .NET Framework yet, so the `netstandard2.0` build is compiled but not
exercised.

## Releases

[`.github/workflows/release.yml`](../.github/workflows/release.yml) publishes a version when a
`v*` tag is pushed ([ADR-0008](adr/0008-publish-previews-to-github-packages-from-version-tags.md)):

```bash
git tag v0.1.0-alpha.1
git push origin v0.1.0-alpha.1
```

1. It runs the whole CI workflow (`build-test` and `pack`) first; nothing is published unless it passes.
2. It packs; MinVer turns the tag into the package version (`v0.1.0-alpha.1` → `0.1.0-alpha.1`).
3. It pushes the `.nupkg` to GitHub Packages with the workflow's own `GITHUB_TOKEN`.
4. It creates a GitHub release for the tag, with generated notes and the `.nupkg` attached. A tag
   with a pre-release suffix (anything after `-`) makes a pre-release.

Debug symbols are embedded in the `.nupkg` (`DebugType` `embedded`), because GitHub Packages doesn't
accept `.snupkg` symbol packages.
