# BRD 0003: Target Users and Workflows

## Status

Draft

## Target Users

- Developers testing message-processing code locally.
- Developers investigating behavior with known structured inputs.
- Contributors building generic parsers or sender adapters.
- Maintainers reviewing replay scenarios and adapter boundaries.
- Future users who prefer CLI or local UI workflows over direct library use.

## Core User Workflows

### Load Messages

A user provides a structured file and ReplayLab converts it into a generic batch of replay messages.

### Inspect Messages

A user reviews message identifiers, payloads, headers, metadata, and parser outcomes before sending anything.

### Select Or Filter Messages

A user narrows a batch to relevant messages before replay.

### Replay Messages

A user sends selected messages through a configured sender adapter and receives per-message results.

### Review Results

A user sees which messages succeeded, failed, or were skipped, with enough context to investigate.

## Early Workflow Priority

The first useful end-to-end workflow should be:

1. Load a tiny structured file.
2. Convert it to `ReplayBatch`.
3. Send messages through `MockReplaySender`.
4. Report per-message results.

## Assumptions

- The earliest users are developers comfortable with code and command-line tools.
- Inspection and filtering should be designed before a full UI is built.
- Mock replay is valuable before real sender adapters exist.

## Open Questions

- Should filtering use a simple predicate model, a query language, or explicit selection lists?
- What result details are required for useful local investigation?
- Should the CLI or library API be treated as the first primary interface?
