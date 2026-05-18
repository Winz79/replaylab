# ReplayLab Roadmap

## Current Position

M2 is the current execution milestone. It should stay focused on local executable distribution for the existing CLI preview and should not expand into packaging, Docker, HTTP sending, Web UI, or private adapter work.

M3 is the next planning milestone. The recommended direction is configurable replay inputs: make the local CLI preview useful across more generic input cases before adding new sender or UI surfaces.

M4 through M6 are directional only. They should remain lightweight roadmap candidates until M3 is planned and M2 is complete.

## Summary Of Post-M2 Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M3 | Configurable Replay Inputs | Stabilize how users describe input format, parser choice, basic replay options, and sample-driven CLI behavior. |
| M4 | HTTP Sender Preview | Add the first generic non-mock sender once input configuration and CLI shape are clearer. |
| M5 | Minimal Web UI | Add local visual inspection only after CLI and configuration concepts have settled. |
| M6 | Private Adapter Extension Model | Document and validate extension boundaries for private adapters without implementing WCF or business-specific adapters in the public repo. |

## M3: Configurable Replay Inputs

### Goal

Let users run the local CLI preview with explicit, generic input configuration instead of relying on one implicit CSV-to-mock-sender path.

### User Value

Developers can try ReplayLab against more realistic synthetic samples while still staying inside a small local workflow. Maintainers can evaluate input decisions before adding HTTP sending or UI behavior.

### Possible Scope

- A small configuration surface for selecting input file, parser format, and basic replay behavior.
- CLI options or a minimal config file, chosen deliberately rather than both by default.
- Clear validation errors for unsupported formats, missing files, or invalid configuration.
- Additional synthetic samples that exercise generic input cases.
- Documentation for the stable M3 command shape and configuration expectations.

### Explicit Out Of Scope

- Full configuration DSL.
- Business-specific mappings, private formats, WCF contracts, or private adapter implementation.
- HTTP sender.
- Web UI.
- Persistence.
- Docker.
- NuGet or package distribution unless directly needed to keep the M2 executable path usable.
- RFC-complete CSV parsing unless accepted as a separate parser decision.

### Main Risks

- The configuration shape could become too broad before the replay model is stable.
- CLI options and config-file behavior could duplicate each other.
- Parser improvements could overtake the real M3 goal of configurable replay inputs.
- Private adapter needs could leak into the public configuration model.

### Dependency On Previous Milestones

M3 depends on M1's local CLI replay preview and M2's local executable distribution path. It should assume users can run the CLI locally and focus on making that run configurable.

## M4: HTTP Sender Preview

### Goal

Introduce the first generic non-mock sender by sending replay messages to an HTTP endpoint in a local preview workflow.

### User Value

Developers can validate ReplayLab against local test services, request inspectors, or mock HTTP endpoints without writing adapter code.

### Possible Scope

- Basic HTTP sender adapter using public .NET HTTP primitives.
- Configurable method, URL, headers, and body mapping from generic `ReplayMessage` values.
- Local-only sample using a synthetic endpoint or documented test receiver.
- CLI selection of mock sender versus HTTP sender.
- Clear result reporting for status code, success, and failure.

### Explicit Out Of Scope

- Authentication schemes beyond simple public examples.
- Certificate management.
- Retry policy framework.
- Hosted service or long-running daemon.
- WCF, customer endpoints, private adapters, or business-specific mappings.
- Web UI.

### Main Risks

- HTTP configuration can become a general integration framework too early.
- Error handling and retries may expand beyond preview needs.
- Sender configuration could expose weaknesses in the M3 input configuration model.

### Dependency On Previous Milestones

M4 should depend on M3's chosen configuration shape so sender selection and sender settings do not invent a second configuration model.

## M5: Minimal Web UI

### Goal

Provide a local visual interface for loading, inspecting, selecting, and replaying messages using the concepts already proven in the CLI.

### User Value

Developers can inspect payloads and replay results faster when CLI summaries are not enough.

### Possible Scope

- Local-only app or host for loading configured replay inputs.
- Message list, payload inspection, and replay result summary.
- Basic selection or filtering if the CLI/config model already supports it.
- Mock sender and possibly HTTP sender composition if M4 has stabilized.
- Documentation that positions the Web UI as optional.

### Explicit Out Of Scope

- Web UI before CLI/configuration stability.
- Hosted service.
- User accounts.
- Persistence by default.
- Business-specific dashboards.
- Private adapter implementation.

### Main Risks

- UI work could force premature product decisions about filtering, persistence, and workflows.
- The project could become UI-led before the core replay flow is stable.
- Packaging the UI could distract from the local executable story.

### Dependency On Previous Milestones

M5 should depend on M3's configuration model and should ideally follow M4 only if HTTP sender behavior is small and stable enough to expose safely.

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

## Recommended M3 Direction

Plan M3 as "Configurable Replay Inputs." This keeps the project on the CLI-first path after M2, strengthens the local executable story, and avoids adding HTTP, Web UI, or private adapter complexity before input and configuration expectations are clear.

M3 should answer one product question: what is the smallest generic configuration model that lets a user run ReplayLab against a chosen input in a repeatable local workflow?

## Draft M3 Planning Structure

### PRD-Light Outline

- Status: Draft after M2 execution is stable.
- Purpose: Make replay input selection and basic local replay behavior explicit.
- Users: local developers, maintainers, and contributors using synthetic replay samples.
- Problem: the current preview proves a path but does not define how users select inputs or replay options.
- Goals:
  - Provide one clear M3 configuration entry point.
  - Keep CSV as the first supported parser unless an ADR changes that.
  - Keep the mock sender as the default sender.
  - Produce predictable validation and exit-code behavior.
  - Document supported and unsupported input configuration.
- Non-goals:
  - HTTP sender.
  - Web UI.
  - Private adapters.
  - Full parser rewrite.
  - General-purpose configuration DSL.
- Acceptance criteria:
  - A user can run the published CLI with an explicit input configuration.
  - Unsupported configuration fails with concise, actionable errors.
  - Synthetic samples cover the documented M3 path.
  - Existing M1/M2 preview behavior remains understandable.

### ADR Candidates

- ADR: M3 configuration entry point: CLI options, config file, or hybrid.
- ADR: Parser selection and format naming for public inputs.
- ADR: Configuration validation and error reporting boundary.
- ADR: How replay input configuration relates to future sender configuration.
- ADR: Whether CSV parser limitations stay documented or become M3 parser work.

### Vertical Slices

1. Define the M3 configuration boundary and user-facing command shape.
2. Add validation behavior for missing, unsupported, or inconsistent input configuration.
3. Support one explicit replay input path using the existing CSV parser and mock sender.
4. Add or revise synthetic samples for the documented configuration workflow.
5. Update CLI and executable usage docs so M2 distribution and M3 configuration work together.
6. Decide which future configuration needs are deferred to M4+.

## Candidate M3 Issues

Do not create these issues yet.

1. Docs/Decision: choose the M3 configuration entry point for replay inputs.
2. CLI: support explicit replay input configuration for the current CSV preview.
3. Validation: return clear errors for unsupported parser formats and invalid input paths.
4. Samples: add synthetic configurable-input examples for the M3 workflow.
5. Tests: cover configured replay input behavior through CLI-level tests.
6. Docs: document M3 usage from source and from the M2 published executable.

## Risks And Open Questions

- Should M3 use CLI flags first, a config file first, or a deliberately tiny hybrid?
- What option names should become stable enough for future HTTP sender configuration?
- Should parser format names be user-facing now, or inferred until a second parser exists?
- How much CSV parser improvement is necessary for realistic synthetic samples?
- Should replay options such as dry-run, inspect-only, or max-count be part of M3 or deferred?
- How should exit codes distinguish command/configuration errors from replay result failures?
- What compatibility promise, if any, should be made before a public package release?
