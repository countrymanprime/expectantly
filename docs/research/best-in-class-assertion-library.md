# What a best-in-class assertion library looks like, and how Expectantly gets there

_Research date: 2026-09-28. Sources are listed at the end. Claims that were only seen in search
snippets or could not be confirmed first-hand are marked **(unverified)**._

## TL;DR

A best-in-class assertion library is judged almost entirely on **what happens when a test fails**.
Passing assertions are all alike; failing ones should tell you *what* was checked, *what* differed,
and *where*, without you re-running under a debugger. Everything else — discoverability, soft
assertions, extensibility, analyzers — exists to make that failure message cheap to get right.

The .NET market has a clear opening in 2026:

| Library | Model | Gap Expectantly can fill |
|---|---|---|
| FluentAssertions 8.x | Sync, rich | Commercial license since v8 (Jan 2025); names subjects by walking the stack and reading source files (needs PDBs + sources, not AOT-safe); reflection-based framework detection |
| AwesomeAssertions 9.x | Apache-2.0 fork of FA | Inherits FA's architecture, including the stack-walk/source-read caller identification |
| Shouldly 4.x / 5.0-preview | Sync, `x.ShouldBe(y)` | Very basic equivalency (maintainer's own words), no assertion scopes, always throws its own exception type |
| TUnit.Assertions, aweXpect | **Async-only** (`await Assert.That(...)`) | Users find mandatory `await` awkward (TUnit #580); every test must become `async Task` |
| xUnit / NUnit / MSTest built-ins | Static `Assert.*` | Framework-bound; limited extensibility |

**Recommended positioning:** _a synchronous-first, MIT-licensed-forever, trimming/AOT-safe library
with compile-time expression capture, Truth-quality failure messages, and soft assertions built
into the core._ No current library occupies all of that at once.

Expectantly is ~150 lines of library code at `0.x` with a clean naming convention
(`docs/design/core-api.md`). That is the ideal moment: several foundational decisions below are
**breaking** and should be made before anyone depends on the current API.

---

## 1. The ten properties of a best-in-class assertion library

Each property is backed by what the leading libraries (Google Truth, AssertJ, Jest/Vitest,
Playwright, pytest, Swift Testing, Rust, Kotest, FluentAssertions, Shouldly, TUnit, aweXpect)
do or publicly regret.

### 1.1 Failure messages that need no debugger

The single most important property. Converging best practice:

- **Name the subject from source.** pytest rewrites `assert`, Swift's `#expect` macro captures
  sub-expressions, Kotlin ships a power-assert compiler plugin, Shouldly prints
  `contestant.Points should be 1337 but was 0`. In .NET the industry has moved to
  `[CallerArgumentExpression]` (Shouldly 5, MSTest 4, NUnit 4, TUnit, aweXpect). FluentAssertions'
  runtime stack-walk + source-file read is the legacy approach; Strikt documents the same
  technique as "costly performance-wise".
- **Structured "facts", aligned.** Google Truth builds messages from `key: value` facts with fixed
  keys and runtime values, padded to a common width; multi-line values move below their key.
  Rust 1.73 changed `assert_eq!` to the same shape (`left:` / `right:` right-aligned). The
  rationale: mixing prose and values makes it harder to spot which is actual and which is expected.
- **Diffs chosen by shape.** Strings: show the first differing index with a caret and trim common
  prefix/suffix (Truth keeps 20 chars of context, only trims when ≥60 chars would be hidden, never
  splits a surrogate pair). Sequences: first failing index. Dictionaries: differing entries.
  Sets/unordered: `missing (n): …` and `unexpected (n): …`. Object graphs: property path
  (`Customer.Address.City`) per difference.
- **Handle look-alikes.** When two values print identically but are not equal, say so (Truth:
  "non-equal instance of same class with same string representation"; Jest: "Compared values have
  no visual difference") and show runtime types when they differ.
- **Stable, bounded formatting.** Quote strings, show `null` consistently, truncate long values
  and big collections with a visible marker (AssertJ: 1000 elements / 80 chars; aweXpect: 100 chars
  / 10 items; Shouldly 5: 1000 chars), and make it configurable.
- **Context without noise.** A `because` reason, optional scoped "clues" that stack (Kotest
  `withClue`), and **lazy** message construction (Jest `message: () => string`, AssertJ
  `withFailMessage(Supplier)`) so the passing path pays nothing.
- **Hide the library from stack traces** (AssertJ does this by default) so the top frame is the
  test line.

### 1.2 Discoverability through type-specific surfaces

`Expect.That(x).` should offer only the assertions that make sense for `x`'s static type. Truth's
comparison doc names this as the main reason it chose subjects over Hamcrest's free-floating
matchers. Hamcrest's generics-heavy design is its best-known regret.

### 1.3 A small, orthogonal, puzzle-free API

Truth: "When a library has more APIs, it's harder to find what you're looking for… projects develop
their own dialects." Swift Testing replaced ~40 `XCTAssert*` functions with two macros. Truth also
keeps a list of "puzzlers" to avoid: `isNotSameAs` breaking on autoboxing (renamed
`isNotSameInstanceAs`), `containsAll` reading like exact equality (renamed `containsAtLeast`),
comparators that silently don't apply. Jest splits equality into three clearly different
methods (`toBe` / `toEqual` / `toStrictEqual`) instead of overloading one.

### 1.4 Soft assertions in the core, not bolted on

Every modern library has them (Truth `Expect`, AssertJ `SoftAssertions`, Vitest/Playwright
`expect.soft`, NUnit `Assert.EnterMultipleScope`, FA `AssertionScope`, TUnit `Assert.Multiple`,
MSTest 4.3 `Assert.Scope` (experimental), Swift `#expect` vs `#require`). The lessons:

- Truth made the **failure strategy** a fundamental part of the design; AssertJ and Kotest added it
  later and have combinations that silently fall back to fail-fast or only work with their own
  assertions.
- The .NET ecosystem has converged on **`using` scopes**, not lambdas: lambda forms break with
  async code (xUnit `Assert.Multiple` issue #3209; NUnit analyzer NUnit2056 discourages
  `Assert.Multiple` in favour of `EnterMultipleScope`).
- Define what a chain does **after** a soft failure. FA's own design notes warn that inside a scope
  `FailWith` may not halt, so the next `.Which` can throw `NullReferenceException`. Truth's answer
  is `ignoreCheck()` for derived subjects.
- Aggregate output is numbered with a total (AssertJ: `Multiple Failures (3 failures) -- failure 1 --`).

### 1.5 Structural equivalency with visible configuration

The #1 reason teams can't leave FluentAssertions is `BeEquivalentTo` with options (Shouldly's
maintainer points FA refugees to AwesomeAssertions for this). Best-in-class (AssertJ recursive
comparison): ignore members by path/pattern/type, per-type and per-member comparers, strict vs
loose runtime typing, opt in/out of overridden `Equals`, ordering options for collections,
cycle-safe, and **the failure message lists each differing path and echoes the configuration
that was used**. Performance matters: FA had to replace its permutation search for unordered
collections (PR #3188) and has open issues about slow large arrays and hangs on cyclic graphs.

### 1.6 Correct integration with the test framework

The runner should show a **failure**, not an **error**, and IDE diff viewers should light up.
Approaches:

- Scan loaded assemblies and build the framework's exception by reflection (FA/AA, NFluent,
  aweXpect's fallback). Fragile — FA 7 broke on xUnit v3 (#2935), NFluent hit assembly-load
  failures (#274) — and not trim/AOT-safe.
- **Adapter interface with explicit registration** (aweXpect: `ITestFrameworkAdapter` with
  `Fail`, `Skip`, `Inconclusive`), with scanning only as a fallback. This is the most modern pattern.
- xUnit v3 lets third-party libraries opt into "assertion failure" reporting by having their
  exception implement an interface named `IAssertionException` **(unverified — confirm against
  xUnit v3 docs before relying on it)**.

### 1.7 Cheap, first-class extensibility

Users must be able to write a custom assertion in ~10 lines that produces messages
indistinguishable from built-ins. Truth criticises AssertJ for making extension authors format
messages themselves. Good models: Jest's `this.utils.printExpected/printReceived/diff`, TUnit's
`[GenerateAssertion]` source generator (a `bool`-returning method becomes a full assertion),
NFluent's builder with first-class negation text, aweXpect's separate stable `aweXpect.Core` package
for extension authors.

### 1.8 Static analysis as part of the product

Some mistakes can only be caught at compile time:

- **An unfinished assertion** — `Expect.That(x);` with nothing after it passes silently. AssertJ uses
  `@CheckReturnValue`; Chai needed a JS Proxy for the same class of bug.
- **Un-awaited async assertion** — FA issue #1746, open since 2021. TUnit and aweXpect ship an
  error-level "must await" analyzer.
- **Misuse** — `Expect.That(a == b).IsTrue()` (loses both values), swapped actual/expected
  (`Expect.That(42).Is(result)`), configuration placed after the assertion (AssertJ's top pitfall).
- **Migration code fixes** — AwesomeAssertions.Analyzers and aweXpect.Migration rewrite xUnit/FA
  calls. This is the lowest-friction adoption path there is.
- **Nullability** — aweXpect ships diagnostic suppressors so `IsNotNull()` quiets CS8602 afterwards.

### 1.9 Async and retrying assertions as a distinct, explicit kind

Exceptions from async code (`ThrowsAsync`), and polling (`Eventually` / Playwright web-first
assertions / Vitest `expect.poll` / TUnit `WaitsFor`) must be awaited and should not mix with
one-shot semantics (Vitest's `expect.poll` rejects `toThrow` and snapshots). Playwright's docs note
that mixing retrying and non-retrying matchers is a source of flakiness.

### 1.10 Engineering quality consumers can trust

A stable, **permissive** license (AwesomeAssertions exists because of FA's change, and says
explicitly that its license will never change). Trimming and Native AOT support (xUnit v3 4.0, TUnit,
Shouldly 5 and aweXpect 3 all target it). `netstandard2.0` for .NET Framework users. Public API
compatibility tracking. SourceLink and symbols. Fast pass path. Golden-file tests of the failure
messages themselves.

---

## 2. Where Expectantly stands today

What is already good:

- A predictable naming scheme (`IsX` / `HasX` / `Contains`) and explicit chaining via `.And`.
- An `IAssertion<T>` / `IAssertionContext` abstraction layer, which gives a place to hang extensibility.
- Nullable reference types are enabled, the license is MIT, and CI runs on pull requests.

Gaps, in order of severity:

| # | Finding | Where | Why it matters |
|---|---|---|---|
| G1 | Failures throw `InvalidOperationException` | `src/Expectantly/ObjectAssertions.cs:19` and every method | Runners report an **error**, not an assertion failure. You can't catch "assertion failures" separately from genuine bugs. |
| G2 | `because`/`becauseArgs` live on the entry point with `params object[]` | `src/Expectantly/Expect.cs:17` | (a) One reason for the whole chain. (b) `params` must be the last parameter, which **blocks adding `[CallerArgumentExpression]`**. Shouldly 5 hit the same wall and dropped its `params` overloads. |
| G3 | No expression capture | `Expect.cs` | Messages read `Expected <43> but found <42>` with no subject name. |
| G4 | `That(Func<T>)` evaluates the factory immediately | `Expect.cs:31` | It adds nothing over `That(factory())`. It also takes the natural overload shape that exception assertions (`Expect.That(() => …).Throws<T>()`) will need. |
| G5 | One catch-all `ObjectAssertions<T>` | `ObjectAssertions.cs` | `Expect.That("x").IsTrue()` compiles and fails at runtime; strings, collections and numbers will have nowhere to go. |
| G6 | `IsSameAs` on value types always fails | `ObjectAssertions.cs:57` | `Expect.That(5).IsSameAs(5)` boxes twice, so `ReferenceEquals` is false. This is Truth's "autoboxing puzzler" in C# form. The message also omits both values. |
| G7 | Equality uses `object.Equals` | `ObjectAssertions.cs:17` | Boxes value types; no `IEqualityComparer<T>`; no numeric tolerance. |
| G8 | Value formatting is `$"<{value}>"` | `ObjectAssertions.cs:95` | `true` shows as `<True>` next to a hard-coded `<true>` (see `BooleanAssertionsTests`); strings are unquoted (`<>` vs `<"">` is invisible); collections print their type name. |
| G9 | `because` formatting can throw | `Internal/AssertionContext.cs:20` | A reason containing `{` with args throws `FormatException`, which **masks the real failure**. |
| G10 | `IsAssignableTo<TExpected>()` doesn't narrow | `ObjectAssertions.cs:65` | You can't continue asserting on the cast value (FA `.Which`, TUnit returns the cast subject). |
| G11 | No soft assertions, async, exceptions, collections, strings, numerics or equivalency | — | These are table-stakes coverage. |
| G12 | No public way to fail consistently from a custom assertion | `Abstractions/*` | `IAssertion<T>` exposes `Actual` and `Context`, but not a failure API, so every extension would invent its own message format and exception. |
| G13 | Library frames appear at the top of stack traces | all | Add `[StackTraceHidden]` (net6+) / `[DebuggerStepThrough]`. |
| G14 | No packaging or quality infrastructure | `.csproj`, CI | No NuGet metadata, SourceLink, `netstandard2.0` target, public-API tracking, AOT check, format check, or analyzers. |

---

## 3. Foundational decisions to make now, while the API is still 0.x

Each one is written as a short architecture decision record (ADR). They are ordered so that each
decision unblocks the next.

### D1. Sync-first, async where it is actually needed

**Decision:** Assertions on values are synchronous and throw immediately. Only assertions that
must wait return `Task` and are awaited: `ThrowsAsync`, `CompletesWithin`, `Eventually`.

**Why:** TUnit and aweXpect already own the async-only niche, and its main complaint is the
mandatory `await` on every line. FA, AwesomeAssertions and Shouldly show that most .NET users want
sync. The cost of sync is the silent un-awaited-async-assertion bug; an error-level analyzer
(A2 below) closes that.

### D2. Failure pipeline: one exception type, a pluggable strategy, framework adapters

```csharp
public class ExpectationFailedException : Exception { /* Facts, Expression, Because */ }

public interface IFailureStrategy { void Fail(Failure failure); }   // throw | collect
public interface ITestFrameworkAdapter { bool IsAvailable { get; } [DoesNotReturn] void Fail(Failure f); }
```

- Every assertion reports through `Failure` → the current `IFailureStrategy`, which is resolved from
  an `AsyncLocal` so it is parallel-safe. There is no direct `throw`. This makes soft assertions
  (D6) and custom assertions (D7) behave identically to built-ins.
- The default strategy throws `ExpectationFailedException`. Optional adapter packages
  (`Expectantly.Xunit`, `.NUnit`, `.MSTest`, `.TUnit`) register the framework-native exception
  explicitly. Reflection scanning is only a fallback, and is annotated `[RequiresUnreferencedCode]`
  so the core stays AOT-clean.
- If xUnit v3's `IAssertionException` marker convention is confirmed, implement it on the core
  exception so xUnit works with no adapter.

### D3. Entry point: capture the expression, move the reason to the assertion

```csharp
public static ObjectAssertions<T> That<T>(T actual,
    [CallerArgumentExpression(nameof(actual))] string? expression = null);

public AndConstraint<ObjectAssertions<T>> Is(T expected, string? because = null,
    [CallerArgumentExpression(nameof(expected))] string? expectedExpression = null);
```

- Drop `becauseArgs`. C# interpolated strings replace composite formatting, and removing it
  eliminates G9. If laziness becomes important, add an interpolated-string-handler overload later.
- `because` belongs on the assertion. That way each check can carry its own reason, and the entry
  point is free to capture the expression.
- Optional follow-up: `using (Expect.Clue($"order {id}"))` for context that stacks across several
  assertions (Kotest `withClue`).

### D4. Type-specific surfaces through overloads with explicit priority

Overload resolution in C# prefers `That<T>(T)` with `T = List<int>` (an identity conversion) over
`That<T>(IEnumerable<T>)`. So a generic catch-all will swallow every specific overload.

Options, in order of preference:

1. Keep specific overloads (`bool`, `string`, `IEnumerable<T>`, `IDictionary<K,V>`, numerics
   via `INumber<T>` on net8+, `Action`/`Func<Task>` for exceptions, `DateTime`, `Guid`, `Enum`) and
   rank them with `[OverloadResolutionPriority]` (C# 13; can be polyfilled for older target
   frameworks). **Caveat (unverified): consumers on older compilers ignore the attribute. Test the
   binding table with a C# 12 compiler before committing.**
2. Make the catch-all non-generic (`That(object?)`) so specific overloads win naturally, as FA does
   with `Should(this object)`. This loses `T` and boxes value types.
3. Always provide an escape hatch, `Expect.ThatObject(x)`, for types that implement several
   interfaces (AssertJ's overload-ambiguity bugs #1045, #3491, #2357 are the warning).

Write a **binding test suite**: a table of `(expression → expected assertion type)` compiled in CI.
This is the most regression-prone part of the whole library.

### D5. Chaining semantics: `.And` versus `.Which`

- `.And` continues on the same subject (existing behaviour).
- Narrowing assertions return `AndWhichConstraint<TSelf, TNarrowed>`, where `.Which` is a new
  subject with its expression extended. Examples: `IsOfType<T>()`, `IsNotNull()` (non-null
  `.Which`), `Throws<TEx>()` (`.Which` is the exception), `ContainsSingle()` (`.Which` is the
  element).
- In a soft scope, once a chain has failed, derived subjects **record a skipped check instead of
  running**. This is Truth's `ignoreCheck`, and avoids FA's `NullReferenceException` trap.

### D6. Soft assertions as a `using` scope

```csharp
using (Expect.Scope())
{
    Expect.That(order.Total).Is(43);
    Expect.That(order.Lines).HasCount(2);
}   // throws once, listing every failure, numbered
```

Use `AsyncLocal` and support nesting (the inner scope flushes into the outer one). This works the
same for sync and async code, and is built on D2, so third-party assertions participate for free.

### D7. Message model: facts, plus a formatter registry

Target output:

```text
Expected order.Total to be 43 because tax is included.
    expected: 43
     but was: 42

Expected user.Name to be "Victoria".
    expected: "Victoria"
     but was: "Vic toria"
                 ↑ first difference at index 3
```

- A `Failure` is a header plus a list of `Fact(key, value)`. The renderer aligns keys, and
  moves multi-line values below their key.
- An `IValueFormatter` registry covers primitives, quoted and escaped strings, collections with a
  truncation marker, dictionaries, records, `Type`, and `DateTime` with kind. It can be configured
  per scope (`using (Expect.Configure(o => o.MaxItems = 20))`).
- Diff helpers are shipped **publicly** so custom assertions reuse them: `StringDiff` (index, caret,
  prefix/suffix trimming at the Truth thresholds, surrogate-safe), `SequenceDiff`, and
  `SetDiff` (missing/unexpected).
- Look-alike detection: when two values format identically but are not equal, the message says so
  and adds type facts.

### D8. Equality tiers named for what they do

| Method | Semantics |
|---|---|
| `Is(expected)` | `EqualityComparer<T>.Default`, no boxing; overload `Is(expected, IEqualityComparer<T>)` |
| `IsSameInstanceAs(x)` | Reference identity. Constrained or analyzer-flagged for value types (fixes G6). |
| `IsEquivalentTo(x, options?)` | Structural graph comparison (§1.5), path-level diffs, echoes its options |
| `IsCloseTo(x, within)` | Numerics, `DateTime`, `TimeSpan` |

---

## 4. Roadmap

### Phase 0: Foundations (breaking; do first)

1. `ExpectationFailedException`, `Failure`/`Fact`, `IFailureStrategy` (throw only for now) (G1, G12).
2. Entry point with `[CallerArgumentExpression]`; `because` moved onto the assertion methods;
   `becauseArgs` dropped (G2, G3, G9).
3. Remove the eager `That(Func<T>)` (G4). That delegate shape is reserved for exception assertions.
4. `EqualityComparer<T>`; `IsSameAs` → `IsSameInstanceAs` with a value-type guard (G6, G7).
5. Value formatter and fact renderer; update every existing message (G8).
6. `[StackTraceHidden]` on all assertion methods (G13).
7. **Golden-file tests for failure messages.** Every failing assertion's full message is committed
   and diffed (Verify, or a small in-house approval helper). The messages are the product, so
   test them as such.
8. Build hygiene: `netstandard2.0;net8.0;net10.0` targets, `IsAotCompatible`, NuGet metadata,
   SourceLink, `Microsoft.CodeAnalysis.PublicApiAnalyzers`, `EnablePackageValidation`, and a CI
   matrix (ubuntu + windows) with `dotnet format --verify-no-changes` and `dotnet pack`.

### Phase 1: Coverage people reach for daily

- **Booleans.** A dedicated surface, so `IsTrue` only appears on `bool`/`bool?` (G5).
- **Nullability.** `IsNull`/`IsNotNull`, where `IsNotNull` narrows to non-null `.Which`.
- **Strings.**
  - `Is` with a diff.
  - Ordinal-by-default comparisons, with an explicit `IgnoringCase` / `StringComparison` option.
  - `Contains`, `StartsWith`, `EndsWith`, `Matches(regex)`, `IsEmpty`, `IsNullOrWhiteSpace`.
  - `HasLength`, `IsEquivalentTo` with whitespace/newline normalisation options.
- **Numerics.** Generic math where available: `IsGreaterThan`, `IsBetween`, `IsCloseTo`,
  `IsPositive`, `IsNaN`.
- **Collections.**
  - `IsEmpty`, `HasCount`, `Contains`, `DoesNotContain`, `ContainsSingle().Which`.
  - `ContainsExactly(...)`, which is **order-sensitive by default**, and `ContainsExactlyInAnyOrder(...)`. These are explicit names, avoiding Truth's weak-default puzzler.
  - `ContainsAtLeast(...)`, `AllSatisfy(x => …)`, `IsInAscendingOrder(keySelector)`.
  - Messages report missing and unexpected elements.
- **Dictionaries.** `ContainsKey(k).WhoseValue`, `ContainsEntry`.
- **Exceptions.**
  - `Expect.That(() => …).Throws<T>()` and `ThrowsExactly<T>()`, returning `.Which` for the
    exception, with `WithMessage(pattern)` and `WithInnerException<T>()`.
  - `DoesNotThrow()`.
  - `ThrowsAsync<T>()` for `Func<Task>`, which must be awaited.
- **Types.** `IsOfType<T>()` (exact) and `IsAssignableTo<T>()`, both narrowing (G10).
- **Time, `Guid`, and `Enum` basics.**

### Phase 2: What makes teams switch

- Soft assertion scopes (D6) and clues.
- **The equivalency engine**:
  - options for member exclusion and inclusion (by expression, path, pattern or type), per-type
    and per-member comparers, strict ordering or any order, and strict runtime typing;
  - cycle detection and an O(n log n)-friendly unordered matcher;
  - path-level diffs, with the options echoed in the failure message.
- **A public extensibility API and guide.** One page, "write a custom assertion in 10 lines",
  showing `Check(condition, () => Failure…)`, diff helpers and formatter registration. Split
  `Expectantly.Core` out as the stable surface for extension authors.
- **Analyzers v1:**
  - **A1** (error): an `Expect.That(...)` result that is not used.
  - **A2** (error): an async assertion that is not awaited.
  - **A3**: `Expect.That(a == b).IsTrue()` should use `Is`, with a code fix.
  - **A4**: a constant or literal as the subject suggests actual and expected are swapped.
  - **A5**: `IsSameInstanceAs` on a value type.
  - **S1**: suppress nullable warnings after `IsNotNull()`.

### Phase 3: Reach and moat

- **Migration code fixes** from xUnit/NUnit/MSTest `Assert`, from FluentAssertions/AwesomeAssertions,
  and from Shouldly. Paired with the MIT pledge, this is the strongest adoption lever available,
  because FA's license change created a population actively looking to move.
- Adapter packages per framework (D2), plus a Microsoft.Testing.Platform smoke test.
- `Eventually(...)` and `CompletesWithin(...)` polling assertions, awaited, with timeout and
  interval configurable per scope.
- A `[GenerateAssertion]` source generator (TUnit-style) that turns a `bool` predicate into an
  assertion with messages and negation.
- Add-on packages: `Expectantly.Json` (JSON equivalence with path diffs) and `Expectantly.Http`.
- A Verify integration recipe, since snapshots complement assertions rather than replace them.

---

## 5. How to know it is best-in-class: measurable bars

| Bar | Target |
|---|---|
| Message quality | Every built-in failure has a committed golden message; each message names the subject expression, shows expected and actual, and pinpoints the difference (index, path, or missing/unexpected). |
| Passing-path cost | Zero allocations for primitive `Is` on the pass path (BenchmarkDotNet in CI; messages built lazily). |
| AOT and trimming | A Native AOT sample test app built in CI with **zero** trim or AOT warnings from the core package. |
| Framework fidelity | A matrix test proving xUnit v2/v3, NUnit 4/5, MSTest 4 and TUnit report a *failure*, not an error, with the expected exception type. |
| Overload binding | A compiled binding table: each common .NET type binds to the intended assertion surface. |
| Extensibility | A custom assertion takes ≤10 lines and its message is indistinguishable from a built-in one (golden-tested in the docs samples). |
| API stability | PublicAPI.Shipped.txt plus package validation block accidental breaks after 1.0. |
| Mutation score | Stryker.NET on the core, so tests prove that assertions actually *fail* when they should. |
| Migration | Code fixes convert the top 20 FluentAssertions/xUnit call shapes. |

---

## 6. Risks and honest caveats

- **AwesomeAssertions is free, FA-compatible and already has 23M downloads.** Expectantly will not
  win on feature parity alone. It wins on modern foundations (compile-time capture, AOT, adapters),
  message quality, and migration tooling. Equivalency (Phase 2) is the make-or-break feature.
- **Overload resolution (D4) is the riskiest technical decision.** Prototype it with a binding
  table before building Phase 1 surfaces on top of it.
- **Sync-first (D1)** means the un-awaited-async analyzer is not optional. It must ship in the same
  release as `ThrowsAsync`.
- Keep the API small (§1.3). Every new assertion should pass the question "is there already one
  way to say this?"

---

## Sources

.NET landscape:

- FluentAssertions license and v8: https://github.com/fluentassertions/fluentassertions/blob/main/LICENSE · https://x.com/ddoomen/status/1879164019229036908 · https://www.devclass.com/development/2025/01/16/another-open-source-project-shifts-to-restrictive-license-fluent-assertions-following-xceed-partnership/1621343 · upgrade notes: https://raw.githubusercontent.com/fluentassertions/fluentassertions/main/docs/_pages/upgradingtov8.md
- FA caller identification (stack walk and source read): https://raw.githubusercontent.com/fluentassertions/fluentassertions/main/Src/FluentAssertions/CallerIdentifier.cs
- FA framework detection: https://raw.githubusercontent.com/fluentassertions/fluentassertions/main/Src/FluentAssertions/Execution/TestFrameworkFactory.cs
- FA issues: #2935 (xUnit v3 not detected), #1746 (un-awaited ThrowAsync passes), PR #3188 (unordered equivalency performance), #1340 (AndWhich after a scoped failure), #1677 (v8 cleanup list) — https://github.com/fluentassertions/fluentassertions
- AwesomeAssertions: https://github.com/AwesomeAssertions/AwesomeAssertions · https://www.nuget.org/packages/AwesomeAssertions
- Shouldly 5 upgrade notes (CallerArgumentExpression, params removal, AOT): https://raw.githubusercontent.com/shouldly/shouldly/master/documentation/documentation/upgrade/4to5.md · equivalency limits: https://github.com/shouldly/shouldly/discussions/1095 · native exceptions request: https://github.com/shouldly/shouldly/issues/407
- TUnit assertions: https://raw.githubusercontent.com/thomhurst/TUnit/main/docs/docs/assertions/awaiting.md · source-generated assertions: …/extensibility/source-generator-assertions.md · migration pain: https://github.com/thomhurst/TUnit/issues/580
- aweXpect: https://raw.githubusercontent.com/aweXpect/aweXpect/main/Docs/pages/08-write-extension.md · adapters: https://raw.githubusercontent.com/aweXpect/aweXpect/main/Source/aweXpect.Core/Core/Adapters/ITestFrameworkAdapter.cs · v3 changelog: https://raw.githubusercontent.com/aweXpect/aweXpect/main/Docs/pages/11-changelog-v3.md
- NFluent extensibility: https://github.com/tpierrain/NFluent/wiki/Extensibility · https://github.com/tpierrain/NFluent/issues/274
- xUnit v3: https://xunit.net/docs/getting-started/v3/whats-new · https://github.com/xunit/xunit/issues/3209
- NUnit multiple asserts: https://raw.githubusercontent.com/nunit/docs/master/docs/articles/nunit/writing-tests/assertions/multiple-asserts.md · NUnit2056: https://raw.githubusercontent.com/nunit/nunit.analyzers/master/documentation/NUnit2056.md
- MSTest assertions: https://raw.githubusercontent.com/dotnet/docs/main/docs/core/testing/unit-testing-mstest-writing-tests-assertions.md
- Verify: https://github.com/VerifyTests/Verify

Cross-ecosystem:

- Google Truth: https://raw.githubusercontent.com/google/truth/gh-pages/comparison.md · …/failure_messages.md · …/fuzzy.md · …/expect.md · Fact layout: https://raw.githubusercontent.com/google/truth/master/core/src/main/java/com/google/common/truth/Fact.java · diff thresholds: …/ComparisonFailures.java
- AssertJ guide: https://raw.githubusercontent.com/assertj/doc/main/src/docs/asciidoc/user-guide/assertj-core-assertions-guide.adoc · overload ambiguity: https://github.com/assertj/assertj/issues/3491 · https://github.com/assertj/assertj/issues/2357
- Hamcrest Matcher: https://raw.githubusercontent.com/hamcrest/JavaHamcrest/master/hamcrest/src/main/java/org/hamcrest/Matcher.java
- Jest expect: https://raw.githubusercontent.com/jestjs/jest/main/docs/ExpectAPI.md · jest-diff: https://raw.githubusercontent.com/jestjs/jest/main/packages/jest-diff/README.md
- Vitest expect: https://raw.githubusercontent.com/vitest-dev/vitest/main/docs/api/expect.md
- Playwright assertions: https://raw.githubusercontent.com/microsoft/playwright/main/docs/src/test-assertions-js.md
- pytest assertion introspection: https://raw.githubusercontent.com/pytest-dev/pytest/main/doc/en/how-to/assert.rst
- Kotlin power-assert: https://kotlinlang.org/docs/power-assert.html
- Swift Testing: https://developer.apple.com/videos/play/wwdc2024/10179/
- Rust `assert_eq!` format change: https://github.com/rust-lang/rust/pull/111071 · pretty_assertions: https://raw.githubusercontent.com/rust-pretty-assertions/rust-pretty-assertions/main/README.md
- Kotest clues and soft assertions: https://raw.githubusercontent.com/kotest/kotest/master/documentation/docs/assertions/clues.md
- Strikt: https://raw.githubusercontent.com/robfletcher/strikt/main/site/src/orchid/resources/wiki/traversing-subjects.md
- Chai pitfalls: https://github.com/chaijs/chai/issues/726
