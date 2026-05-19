# ReplayLab Roadmap

## Current Position

M1, M2, M3, M4, and M5 are complete.

M6 is the current planning milestone. It should focus on documenting the private adapter extension model for ReplayLab.

## Summary Of Post-M2 Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M6 | Private Adapter Extension Model | Document and validate extension boundaries for private adapters without implementing WCF or business-specific adapters in the public repo. |


## M4: HTTP Sender Preview

### Goal

Introduce the first generic non-mock sender by sending replay messages to an HTTP endpoint in a local preview workflow.

### User Value

Developers can validate ReplayLab against local test services, request inspectors, or mock HTTP endpoints without writing adapter code.

### Outcome

- Basic HTTP sender adapter using public .NET HTTP primitives.
- Configurable method, URL, headers, and body mapping from generic `ReplayMessage` values.
- Local-only sample using a synthetic endpoint or documented test receiver.
- CLI selection of mock sender versus HTTP sender.
- Clear result reporting for status code, success, and failure.
- System.CommandLine adoption for CLI growth.

### Status

**Complete** - M4 shipped with HTTP sender adapter, CLI sender selection, and local HTTP preview documentation.

## M5: Minimal Web UI

### Goal

Provide a local visual interface for loading, inspecting, selecting, and replaying messages using the concepts already proven in the CLI.

### User Value

Developers can inspect payloads and replay results faster when CLI summaries are not enough.

### Outcome

- Local-only ASP.NET Core Razor Pages app in `src/ReplayLab.Web`.
- Browser CSV upload and parsed message preview.
- Tabulator-based data table workflow (replaced initial card-based UI).
- Mock replay execution from the UI with per-message results.
- No persistence - short-lived workflow state only.

### Status

**Complete** - M5 shipped with local Razor Pages app, CSV upload/preview, and mock replay execution.

## M6: Private Adapter Extension Model

### Goal

Define how private integrations can extend ReplayLab outside the public repository without adding WCF, proprietary contracts, or business-specific mapping code to the public repo.

### User Value

Teams can understand how to keep private replay needs separate while still benefiting from the public core, parser, CLI, and generic adapter surfaces.

### Possible Scope

- Public extension guidance for private adapter composition.
- Example boundaries for mapping from generic `ReplayMessage` values to private contract objects outside the repo.
- Adapter capability metadata discussion if needed by M4 or M5.
- Documentation checks that prove public contracts remain generic.
- Private-adapter examples described as boundaries and flow diagrams, not implementation code.

### Explicit Out Of Scope

- WCF implementation in the public repo.
- Private business contract models.
- Customer data or proprietary payload examples.
- Private mapping rules.
- Certificate-specific implementation.
- Public package commitments not already accepted through distribution planning.

### Main Risks

- Extension guidance could accidentally encode private architecture.
- Capability metadata could overcomplicate the public adapter contract.
- Users may expect official support for private integrations that belong outside the repo.

### Dependency On Previous Milestones

M6 should depend on M4's generic sender learning and M5's composition needs, if the Web UI proceeds. It can be pulled earlier only as documentation if private extension pressure starts influencing public design.

## Completed Milestones

| Milestone | Outcome |
| --- | --- |
| M1 | Local CLI Replay Preview |
| M2 | Local Executable Distribution |
| M3 | Configurable Replay Inputs |
| M4 | HTTP Sender Preview |
| M5 | Minimal Web UI |