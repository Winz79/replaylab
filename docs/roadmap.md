# ReplayLab Roadmap

## Current Position

M1 through M7 are complete.

M8 is the current milestone. It builds a Desktop AppHost with Photino.NET and a self-hosted Web UI on top of the hostable Web entry points established in M7.

M7 established the hostable entry point boundary for private projects, and the Web external composition follow-up (decoupling the Web parser workflow from CSV assumptions via `IWebReplayParser`) shipped in the M7 closeout. M8 uses that seam to make the ReplayLab Web experience available inside a desktop shell while keeping the composition root explicit.

## Summary Of Active Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M7 | Hostable Entry Points | Complete. Reusable CLI and Web host surfaces extracted so private projects can own composition roots and invoke ReplayLab workflows without modifying the public repo. Web parser decoupling (`IWebReplayParser`) shipped in the M7 closeout as Web external composition. |
| M8 | Desktop AppHost with Photino.NET | Current milestone. Build a desktop shell that self-hosts the ReplayLab Web UI, embeds it in the platform-native web view, and owns window lifecycle and local server startup. |

## Post-M7 Candidate Tracks

Implementation plan for M8 desktop apphost:
- [docs/plans/m8-desktop-apphost.md](docs/plans/m8-desktop-apphost.md)

The following discovery issues are future candidate milestones or candidate
tracks. They are not part of M8 unless explicitly promoted later.

- `#69` RFC-compliant CSV parser strategy is a parser-quality candidate that is
  independent from M8 desktop apphost work.
- `#68` editable Web grid values before replay was promoted and delivered by
  `#92` as the Web editable replay workspace.


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

Complete — see `docs/milestones/m6-private-adapter-extension-model.md`.

## M7: Hostable Entry Points

### Goal

Refactor `ReplayLab.Cli` and `ReplayLab.Web` into hostable libraries so a
private project can register its own adapters, call into the ReplayLab entry
points, and get a fully working CLI and Web UI without modifying this repo.

### User Value

Teams can ship their own ReplayLab-powered CLI and Web UI by composing private
adapters with hostable entry points via DI registration.

### Possible Scope

- Refactor `ReplayLab.Cli` startup into a hostable entry point.
- Refactor `ReplayLab.Web` startup into a hostable entry point.
- Define the composition model and ownership boundary for private hosts.
- Document how private projects register adapters/parsers and consume the
  hostable entry points.

### Explicit Out Of Scope

- Editable Web grid values before replay (`#68`).
- RFC-compliant CSV parser strategy (`#69`).
- Desktop AppHost with Photino.NET and self-hosted Web UI (`#70`).
- New parser library adoption.
- Desktop shell work beyond the accepted Photino.NET direction.
- Product UX expansion beyond current CLI/Web workflows.
- Business-specific adapters.

### Dependency On Previous Milestones

M7 depends on M6's stable contracts and DI registration pattern.

### Status

Complete — see `docs/milestones/m7-hostable-entry-points.md` and
`docs/retrospectives/m7-hostable-entry-points.md`.

## Completed Milestones

| Milestone | Outcome |
| --- | --- |
| M1 | Local CLI Replay Preview |
| M2 | Local Executable Distribution |
| M3 | Configurable Replay Inputs |
| M4 | HTTP Sender Preview |
| M5 | Minimal Web UI |
| M6 | Private Adapter Extension Model |
| M7 | Hostable Entry Points |
| M9B | Editable Replay Workspace |

## Future Milestones

| Milestone Or Track | Candidate Direction |
| --- | --- |
| M9A | Parser Quality with CsvHelper | Post-M8 track. Replace the minimal custom CSV parser with CsvHelper to handle real-world CSV inputs. See #91. |
| Candidate M10 | Persistence / sessions or additional product-shell candidates | Future track after M9A and the current editable workspace slice are complete. |
