# ReplayLab Roadmap

## Current Position

M1, M2, and M3 are complete.

M4 is the current planning milestone. It should focus on the first generic non-mock sender: an HTTP Sender Preview.

M5 and M6 are directional roadmap candidates only. They should remain lightweight until M4 is planned and executed.

## Summary Of Post-M2 Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M4 | HTTP Sender Preview | Add the first generic non-mock sender once input configuration and CLI shape are clearer. |
| M5 | Minimal Web UI | Add local visual inspection only after CLI and configuration concepts have settled. |
| M6 | Private Adapter Extension Model | Document and validate extension boundaries for private adapters without implementing WCF or business-specific adapters in the public repo. |


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

## Completed Milestones

| Milestone | Outcome |
| --- | --- |
| M1 | Local CLI Replay Preview |
| M2 | Local Executable Distribution |
| M3 | Configurable Replay Inputs |