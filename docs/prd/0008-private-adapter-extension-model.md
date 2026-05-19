# PRD 0008: Private Adapter Extension Model

## Status

Draft

## Purpose

Define how developers can extend ReplayLab with private sender adapters and
parsers outside the public repository, using the public contracts as a stable
dependency.

## Users

- Developers who need to build a private sender adapter (WCF, messaging bus,
  business-specific HTTP endpoint) without modifying or forking the public repo.
- Teams who want to use the ReplayLab replay engine and CLI/Web entry points
  with their own private adapters.
- Contributors validating that the public contracts are clean and generic enough
  to extend against.

## Requirements

- `ReplayLab.Core` must be available as a NuGet package so private projects can
  reference it without cloning the repo.
- `IReplaySender` and `IMessageParser` must be unambiguous, stable, and
  sufficient for a private implementation to depend on.
- Each public adapter and parser project must expose `IServiceCollection`
  extension methods so private projects can integrate into their own DI
  container.
- The extension model must be documented with an architecture diagram, a flow
  diagram, and a step-by-step guide.
- A compilable example adapter (`ReplayLab.Adapters.Example`) must exist in the
  solution to prove the extension seam works end-to-end.

## Acceptance Criteria

- A private project can add `<PackageReference Include="ReplayLab.Core" />`
  and implement `IReplaySender` without cloning this repo.
- The example adapter compiles, its tests pass, and it depends only on
  `ReplayLab.Core`.
- DI registration extension methods exist in Mock, Http, Csv, and Example
  projects.
- `ReplayLab.Core` has no dependency on
  `Microsoft.Extensions.DependencyInjection`.
- The extension guide is complete and accurate.

## Out Of Scope

- WCF, proprietary contracts, or business-specific mapping code in this repo.
- Hostable CLI or Web entry points for private composition (M7).
- NuGet publishing for CLI, Web, parsers, or adapters (M7).
- AppHost or desktop entry point.
- Authentication, persistence, Docker, or deployment changes.
- CI/CD pipeline for automated publishing.
- Multi-targeting.

## Assumptions

- Breaking changes to `ReplayLab.Core` are acceptable in M6 because no external
  adopters exist yet.
- Private adapter projects own their own composition root in M6. Hostable entry
  points come in M7.
- The example adapter is intentionally simple and fictional — it is a pattern
  reference, not a production adapter.

## Open Questions

- Should `ReplayLab.Parsers.Csv` be published as a NuGet package in M6 or M7?
- Should `ReplayLab.Adapters.Mock` be published for use in private adapter
  tests?
- What is the versioning strategy for packages after M6?
