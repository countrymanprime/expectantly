# 0003. Report failures through a failure pipeline that throws `ExpectationFailedException`

- Status: Accepted
- Date: 2026-09-28

## Context

Every check in `ObjectAssertions<TActual>` threw `InvalidOperationException` directly. Test runners
report that as an error in the code under test, not as a failed expectation, and callers can't tell
an assertion failure apart from a real bug. The [PRD](../prd/expectantly-v1.md) (CORE-1, CORE-2) also
needs soft-assertion scopes (SOFT-1) and test-framework adapters (M3), which both have to change what
happens to a failure without touching every assertion.

## Considered options

1. Keep throwing `InvalidOperationException` from each check.
2. Throw a dedicated exception from each check.
3. Have each check build a `Failure` and hand it to a failure strategy, whose default throws a
   dedicated `ExpectationFailedException`.

## Decision

We choose option 3. A check never throws directly: it builds a `Failure` (the parts of the message
sentence) and calls `FailureStrategy.Fail` in `src/Expectantly/Internal/FailureStrategy.cs`. The only
strategy today throws `ExpectationFailedException`, which exposes the `Failure` for tooling. The
strategy interface stays internal until soft-assertion scopes need to install another one.

## Consequences

- Good, because runners and callers can recognize assertion failures by type.
- Good, because soft-assertion scopes and framework adapters change one place, not every assertion.
- Good, because tools can read the parts of a failure without parsing the message.
- Bad, because it's a breaking change: code catching `InvalidOperationException` must catch
  `ExpectationFailedException` instead.
- Bad, because once scopes exist a check can return after failing, so code after a failed check must
  not assume the check passed. SOFT-3 in the PRD addresses `.Which` for that case.
