# 0004. Capture expressions with `[CallerArgumentExpression]` and take `because` per assertion

- Status: Accepted
- Date: 2026-09-28

## Context

Failure messages didn't name the value under test (`Expected <43> but found <42>`). `Expect.That`
took `string? because` and `params object[] becauseArgs`, which had two problems. A `params` parameter
must come last, so it blocked adding an optional `[CallerArgumentExpression]` parameter. And a reason
containing `{` or `}` made `string.Format` throw `FormatException`, which hid the real failure. The
[PRD](../prd/expectantly-v1.md) (CORE-3, CORE-4) calls for compile-time expression capture and a
reason on each check.

## Considered options

1. Keep `because`/`becauseArgs` on `Expect.That`, and name values by walking the stack and reading
   source files at runtime, as FluentAssertions does.
2. Capture the expression at compile time with `[CallerArgumentExpression]` on `Expect.That`, move
   `because` onto each assertion method, and drop `becauseArgs` in favour of interpolated strings.

## Decision

We choose option 2. `Expect.That<T>(T actual, [CallerArgumentExpression] string? expression = null)`
records the source text of `actual`. Each assertion takes `string? because = null`, followed by
`[CallerArgumentExpression]` parameters for its value arguments, which messages use to name expected
values (`to be expectedTotal (43)`). Assertion methods don't use `params` arrays. The reason is
used as written: no formatting, so braces are safe.

## Consequences

- Good, because messages name the value (`Expected order.Total to be 43, but found 42.`) with no
  runtime cost, no source files and no PDBs, which also keeps Native AOT working.
- Good, because each check can carry its own reason, and a reason can no longer throw.
- Bad, because it's a breaking change: `Expect.That(x, "reason")` now compiles but treats `"reason"`
  as the expression. Nothing flags this yet; none of the analyzers planned in the PRD covers it.
- Bad, because future multi-value assertions must take collections (`ContainsAll([1, 2])`) instead
  of `params` arrays.
