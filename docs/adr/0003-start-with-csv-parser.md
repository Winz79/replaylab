# ADR 0003: Start With CSV Parser

## Status

Accepted

## Context

ReplayLab needs a first concrete parser implementation. JSON and CSV are both valid public formats, but CSV is expected to be the first practical input shape for the initial replay workflow.

The parser should remain generic and must not introduce proprietary business mappings, WCF contracts, customer data, or internal field semantics.

## Decision

Implement CSV parsing first.

The first CSV parser will use a whole-row payload model:

- The first non-empty, non-comment line is the CSV header row.
- Empty lines are ignored.
- Lines whose first non-whitespace character is `#` are ignored.
- Each parsed data row becomes one `ReplayMessage`.
- `ReplayMessage.Id` is generated from the parsed data record number.
- `ReplayMessage.Payload` is the full CSV row serialized as a JSON object using the CSV column names.
- `ReplayMessage.Headers` is empty by default.
- `ReplayMessage.Metadata` contains parser/tooling context, such as source format and row numbers.

For the first slice, parser failures should be reported with clear exceptions rather than a structured diagnostics model. This keeps `IMessageParser.ParseAsync(...)` simple because it currently returns `ReplayBatch`.

## Consequences

- The first parser slice can validate real file loading without adding a broader diagnostics model.
- The parser does not require business-specific columns.
- The parser preserves all CSV columns in the payload for later mapping.
- Dynamic promotion of CSV columns into `ReplayMessage.Headers` is deferred to a later mapping/configuration feature.
- JSON parsing remains a future capability.
- Structured parser diagnostics can be reconsidered when inspection UX or CLI requirements need them.
- WCF contract mapping remains outside the public repository and can consume the generic row payload later through a private adapter or extension package.

## Assumptions

- CSV will be the first concrete implementation used by the project.
- A whole-row payload is more generic than requiring specific columns such as `payload`.
- Exception-based parser errors are acceptable until user-facing inspection requirements become clearer.

## Open Questions

- Should large-file streaming be deferred until after the first parser works end to end?
- What later mapping/configuration model should promote CSV columns into sender headers or business contract fields?
