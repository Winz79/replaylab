# ReplayLab Roadmap

## Current Position

M1, M2, M3, M4, and M5 are complete.

M6 is the current milestone. It hardens the public contracts, adds DI
registration helpers, adds a compilable example adapter, and makes
`ReplayLab.Core` packageable and pack verified so private adapter development is
concretely possible.

M7 will refactor CLI and Web into hostable entry points so private projects
can compose their own adapters with hostable entry points instead of the current
repo-owned application shells.

## Summary Of Active Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M6 | Private Adapter Extension Model | Harden public contracts, add DI helpers per adapter/parser project, add compilable example adapter, make ReplayLab.Core packageable and pack verified at version 0.6.0. |
| M7 | Hostable Entry Points | Refactor CLI and Web into hostable entry points. Private projects register their adapters and call ReplayLabHost.RunCli() or ReplayLabHost.RunWeb(). |


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

Make it concretely possible for any developer to build a private ReplayLab
adapter outside the public repo by hardening the public contracts, providing DI
registration helpers, adding a compilable example adapter, and publishing
`ReplayLab.Core` packageable and pack verified as a NuGet package.

### User Value

Developers can build private sender adapters and parsers against a stable,
packageable and pack verified `ReplayLab.Core` contract without forking or
cloning this repo.

### Outcome

- Public contracts in `ReplayLab.Core` hardened (breaking changes accepted).
- `IServiceCollection` extension methods added to each adapter and parser project.
- `ReplayLab.Adapters.Example` with `FileReplaySender` as a compilable proof of the extension seam.
- `ReplayLab.Core` NuGet packaging metadata added and `dotnet pack` verified.
- Extension guide, ADR 0008, and PRD 0008 documenting the public contracts, DI pattern, and composition boundaries.

### Explicit Out Of Scope

- WCF implementation in the public repo.
- Private business contract models.
- Customer data or proprietary payload examples.
- Hostable CLI or Web entry points (M7).
- NuGet publishing for CLI, Web, parsers, or adapters (M7).
- AppHost or desktop entry point.

### Status

In progress — see `docs/milestones/m6-private-adapter-extension-model.md`.

## M7: Hostable Entry Points

### Goal

Refactor `ReplayLab.Cli` and `ReplayLab.Web` into hostable libraries so a
private project can register its own adapters, call into the ReplayLab entry
points, and get a fully working CLI and Web UI without modifying this repo.

### User Value

Teams can ship their own ReplayLab-powered CLI and Web UI by composing private
adapters with hostable entry points via DI registration.

### Possible Scope

- Refactor `ReplayLab.Cli` startup to be callable as a library
  (`ReplayLabHost.RunCli(args, services)`).
- Refactor `ReplayLab.Web` startup similarly.
- Publish CLI and Web as NuGet packages.
- Private project sets up a host, registers its adapters, calls into ReplayLab.
- AppHost/desktop entry point if scope permits.

### Dependency On Previous Milestones

M7 depends on M6's stable contracts and DI registration pattern.

## Completed Milestones

| Milestone | Outcome |
| --- | --- |
| M1 | Local CLI Replay Preview |
| M2 | Local Executable Distribution |
| M3 | Configurable Replay Inputs |
| M4 | HTTP Sender Preview |
| M5 | Minimal Web UI |

## Future Milestones

| Milestone | Candidate Direction |
| --- | --- |
| M7 | Hostable Entry Points |
