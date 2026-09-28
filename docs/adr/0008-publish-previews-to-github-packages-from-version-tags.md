# 0008. Publish previews to GitHub Packages from version tags

- Status: Accepted
- Date: 2026-09-28

## Context

M0 made the library packable (package validation, MinVer versions from `v*` tags), but nothing
published it. The [PRD](../prd/expectantly-v1.md) release policy ships a `0.x` preview per
milestone. The maintainer asked for a release workflow that packs and pushes to the GitHub NuGet
feed, kept separate from PR CI. GitHub Packages doesn't accept `.snupkg` symbol packages, and it
requires authentication to install, even for public packages.

## Considered options

1. Publish from a `pack` job inside the PR/CI workflow when a tag is pushed.
2. A separate release workflow, triggered by `v*` tags, that reuses the CI workflow and pushes to
   GitHub Packages.
3. The same, but pushing to nuget.org from the first preview.

## Decision

We choose option 2. [`release.yml`](../../.github/workflows/release.yml) runs on `v*` tags. It calls
[`ci.yml`](../../.github/workflows/ci.yml) as a reusable workflow, then packs and pushes the
`.nupkg` to `https://nuget.pkg.github.com/countrymanprime/index.json` with the built-in
`GITHUB_TOKEN`. It also creates a GitHub release, marked as a pre-release when the tag has a
`-suffix`. Only the publish job has `packages: write` and `contents: write`. Debug symbols are
embedded in the `.nupkg` (`DebugType` `embedded`) instead of shipping a `.snupkg`. PR CI still packs,
to run package validation, but uploads the package only on pushes to `main`.

## Consequences

- Good, because a release is one `git tag` and `git push`, and it can't skip the CI checks.
- Good, because no repository secret is needed; PR runs keep read-only permissions.
- Good, because embedded symbols work the same on any feed.
- Bad, because installing from GitHub Packages needs a personal access token with `read:packages`,
  which is friction for anyone trying a preview.
- Bad, because embedded symbols make the package slightly larger than a separate `.snupkg`.
- Neutral: publishing to nuget.org, planned for 1.0, is one more push step in the same workflow.
  Package signing is still an open question in the PRD.
