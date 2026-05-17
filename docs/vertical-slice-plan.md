# Vertical Slice Plan

## Slice 0: Repository Scaffold

Status: complete in this scaffold.

Deliverables:

- Solution and project skeleton.
- Core contracts only.
- Mock sender only.
- Placeholder CLI.
- Smoke tests.
- Initial docs and AI agent instructions.

## Slice 1: File Parser Spike

Goal: load a tiny set of structured messages from a CSV file and convert them into `ReplayBatch`.

Candidate scope:

- Add a CSV parser as the first concrete parser implementation.
- Treat the first non-empty, non-comment line as the CSV header row.
- Ignore empty lines and lines whose first non-whitespace character is `#`.
- Convert each parsed data row into one `ReplayMessage`.
- Store the full row as a JSON object payload.
- Generate message IDs from parsed data record numbers.
- Leave headers empty by default.
- Store parser context in metadata.
- Use clear exceptions for invalid CSV input.
- Add minimal parser tests.
- Keep parsing generic and free of business mappings.

## Slice 2: Replay Engine

Goal: send a `ReplayBatch` through an `IReplaySender` and collect results.

Candidate scope:

- Add a small replay service.
- Preserve message order unless requirements change.
- Return per-message results.

## Slice 3: CLI Preview

Goal: run a simple local replay from the command line.

Candidate scope:

- Accept input file path.
- Use one parser and mock sender.
- Print a concise result summary.

## Assumptions

- The next useful vertical slice is parser-first because replay requires messages to exist.
- UI, Docker, HTTP, and persistence should wait until the core workflow is proven.

## Open Questions

- Should filtering be implemented before real sender adapters?
- What minimal CLI command shape should be considered stable?
- What later mapping/configuration model should support dynamic headers and private WCF contract mapping?
