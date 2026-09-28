# 0007. Target `netstandard2.0`, `net8.0` and `net10.0`

- Status: Accepted
- Date: 2026-09-28

## Context

The library targeted only `net8.0`, so .NET Framework test projects couldn't use it, and nothing
declared it trim- or AOT-safe. The [PRD](../prd/expectantly-v1.md) (non-functional requirements)
asks for `netstandard2.0`, `net8.0` and `net10.0`, zero trimming or AOT warnings, and no third-party
runtime dependencies.

## Considered options

1. Keep `net8.0` only.
2. Target `netstandard2.0` only.
3. Multi-target `netstandard2.0;net8.0;net10.0`, marking the `net8.0`+ builds trimmable and AOT-compatible.

## Decision

We choose option 3 in `src/Expectantly/Expectantly.csproj`. Attributes missing from
`netstandard2.0` come from the source-only PolySharp package, plus a hand-written
`StackTraceHiddenAttribute` in `Internal/StackTraceHiddenAttribute.cs`. `global.json` pins the
.NET 10 SDK, and the test project runs on `net8.0` and `net10.0`.

## Consequences

- Good, because .NET Framework 4.6.2+ projects can reference the package through `netstandard2.0`.
- Good, because `IsTrimmable`/`IsAotCompatible` turn trim and AOT warnings into build errors on the
  modern targets.
- Bad, because contributors need the .NET 10 SDK, plus the .NET 8 runtime to run the `net8.0` tests.
- Bad, because the `netstandard2.0` build isn't exercised by tests yet: nothing runs them on .NET
  Framework, and stack-trace hiding has no effect there.
