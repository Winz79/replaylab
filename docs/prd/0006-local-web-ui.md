# PRD 0006: Local Web UI

## Status

Draft

## Purpose

Provide a future local interface for inspecting, selecting, filtering, and replaying messages.

## Users

- Developers who prefer visual inspection over CLI output.
- Maintainers reviewing replay examples.
- Users comparing message payloads and replay results.

## Requirements

- Show loaded messages.
- Show message payloads and metadata.
- Support selection and filtering.
- Trigger replay through configured adapters.
- Show per-message results.
- Run locally without requiring private infrastructure.

## Acceptance Criteria

- UI uses public core concepts.
- UI does not introduce persistence by default.
- UI examples use synthetic data.
- UI remains optional and separate from core.

## Out Of Scope

- Initial scaffold.
- Hosted service.
- User accounts.
- Persistence.
- Business-specific dashboards.

## Assumptions

- UI should wait until parser, replay engine, and CLI behavior are clearer.
- Local-only UI is enough for the foreseeable scope.

## Open Questions

- Should the UI be a separate app or packaged with the CLI?
- Should filtering be designed in core before UI work starts?
- Should replay results be exportable?
