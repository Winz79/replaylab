# ReplayLab Roadmap

## Product Direction

ReplayLab should become a polished, embeddable replay toolkit that developers can reference as local NuGet packages, customize with parsers and adapters, and ship as a Web or Desktop replay tool.

The next milestones prioritize UX polish and package-based developer adoption. Persistence and local sessions are intentionally deferred until the package composition path is proven.

## Current Position

M1 through M8 are complete.

M9A (Parser Quality with CsvHelper) and M9B (Editable Replay Workspace) are complete.

M9C is complete.

## Summary Of Active Roadmap

| Milestone | Candidate Direction | Roadmap Intent |
| --- | --- | --- |
| M7 | Hostable Entry Points | Complete. Reusable CLI and Web host surfaces extracted so private projects can own composition roots and invoke ReplayLab workflows without modifying the public repo. Web parser decoupling (`IWebReplayParser`) shipped in the M7 closeout as Web external composition. |
| M8 | Desktop AppHost with Photino.NET | Complete. Desktop shell that self-hosts the ReplayLab Web UI in a native web view, with dynamic loopback port selection and graceful shutdown. |
| M9A | Parser Quality with CsvHelper | Complete. Replaced the minimal custom CSV parser with CsvHelper to handle real-world CSV inputs. Delivered in PR #96. |
| M9B | Editable Replay Workspace | Complete. Added in-place editing of parsed payload values in the Web UI grid before replay, with dirty state, row reset, and edited payload submission. Delivered in PR #95. |
| M9C | Editable Workspace UX Polish | Complete. Hardened the editable replay workspace UX and server-side validation before broadening the surface. Tracked in #98 and #97. |
| M10A | Packageable ReplayLab SDK | Complete. Local NuGet packages produced for Core, parsers, adapters, and hosting libraries via `eng/pack-local.ps1`. Tracked in #99. |
| M10B | NuGet-based Custom Desktop/Web Tool Sample | Complete. External-style sample under `samples/CustomReplayTool` consumes ReplayLab via `PackageReference` and demonstrates custom parser/sender composition with the Web host. Tracked in #100. |
| M11 | Composition / Extension Model Hardening | Improve parser/adapter composition after the package-based sample proves the static composition path. Tracked in #101. |
| M12 | Local Sessions / Persistence | Deferred. Do not implement before UX and package adoption are proven. |

## Post-M8 Notes

- Discovery issues #68 (editable Web grid) and #69 (RFC-compliant CSV parser) were promoted into implementation and delivered as M9B and M9A.
- Issue #102 tracks closing or updating #68 and #69 with clear references to the delivered work.
- Implementation plan for M8 desktop apphost: [docs/plans/m8-desktop-apphost.md](docs/plans/m8-desktop-apphost.md).

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

Make it concretely possible for any developer to build a private ReplayLab adapter outside the public repo by hardening the public contracts, providing DI registration helpers, adding a compilable example adapter, and publishing `ReplayLab.Core` packageable and pack verified as a NuGet package.

### User Value

Developers can build private sender adapters and parsers against a stable, packageable and pack verified `ReplayLab.Core` contract without forking or cloning this repo.

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
- NuGet publishing for CLI, Web, parsers, or adapters (M10A).
- AppHost or desktop entry point.

### Status

Complete — see `docs/milestones/m6-private-adapter-extension-model.md`.

## M7: Hostable Entry Points

### Goal

Refactor `ReplayLab.Cli` and `ReplayLab.Web` into hostable libraries so a private project can register its own adapters, call into the ReplayLab entry points, and get a fully working CLI and Web UI without modifying this repo.

### User Value

Teams can ship their own ReplayLab-powered CLI and Web UI by composing private adapters with hostable entry points via DI registration.

### Possible Scope

- Refactor `ReplayLab.Cli` startup into a hostable entry point.
- Refactor `ReplayLab.Web` startup into a hostable entry point.
- Define the composition model and ownership boundary for private hosts.
- Document how private projects register adapters/parsers and consume the hostable entry points.

### Explicit Out Of Scope (at the time of M7)

- ~~Editable Web grid values before replay (`#68`)~~ — delivered in M9B / PR #95.
- ~~RFC-compliant CSV parser strategy (`#69`)~~ — delivered in M9A / PR #96.
- Desktop AppHost with Photino.NET and self-hosted Web UI (`#70`) — delivered in M8.
- Product UX expansion beyond current CLI/Web workflows.
- Business-specific adapters.

### Dependency On Previous Milestones

M7 depends on M6's stable contracts and DI registration pattern.

### Status

Complete — see `docs/milestones/m7-hostable-entry-points.md` and `docs/retrospectives/m7-hostable-entry-points.md`.

## M9C: Editable Workspace UX Polish

### Goal

Make the editable replay workspace professional enough for demo and daily use.

### Context

The editable grid delivered in M9B supports changing values before replay, row reset, dirty state, and edited payload submission. M9C focuses on product-quality UX rather than adding new backend capability.

Server-side validation hardening tracked in #97 should land alongside or before this milestone.

### Scope

- Improve changed-value display.
- Replace the visible "Reset row" button with a compact icon/action.
- Hide or minimize the reset action header.
- Separate row selection from row editing.
- Add an explicit row edit action/mode.
- Prevent accidental select/unselect while editing.
- Preserve existing replay behavior.

### Linked Issues

- #98 — Polish editable replay workspace UX
- #97 — Harden editable replay payload validation server-side

### Out Of Scope

- Persistence.
- Session save/load.
- New parser/adapter functionality.
- Dynamic plugin loading.

## M10A: Packageable ReplayLab SDK

### Goal

Make ReplayLab consumable as local NuGet packages by external solutions.

### Context

The strategic adoption path is that a developer can reference ReplayLab packages, provide custom parsers/adapters, and quickly ship a replay tool. This milestone proves local package consumption before any public NuGet publishing decision.

### Scope

- Identify packageable projects.
- Add or normalize package metadata.
- Create a local pack script (for example `eng/pack-local.ps1`).
- Pack packages into `artifacts/packages`.
- Verify that packages restore from a local feed.
- Document the package set and local feed workflow.

### Candidate Packages

- `ReplayLab.Core`
- `ReplayLab.Parsers.Csv`
- `ReplayLab.Adapters.Mock`
- `ReplayLab.Adapters.Http`
- `ReplayLab.Cli.Hosting`
- `ReplayLab.Web.Hosting`

Investigate whether `ReplayLab.Desktop` should remain an app and whether a new `ReplayLab.Desktop.Hosting` package is needed for reusable desktop bootstrap.

### Linked Issues

- #99 — Package ReplayLab SDK for local NuGet consumption
- #101 — Extract reusable Desktop hosting seam (if applicable)

### Out Of Scope

- Publishing to nuget.org.
- Signing packages.
- Release automation.
- Dynamic plugin loading.

## M10B: NuGet-based Custom Desktop/Web Tool Sample

### Goal

Provide a realistic external-style sample that references ReplayLab packages, not project references, and demonstrates custom parser/adapter composition.

### Context

The killer adoption story is: reference ReplayLab, customize parser/adapter, and ship a replay tool quickly. The sample should prove that story without using project references.

### Scope

- Add a sample solution under `samples/` (for example `samples/CustomReplayTool`).
- Add a `NuGet.config` pointing to `artifacts/packages`.
- Reference ReplayLab packages via `PackageReference`.
- Implement a fictional custom parser.
- Implement a fictional custom sender/adapter.
- Compose the ReplayLab Web and/or Desktop host with the custom services.
- Add build/run documentation.

### Linked Issues

- #100 — Add NuGet-based custom replay tool sample

### Out Of Scope

- Publishing packages publicly.
- Dynamic plugins.
- WCF/private adapter implementation.
- Installer creation.
- Persistence/session storage.

## M11: Composition / Extension Model Hardening

### Goal

Improve parser/adapter composition after the package-based sample proves the static composition path.

### Scope

- Clarify DI composition conventions.
- Decide whether dynamic plugins are needed or whether static package/reference composition is enough for now.
- Consider `IReplayLabModule` or similar only if justified.
- Add ADR if this becomes an architectural decision.

### Linked Issues

- #101 — Extract reusable Desktop hosting seam (architecture evaluation)

### Out Of Scope

- Dynamic plugin redesign unless justified.
- Business-specific adapter packages.

## M12: Local Sessions / Persistence

### Goal

Persist local replay workspace state later.

### Status

**Deferred.** Do not implement before UX and package adoption are proven.

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
| M8 | Desktop AppHost with Photino.NET |
| M9A | Parser Quality with CsvHelper |
| M9B | Editable Replay Workspace |
| M9C | Editable Workspace UX Polish |

## Future Milestones

| Milestone Or Track | Candidate Direction |
| --- | --- |
| M10A | Packageable ReplayLab SDK — In progress. Make ReplayLab consumable as local NuGet packages by external solutions. See #99. |
| M10B | NuGet-based Custom Desktop/Web Tool Sample — Provide a realistic external-style sample that references ReplayLab packages, not project references. See #100. |
| M11 | Composition / Extension Model Hardening — Improve parser/adapter composition after the package-based sample proves the static composition path. See #101. |
| M12 | Local Sessions / Persistence — Deferred. Do not implement before UX and package adoption are proven. |
