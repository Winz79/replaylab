# ADR 0003: Start With CSV Parser

## Status

Accepted

## Context

ReplayLab needs a first concrete parser implementation. JSON and CSV are both valid public formats, but CSV is expected to be the first practical input shape for the initial replay workflow.

The parser should remain generic and must not introduce proprietary business mappings, WCF contracts, customer data, or internal field semantics.

## Decision

Implement CSV parsing first.

For the first slice, parser failures should be reported with clear exceptions rather than a structured diagnostics model. This keeps `IMessageParser.ParseAsync(...)` simple because it currently returns `ReplayBatch`.

## Consequences

- The first parser slice can validate real file loading without adding a broader diagnostics model.
- The CSV schema must be documented before or during implementation.
- JSON parsing remains a future capability.
- Structured parser diagnostics can be reconsidered when inspection UX or CLI requirements need them.

## Assumptions

- CSV will be the first concrete implementation used by the project.
- A small, generic CSV schema is enough for the first parser slice.
- Exception-based parser errors are acceptable until user-facing inspection requirements become clearer.

## Open Questions

- What exact CSV columns should the first parser require?
- Should headers and metadata be encoded as JSON strings, prefixed columns, or omitted from the first CSV slice?
- Should large-file streaming be deferred until after the first parser works end to end?
