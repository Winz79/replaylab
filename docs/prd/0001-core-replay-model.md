# PRD 0001: Core Replay Model

## Status

Draft

## Purpose

Define the generic public model used by parsers, replay orchestration, senders, CLI, and future UI.

## Users

- Developers consuming ReplayLab as a library.
- Adapter authors.
- Parser authors.
- CLI and UI maintainers.

## Requirements

- Provide a generic `ReplayMessage`.
- Provide a `ReplayBatch` for grouped messages.
- Provide a `ReplayResult` for sender outcomes.
- Provide `IMessageParser` as the parser contract.
- Provide `IReplaySender` as the sender contract.
- Keep the core model independent from adapters and applications.

## Acceptance Criteria

- Core types can be instantiated in tests.
- Core does not reference adapter projects.
- Core does not reference CLI, UI, Docker, persistence, WCF, or business-specific packages.
- Core names remain generic.

## Out Of Scope

- Parser implementations.
- Replay orchestration.
- Filtering behavior.
- Persistence.
- Business-specific message interpretation.

## Assumptions

- Message payloads can start as strings until real parser requirements prove otherwise.
- Public contracts can evolve before the first release.

## Open Questions

- Should payloads remain strings, become streams, or support typed content later?
- Should message metadata have reserved keys?
- Should `ReplayResult` include timing, status codes, or adapter-specific details?
