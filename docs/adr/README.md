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
| [0003](0003-report-failures-through-a-failure-pipeline.md) | Report failures through a failure pipeline that throws `ExpectationFailedException` | Accepted |
| [0004](0004-capture-expressions-and-take-because-per-assertion.md) | Capture expressions with `[CallerArgumentExpression]` and take `because` per assertion | Accepted |
| [0005](0005-write-failure-messages-as-one-sentence.md) | Write failure messages as one readable sentence | Accepted |
| [0006](0006-narrowing-assertions-return-andwhichconstraint.md) | Narrowing assertions return `AndWhichConstraint<TSelf, TValue>` | Accepted |
| [0007](0007-target-netstandard20-net80-and-net100.md) | Target `netstandard2.0`, `net8.0` and `net10.0` | Accepted |
| [0008](0008-publish-previews-to-github-packages-from-version-tags.md) | Publish previews to GitHub Packages from version tags | Accepted |
