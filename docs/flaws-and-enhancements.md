# Known flaws and quick enhancements

Findings from reading the repository as of 2026-09-18, rated by the severity convention used for
code review in this workflow (CRITICAL/HIGH/MEDIUM/LOW). This is a documentation snapshot, not a
tracked issue list — nothing here has been fixed as part of writing these docs.

## HIGH

### `IsSameAs` silently misbehaves for value types
[`ObjectAssertions<TActual>.IsSameAs`](../src/Expectantly/ObjectAssertions.cs) uses
`ReferenceEquals(Actual, instance)`. `TActual` can't be constrained to `class` here because it's
fixed at the `ObjectAssertions<TActual>` level and shared with value-type-only checks like
`IsTrue()` (this tradeoff is already called out in
[`docs/design/core-api.md`](design/core-api.md)). The consequence: calling `IsSameAs` with a struct
boxes both operands independently, so `ReferenceEquals` is false even for equal values — the
assertion always fails for value types, with a message that gives no hint why.
**Quick enhancement:** throw a clearer `InvalidOperationException`/`NotSupportedException` at
runtime when `TActual` is a value type, instead of a confusing generic failure.

### `because` formatting can throw the wrong exception
[`AssertionContext.FormatBecauseClause`](../src/Expectantly/Internal/AssertionContext.cs) runs
`string.Format(CultureInfo.InvariantCulture, Because, BecauseArgs.ToArray())` whenever
`becauseArgs` is non-empty. If the caller's `because` string contains a stray `{` or `}` (easy to
do accidentally in a natural-language reason), this throws `FormatException` — masking the actual
assertion failure the caller was trying to see.
**Quick enhancement:** wrap the `string.Format` call and fall back to the raw `Because` text (or
append a formatting-error note) instead of letting a new exception replace the original failure.

## MEDIUM

### No NuGet packaging metadata
[`Expectantly.csproj`](../src/Expectantly/Expectantly.csproj) has no `PackageId`, `Version`,
`Authors`, `Description`, `RepositoryUrl`, or `PackageLicenseExpression`, even though a `LICENSE`
file (MIT) already exists at the repo root. As-is, the project can't be `dotnet pack`ed into a
usable NuGet package.
**Quick enhancement:** add the standard package metadata properties and a `dotnet pack` step to CI.

### No analyzers or `.editorconfig`
The coding-style conventions this repo already follows (explicit access modifiers, nullable
annotations, `AndConstraint<T>` return shape) aren't enforced by tooling — there's no
`.editorconfig` and no analyzer package (`Microsoft.CodeAnalysis.NetAnalyzers`,
`StyleCop.Analyzers`, or similar).
**Quick enhancement:** add a root `.editorconfig` and enable `EnforceCodeStyleInBuild` /
`AnalysisLevel=latest` in the csproj so `dotnet build` catches drift.

### CI has no coverage reporting despite collecting it
The test project already references `coverlet.collector`, but [`ci.yml`](../.github/workflows/ci.yml)
never passes `--collect:"XPlat Code Coverage"` or publishes/thresholds the result.
**Quick enhancement:** add the collect flag plus a coverage-report step (e.g.
`danielpalme/ReportGenerator-GitHub-Action`) and a minimum-coverage gate.

### CI runs a single OS, no matrix
Only `ubuntu-latest` is exercised. For a library meant to be consumed cross-platform, this doesn't
prove Windows/macOS behavior (unlikely to differ here, but cheap to verify).
**Quick enhancement:** add a `strategy.matrix.os` with `windows-latest` and `macos-latest`.

### No dependency update automation
No Dependabot or Renovate config exists, so `xunit`, `Microsoft.NET.Test.Sdk`, etc. will drift
silently.
**Quick enhancement:** add a minimal `.github/dependabot.yml` for the `nuget` and `github-actions`
ecosystems.

### No versioning or changelog strategy
Nothing documents a SemVer commitment or release process, and there's no `CHANGELOG.md`.
**Quick enhancement:** adopt [Keep a Changelog](https://keepachangelog.com/) and
[SemVer 2.0.0](https://semver.org/), starting a `CHANGELOG.md` now while the API surface is small.

## LOW

### No `CONTRIBUTING.md`, `SECURITY.md`, or issue/PR templates
Standard OSS repo hygiene files are absent, which is more friction the moment this repo gets its
first external contributor or vulnerability report.
**Quick enhancement:** add short versions of each; low effort, mostly boilerplate.

### Limited assertion surface
Today's public surface is `Is`, `IsNot`, `IsNull`, `IsNotNull`, `IsSameAs`, `IsAssignableTo`,
`IsTrue`, `IsFalse` — no string, numeric, or collection-specific assertions yet (no `HasCount`,
`Contains`, `IsGreaterThan`, etc.), even though `docs/design/core-api.md` already reserves the
`HasX`/`ContainsX` naming for them.
**Quick enhancement:** not a "flaw" so much as the obvious next milestone — the design doc has
already paved the naming convention for it.

### No package lock file
There's no `packages.lock.json` for either project, so restores aren't pinned to exact resolved
versions across machines/CI runs.
**Quick enhancement:** set `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>` and
commit the generated lock files.
