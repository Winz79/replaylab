# Future Candidate Milestones

This file collects post-M13 candidate milestone dossiers. These are planning
artifacts, not final commitments. Promote a candidate only after its value,
dependencies, risks, and required artifacts are accepted.

## Candidate M14: SDK Adoption Instrumentation & Polish

### Goal

Add observability and documentation to make the SDK production-ready for
external developers.

### User Value

- Developers can see structured logs from the engine, parser, and sender.
- All public API surfaces have IntelliSense documentation.
- A getting-started guide reduces time-to-first-replay.
- A badge on the README signals available packages.

### Dependency On Previous Work

- Builds on M10A/M10B (packageable SDK), M11 (composition hardening), M13 (release path).

### Required Artifacts

- Plan doc: `docs/plans/m14-sdk-adoption-instrumentation.md`
- PRD update if behavior contracts change.

### Candidate Slices

- Add `ILogger`/structured logging to `SequentialReplayEngine`, `CsvParser`, `HttpSender` using `Microsoft.Extensions.Logging.Abstractions`.
- Add XML doc comments on all public `ReplayLab.Core` API surfaces.
- Write `docs/getting-started.md`: NuGet source setup, `PackageReference`, implementing `IMessageParser`/`IReplaySender`, hosting the Web UI.
- Add GitHub Packages download badge to `README.md`.

### Explicit Non-Goals

- Persistence or session storage (M12, deferred).
- NuGet.org publishing.
- Docker, installers, dynamic plugins.
- New parser formats or adapter types.

### Readiness

Ready for planning. Promote to active milestone after conductor reviews the plan.

### Recommended Next Action

Run `plan-milestone` skill on M14 to produce a detailed milestone plan and create implementation issues.

## Candidate M12: Local Sessions / Persistence

### Goal

Persist local replay workspace state later if the product direction requires it.

### Status

**Deferred.** Reopen only when >=1 external consumer exists and the SDK/toolkit
story is stable.

### Possible Future Scope

- Save and reload local replay workspace state.
- Preserve imported messages, edits, selected rows, and replay results.
- Export/import replay plans.
- Keep persistence local-first and business-agnostic.

### Non-Goals

- Database-backed storage, cloud sync, authentication, business-specific persistence.
