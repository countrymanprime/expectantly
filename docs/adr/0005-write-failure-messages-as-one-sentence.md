# 0005. Write failure messages as one readable sentence

- Status: Accepted
- Date: 2026-09-28

## Context

Messages mixed angle-bracketed values into short fragments (`Expected <43> but found <42>`, or
`Expected references to match` with no values at all). Libraries split into two styles: Google Truth
and Rust's `assert_eq!` print aligned `expected:` / `but was:` lines, while Shouldly and aweXpect
write one sentence. During PRD review the maintainer chose sentences: restating expected and actual
on separate lines adds length without adding clarity.

## Considered options

1. A header line followed by aligned `key: value` facts.
2. One sentence, `Expected <subject> <expectation> [because <reason>], but <outcome>[, which
   <difference>].`, with extra lines only for detail a sentence can't hold.

## Decision

We choose option 2. `Failure` in `src/Expectantly/Failure.cs` holds the sentence's parts and
composes the message. Values appear once, inside the sentence, formatted by
`Internal/ValueFormatter.cs` (quoted strings, culture-invariant numbers, bounded collections).
Detail lines, indented four spaces, are used only for a pointer to the first difference in a
string. Lines are always joined with `\n`.

## Consequences

- Good, because a failure reads like a person explaining it:
  `Expected name to be "Victoria", but found "Vic toria", which differs at index 3:`.
- Good, because each assertion's message is short enough to assert exactly in tests.
- Bad, because long values make long sentences; formatting has to shorten them around the
  difference, which the formatter does for strings and collections.
- Neutral: structured output (for example equivalency differences in M2) still needs detail lines,
  so the format isn't strictly one line.
