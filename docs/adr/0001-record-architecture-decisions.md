# 0001. Record architecture decisions as lightweight ADRs

- Status: accepted
- Date: 2026-09-18

## Context

`docs/design/core-api.md` already captures the *current* shape of the public API
(naming conventions, fluent-chaining return types, overload consistency rules), but
nothing in the repo captures *why* a given rule was chosen or when it changed. As
`Expectantly` moves past pre-release and its assertion surface grows, decisions such
as a return-type convention or a generic-constraint tradeoff need a durable, versioned
place to live next to the code, instead of being reconstructed from commit history or
lost entirely.

## Decision

We record notable, hard-to-reverse technical decisions as lightweight ADRs under
`docs/adr/`, one file per decision (`NNNN-slug.md`), indexed in `docs/adr/README.md`,
using a `Status`/`Date`/`Context`/`Decision`/`Consequences` format (see
`docs/adr/template.md`). `docs/design/*.md` keeps describing the current public API
surface — conventions a contributor follows today — while `docs/adr/` records the
history of *why* those conventions exist. An accepted ADR's Decision and Consequences
are not edited after the fact; a change of mind gets a new ADR that supersedes it.

## Consequences

- Good, because a decision that's expensive to reverse (a return-type convention, a
  generic-constraint limitation, a target framework choice) gets a permanent, linkable
  record instead of living only in a commit message.
- Good, because this reuses a convention already adopted across sibling repos, so it's
  immediately familiar rather than another repo-specific format to learn.
- Bad, because it adds a small amount of process: a real decision now needs both a
  design-doc update (if it changes the current API description) and, when it's
  non-obvious or reversible only at real cost, an ADR.
- Bad, because `docs/design/*.md` and `docs/adr/` can drift into overlapping content if
  contributors aren't deliberate about which one a given change belongs in.
