# PRD 0007: Packaging

## Status

Draft

## Purpose

Define future packaging expectations for libraries, CLI, and optional Docker support.

## Users

- Developers consuming ReplayLab packages.
- Contributors running the project locally.
- Maintainers publishing releases.

## Requirements

- Keep package boundaries aligned with project boundaries.
- Package core independently from adapters.
- Package adapters independently where useful.
- Delay Docker until there is a real runnable workflow.
- Document supported target frameworks before release.

## Candidate Packages

- `ReplayLab.Core`.
- `ReplayLab.Adapters.Mock`.
- Future parser packages if needed.
- Future generic sender adapter packages.
- Future CLI tool package.

## Acceptance Criteria

- Packaging does not force business-specific dependencies.
- Package naming is consistent.
- Release notes identify public API changes.
- Docker packaging, if added, runs a generic workflow only.

## Out Of Scope

- Docker in the initial scaffold.
- Private adapter packaging.
- Publishing before public contract review.

## Assumptions

- Package boundaries should stay close to project boundaries.
- Docker is useful only after CLI or UI workflows exist.

## Open Questions

- Should packages multi-target for broader adoption?
- Should the CLI be distributed as a .NET tool?
- Should adapters be published from this repo or separate repos?
