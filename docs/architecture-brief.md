# Architecture Brief

## Design Direction

ReplayLab starts with a small .NET solution split by responsibility:

- `ReplayLab.Core`: generic contracts and models.
- `ReplayLab.Adapters.Mock`: a sender adapter used for smoke tests and local development.
- `ReplayLab.Cli`: a placeholder console entry point for future CLI work.
- `tests/*`: focused smoke tests for core contracts and the mock adapter.

## Dependency Direction

`ReplayLab.Core` must not depend on adapters, CLI, UI, Docker, WCF, persistence, or business-specific packages.

Adapters depend on core. Applications such as CLI or future UI compose core and adapters.

## Initial Core Surface

- `ReplayMessage`
- `ReplayResult`
- `ReplayBatch`
- `IMessageParser`
- `IReplaySender`

## Excluded From Initial Architecture

- Parser implementations.
- Replay orchestration.
- Filtering language.
- HTTP sending.
- UI.
- Docker.
- Persistence.
- WCF or business-specific adapters.

WCF and business-specific adapters must live outside the public repository.

## Assumptions

- Separating contracts from adapters will keep the public API generic.
- A mock adapter is enough to prove dependency direction and testing setup.

## Open Questions

- Should replay orchestration live in core or a separate engine package?
- Should message payloads stay as strings, become streams, or support typed content later?
- How should sender capabilities be described when multiple adapter types exist?
