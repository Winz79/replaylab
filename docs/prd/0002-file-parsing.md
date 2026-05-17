# PRD 0002: File Parsing

## Status

Draft

## Purpose

Load structured message files into generic `ReplayBatch` instances.

## Users

- Developers preparing local replay scenarios.
- Contributors adding generic parser implementations.
- CLI and future UI users loading files.

## Requirements

- Support CSV as the first structured public file format.
- Keep parser output generic.
- Treat each CSV data row as one replay message.
- Serialize the full CSV row into the message payload using CSV column names.
- Generate message IDs from parsed data record numbers for the first slice.
- Ignore empty lines.
- Ignore lines whose first non-whitespace character is `#`.
- Leave `ReplayMessage.Headers` empty by default.
- Use `ReplayMessage.Metadata` only for parser/tooling context.
- Throw clear exceptions for invalid input in the first parser slice.
- Avoid business-specific mappings.
- Use synthetic examples in tests and docs.

## Format Direction

- CSV first.
- JSON later.

## Acceptance Criteria

- A parser can load a tiny sample file into `ReplayBatch`.
- The first non-empty, non-comment line is treated as the CSV header row.
- Each parsed data row becomes one `ReplayMessage`.
- Each payload contains the full row as a JSON object.
- Empty lines and comment lines are ignored.
- Metadata records CSV parser context such as source row number and data record number.
- Headers are empty unless a later mapping/configuration feature is introduced.
- Invalid input throws a predictable, clear exception.
- Parser tests use synthetic data.
- Parser implementation does not depend on sender adapters.

## Out Of Scope

- Proprietary formats.
- WCF contracts.
- Customer data.
- Persistence.
- Full filtering language.
- Dynamic header mapping from CSV columns.
- WCF or business contract mapping.

WCF and business-specific mapping must live outside the public repository, using public ReplayLab extension points or private packages later.

## Assumptions

- File parsing should be implemented before real sender adapters.
- CSV is the first concrete parser implementation.
- One format is enough for the first parser slice.
- A whole-row payload keeps the parser generic and preserves data for later mapping.

## Open Questions

- Should parsers support streaming for large files in the first version?
- What mapping/configuration model should later support dynamic sender headers?
- What mapping/configuration model should later support private WCF contract mapping?
