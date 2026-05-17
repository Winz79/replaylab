# PRD 0004: Sender Adapters

## Status

Draft

## Purpose

Define how ReplayLab sends generic messages to destinations through replaceable adapters.

## Users

- Developers using the mock sender in tests.
- Contributors adding generic sender adapters.
- Private teams building business-specific adapters outside this repo.

## Requirements

- Provide `IReplaySender` as the adapter contract.
- Provide `MockReplaySender` for local smoke testing.
- Keep adapters separate from `ReplayLab.Core`.
- Allow future generic adapters such as HTTP.
- Keep business-specific adapters outside the public repo.

## Acceptance Criteria

- Mock sender returns a successful `ReplayResult`.
- Adapter projects depend on core.
- Core does not depend on adapter projects.
- Adapter tests do not require external systems.

## Out Of Scope

- WCF.
- Real certificates.
- Internal service contracts.
- Customer endpoints.
- Business-specific adapters.

WCF and business-specific adapters must live outside the public repository.

## Assumptions

- Mock sender should remain intentionally simple.
- HTTP is the likely first real generic sender, but it is not part of the scaffold.

## Open Questions

- Should adapters expose capability metadata?
- Should adapter configuration be strongly typed per adapter?
- Should generic HTTP sending be official or example-only?
