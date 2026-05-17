# PRD 0005: CLI Experience

## Status

Draft

## Purpose

Provide a command-line interface for loading, inspecting, and replaying messages without writing code.

## Users

- Developers running local replay scenarios.
- Maintainers testing examples.
- Contributors validating parser and sender behavior.

## Requirements

- Start with a minimal placeholder until parser and engine slices exist.
- Later accept an input file path.
- Later choose a parser format.
- Later choose a sender adapter.
- Print concise result summaries.
- Exit with meaningful status codes.

## Acceptance Criteria

- The scaffold CLI prints a placeholder message and exits successfully.
- Future CLI slices use public core contracts.
- CLI does not contain business-specific mappings.
- CLI examples use synthetic data.

## Out Of Scope

- Interactive UI.
- Docker packaging.
- Secrets management.
- Business-specific command presets.

## Assumptions

- CLI should become useful after parser and replay engine slices exist.
- Early CLI output should favor clarity over rich formatting.

## Open Questions

- What command shape should become stable first?
- Should the CLI support inspection before replay?
- Should non-zero exit codes reflect any failed message or only command-level failures?
