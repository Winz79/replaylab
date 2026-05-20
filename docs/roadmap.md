# ReplayLab Roadmap

## Current Position

M1 through M7 are complete.

M7 refactored `ReplayLab.Cli` and `ReplayLab.Web` into hostable entry points so
private projects can compose around ReplayLab without modifying the public repo.
CLI hostability consumes externally registered parser and sender services through
DI. Web hostability exposes `AddReplayLabWeb()` and `MapReplayLabWeb()`, but the
current Web workflow still uses the internal CSV parser and mock sender path.
External Web parser/sender DI consumption is deferred.

## Summary Of Active Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M6 | Private Adapter Extension Model | Complete. Public contracts hardened, DI helpers added per adapter/parser project, example adapter added, ReplayLab.Core packageable and pack verified at version 0.6.0. |
| M7 | Hostable Entry Points | Complete. Hostable CLI runner, Web hosting hooks, external host sample, and consumption-model docs delivered. Web parser/sender DI consumption remains deferred follow-up scope. |
| M8 | Web External Composition | Recommended next milestone. Make `ReplayLab.Web.Hosting` consume host-provided parser/sender/workflow services through DI while preserving current Web defaults. |

## Post-M7 Planning Package

- [Post-M7 Product Direction](business-analysis/post-m7-product-direction.md)
- [Post-M7 Artifact Map](business-analysis/post-m7-artifact-map.md)
- [M8: Web External Composition](milestones/m8-web-external-composition.md)
- [Future Candidate Milestones](milestones/future-candidate-milestones.md)

## Post-M7 Candidate Tracks

The following discovery issues are future candidate milestones or candidate
tracks. They remain discovery/candidate tracks unless explicitly promoted later.

- M8 Web External Composition is the recommended next milestone. It closes the
  main M7 limitation by making the Web workflow consume host-provided services
  from DI while keeping current CSV/mock defaults.
- M9A Parser Quality / RFC-compliant CSV (`#69`) and M9B Editable Replay
  Workspace (`#68`) are competing or alternative next candidates after M8.
- Desktop AppHost / Product Shell (`#70`) is a later candidate that depends on
  M8 and likely benefits from either Parser Quality or Editable Replay Workspace.

Candidate milestone direction, not final commitment unless explicitly accepted:

- Recommended M8: Web External Composition.
- Candidate M9A: Parser Quality / RFC-compliant CSV.
- Candidate M9B: Editable Replay Workspace.
- Candidate M10: Desktop AppHost / Product Shell.

See `docs/business-analysis/post-m7-product-direction.md` for the current
post-M7 business analysis and `docs/business-analysis/post-m7-artifact-map.md`
for recommended future BRD/PRD/ADR timing.


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
- Desktop AppHost with WebView2 and self-hosted Web UI (`#70`).
- New parser library adoption.
- WebView2 desktop shell work.
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

## Future Milestones

| Milestone Or Track | Candidate Direction |
| --- | --- |
| Recommended M8 | Web External Composition |
| Candidate M9A | Parser Quality / RFC-compliant CSV |
| Candidate M9B | Editable Replay Workspace |
| Candidate M10 | Desktop AppHost / Product Shell |
