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
- Throw clear exceptions for invalid input in the first parser slice.
- Avoid business-specific mappings.
- Use synthetic examples in tests and docs.

## Format Direction

- CSV first.
- JSON later.

## Acceptance Criteria

- A parser can load a tiny sample file into `ReplayBatch`.
- Invalid input throws a predictable, clear exception.
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
- CSV is the first concrete parser implementation.
- One format is enough for the first parser slice.

## Open Questions

- What exact CSV columns should the first parser require?
- How should headers and metadata be represented in CSV?
- Should parsers support streaming for large files in the first version?
