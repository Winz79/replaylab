# ReplayLab Roadmap

## Product Direction

ReplayLab is now positioned as a polished, embeddable replay toolkit that developers can reference as NuGet packages, customize with parsers and adapters, and ship as CLI, Web, or Desktop replay tools.

The near-term direction is no longer persistence-first. ReplayLab should first finish the SDK/developer experience story, then add a real release/deployment path through GitHub Packages, and only then revisit local sessions if the product needs it.

## Current Position

M1 through M14 are complete.

Completed foundations include:

- Core replay contracts and models.
- CSV parsing with CsvHelper.
- Sequential replay engine.
- Mock and HTTP adapters.
- CLI preview.
- Web UI with editable replay workspace.
- Hostable CLI and Web entry points.
- Desktop AppHost with Photino.NET.
- Packageable ReplayLab SDK via local NuGet packages.
- External-style custom replay tool sample.
- Reusable Desktop hosting seam via `ReplayLab.Desktop.Hosting`.

## Summary Of Active Roadmap

| Milestone | Direction | Roadmap Intent |
| --- | --- | --- |
| M7 | Hostable Entry Points | Complete. Reusable CLI and Web host surfaces extracted so private projects can own composition roots and invoke ReplayLab workflows without modifying the public repo. |
| M8 | Desktop AppHost with Photino.NET | Complete. Desktop shell that self-hosts the ReplayLab Web UI in a native web view, with dynamic loopback port selection and graceful shutdown. |
| M9A | Parser Quality with CsvHelper | Complete. Replaced the minimal custom CSV parser with CsvHelper to handle realistic CSV inputs. Delivered in PR #96. |
| M9B | Editable Replay Workspace | Complete. Added in-place editing of parsed payload values in the Web UI grid before replay. Delivered in PR #95. |
| M9C | Editable Workspace UX Polish | Complete. Hardened the editable replay workspace UX and server-side validation. Tracked in #98 and #97. |
| M10A | Packageable ReplayLab SDK | Complete. Local NuGet packages produced for Core, parsers, adapters, and hosting libraries via `eng/pack-local.ps1`. Tracked in #99. |
| M10B | NuGet-based Custom Replay Tool Sample | Complete. External-style sample under `samples/CustomReplayTool` consumes ReplayLab via `PackageReference` and demonstrates custom parser/sender composition with the Web host. Tracked in #100. |
| M11 | SDK Composition Hardening | Complete. Parser/sender override conventions clarified with TryAdd, composition tests added, and docs updated. Delivered via #113. |
| M12 | Local Sessions / Persistence | Deferred / optional. Do not implement before SDK adoption and release automation are proven. |
| M13 | Release Automation / Portfolio Release | Complete. GitHub Packages workflow delivered and triggered via `v0.13.0-preview.1` tag. Engine bug fixes (#119–#124) included via PR #132. |
| M14 | SDK Adoption Instrumentation & Polish | Complete. Structured logging (ILogger), XML doc comments, getting-started guide, and GitHub Packages badge delivered. |
| M15 | Web Deployment & Observability Guide | In progress. #141 (Dockerize), #142 (Deploy), and #143 (Cloudflare Tunnel guide) delivered. #144 (Seq observability guide) remaining. |

## Near-Term Priorities

1. **M11 SDK composition hardening complete**  
   Parser/sender override conventions clarified with TryAdd, composition tests added, and docs updated. Delivered via #113.

2. **M13 release automation delivered**  
   GitHub Actions workflow publishes packageable ReplayLab SDK packages to GitHub Packages. Tag `v0.13.0-preview.1` pushed. Engine robustness bug fixes (#119–#124) merged via #132.

3. **M13 complete — release tagged**  
   Release `v0.13.0-preview.1` published on GitHub. Packages available via GitHub Packages.

4. **M14 complete — SDK adoption instrumentation**  
   Structured logging, XML docs, getting-started guide, and GitHub Packages badge delivered via #134–#137.

5. **M15 in progress — Web deployment & observability**  
   Dockerize (#141), deploy workflow (#142), and Cloudflare Tunnel guide (#143) delivered. Seq observability guide (#144) remaining. Plan in `docs/milestones/m15-deployment-observability.md`.

6. **Keep M12 persistence deferred**  
   Local sessions and workspace persistence are useful product features, but they are not required to prove the SDK/toolkit story.

## M11: SDK Composition Hardening

### Goal

Make the consumer composition story clear and stable enough for external developers.

### Context

ReplayLab now exposes packageable Core, parser, adapter, CLI/Web/Desktop hosting surfaces. The next SDK-quality step is to make consumer registration and override behavior obvious.

### Scope

- Review current DI registration conventions.
- Clarify how consumers override default parser and sender registrations.
- Document recommended registration order.
- Add helper methods only if they remove obvious friction.
- Update sample docs if composition conventions change.
- Keep static package/reference composition as the default model.

### Linked Issues

- #113 — Harden SDK composition conventions

### Out Of Scope

- Persistence/session storage.
- Dynamic plugin loading.
- Public NuGet.org publishing.
- Installer creation.
- Business-specific adapters.

## M12: Local Sessions / Persistence

### Goal

Persist local replay workspace state later if the product direction requires it.

### Status

**Deferred / optional.**

Persistence is not part of the immediate roadmap. ReplayLab should first prove SDK composition, package consumption, and release automation. M12 can be revisited after the toolkit story is stable and there is a concrete workflow need for save/load sessions.

### Possible Future Scope

- Save and reload local replay workspace state.
- Preserve imported messages, edits, selected rows, and replay results.
- Export/import replay plans.
- Keep persistence local-first and business-agnostic.

### Out Of Scope For Now

- Database-backed storage.
- Cloud sync.
- Authentication.
- Business-specific persistence.
- Release-blocking work.

## M13: Release Automation / Portfolio Release

### Status

**Complete.** Tag `v0.13.0-preview.1` pushed and release published. Engine robustness fixes (#119–#124) merged via #132.

### Goal

Turn ReplayLab from a locally packable SDK into a small, releasable developer toolkit.

### Delivered

- GitHub Actions release workflow triggered by version tags (`v*.*.*`).
- Restore, build, test, pack, and publish pipeline.
- Selected SDK projects published to GitHub Packages.
- `--skip-duplicate` for safe reruns.
- Release notes and manual tag checklist prepared.
- Engine null-guard, error handling, and cancellation hardening (#119–#124).

### Linked Issues

- #111 — Publish ReplayLab packages to GitHub Packages on version tag
- #112 — Prepare next portfolio release
- #132 — Harden SequentialReplayEngine against nulls and improve error handling

## M14: SDK Adoption Instrumentation & Polish

### Status

**Complete.** All four slices delivered: structured logging (#134), XML docs (#135), getting-started guide (#136), GitHub Packages badge (#137).

Make the ReplayLab SDK observable, documented, and discoverable for external developers.

### Context

M10A/B proved the packageable SDK works. M11 hardened composition. M13 proved the release path. M14 should add the observability and documentation that makes the SDK production-ready for external consumers.

### Scope

- Add `ILogger`/structured logging to `SequentialReplayEngine`, `CsvParser`, `HttpSender` (via `Microsoft.Extensions.Logging.Abstractions`).
- Add XML doc comments on all public `ReplayLab.Core` API surfaces.
- Write `docs/getting-started.md`: NuGet source setup, `PackageReference`, implementing `IMessageParser`/`IReplaySender`, hosting the Web UI.
- Add GitHub Packages download badge to `README.md`.

### Out Of Scope

- Persistence/session storage (M12, deferred).
- NuGet.org publishing.
- Docker, installers, dynamic plugins.
- New parser formats or adapter types.

## M15: Web Deployment & Observability Guide

### Status

In progress. #141, #142, and #143 delivered. #144 (Seq observability guide)
remains.

### Goal

Make the ReplayLab Web UI deployable and observable. Dockerize the Web app,
auto-deploy on version tags, and document Cloudflare Tunnel and Seq setup.

### Context

M14 added structured logging. M15 makes the logging visible (Seq) and makes
the Web UI reachable (Docker + Cloudflare Tunnel + GitHub Actions).
#141 (Dockerize), #142 (Deploy workflow), and #143 (Cloudflare Tunnel guide) are done.

### Scope

- Multi-stage Dockerfile + docker-compose (Web + Seq).
- GitHub Actions deploy workflow (`deploy-web.yml`) triggered by version tags.
- Cloudflare Tunnel setup guide (`docs/cloudflare-tunnel.md`).
- Seq observability guide (`docs/observability-seq.md`).

### Linked Issues

- #141 — Dockerize ReplayLab.Web with multi-stage build and docker-compose (Web + Seq)
- #142 — GitHub Actions deploy-web workflow triggered by version tags
- #143 — Write Cloudflare Tunnel setup guide for self-hosted Web UI
- #144 — Write Seq observability guide for structured logging

### Out Of Scope

- Kubernetes, managed cloud, Cloudflare Pages/Workers.
- Desktop or CLI deployment.
- Seq code dependency in any ReplayLab project.
- New parser or adapter types.

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
| M10A | Packageable ReplayLab SDK |
| M10B | NuGet-based Custom Replay Tool Sample |
| M11 | SDK Composition Hardening |
| M13 | Release Automation / Portfolio Release |
| M14 | SDK Adoption Instrumentation & Polish |

## Future / Parking Lot

| Track | Direction |
| --- | --- |
| Persistence | Deferred. Revisit only after SDK adoption and release automation are proven. |
| Dynamic plugins | Deferred. Static package/reference composition remains the default model. |
| NuGet.org publishing | Deferred. GitHub Packages is the first release target. |
| Installer creation | Deferred. Not needed for the current SDK/toolkit story. |
