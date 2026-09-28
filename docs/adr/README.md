# Architecture Decision Records

Notable architecture/technical decisions for `Expectantly`, recorded as lightweight ADRs.

- One decision per file, `NNNN-short-title.md`, numbered in order and started from
  [`template.md`](template.md).
- The log is append-only. To change an accepted decision, add a record that supersedes it and
  change only the old record's status line.

| # | Title | Status |
|---|---|---|
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions as lightweight ADRs | Accepted |
| [0002](0002-fluent-assertions-return-andconstraint-for-chaining.md) | Fluent assertion methods return `AndConstraint<TAssertion>` for chaining | Accepted |
