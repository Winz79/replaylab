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

- Support at least one structured public file format first.
- Keep parser output generic.
- Return useful errors for invalid input.
- Avoid business-specific mappings.
- Use synthetic examples in tests and docs.

## Candidate Formats

- JSON.
- CSV.

## Acceptance Criteria

- A parser can load a tiny sample file into `ReplayBatch`.
- Invalid input has predictable error behavior.
- Parser tests use synthetic data.
- Parser implementation does not depend on sender adapters.

## Out Of Scope

- Proprietary formats.
- WCF contracts.
- Customer data.
- Persistence.
- Full filtering language.

## Assumptions

- File parsing should be implemented before real sender adapters.
- One format is enough for the first parser slice.

## Open Questions

- Should JSON or CSV be implemented first?
- Should parser errors be exceptions, structured results, or both?
- Should parsers support streaming for large files in the first version?
