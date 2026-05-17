# ReplayLab Intake Brief

## Intent

ReplayLab is a lightweight, generic replay and testing toolkit for loading structured messages from files, inspecting them, selecting or filtering them, and replaying them through configurable sender adapters.

## Public Scope

- Generic replay engine.
- Generic message model.
- CSV and JSON parser direction.
- Mock sender for local testing.
- Optional HTTP/WCF sender later.
- Local web UI, CLI, and Docker packaging later.

## Out of Scope

- Proprietary business formats.
- Internal WCF contracts.
- Real certificates.
- Real customer data.
- Company-specific mappings.
- Business-specific adapters.

WCF and business-specific adapters must live outside the public repository.

## Assumptions

- ReplayLab should be open-source friendly from the first commit.
- The first scaffold should define project boundaries, not production replay behavior.
- The AI Engineering Toolkit is methodology guidance only and is not copied, vendored, or referenced.

## Open Questions

- Which file format should be implemented first: JSON, CSV, or another structured text format?
- What minimum metadata should every replayed message expose?
- Should sender adapters stream results or return per-message results in batches?
