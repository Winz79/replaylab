# BRD Light

## Problem

Developers need a small generic tool for replaying structured messages during testing and local investigation without coupling the public project to proprietary formats, customer data, or internal infrastructure.

## Goals

- Provide a simple core model for replay messages, batches, parsers, senders, and results.
- Keep adapters separate from the core replay contracts.
- Support a mock sender first so behavior can be tested without external systems.
- Leave room for future CSV, JSON, HTTP, CLI, UI, and Docker work.

## Non-Goals

- No CSV parser implementation in the scaffold.
- No UI.
- No Docker setup.
- No WCF.
- No persistence.
- No business-specific mappings or adapters.

## Success Criteria

- The repository builds as a .NET solution.
- Core abstractions compile independently from adapters.
- Mock adapter depends on core and returns a successful result.
- Smoke tests pass.
- Documentation records assumptions and open questions.

## Assumptions

- Replay selection and filtering will be designed after the message model is validated.
- The first public API can evolve before a package release.

## Open Questions

- What is the first real replay workflow to support end to end?
- Should parser errors be represented as exceptions, structured results, or both?
- What naming should be used for packages if published later?
