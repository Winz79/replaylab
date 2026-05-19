# ADR 0007: M5 Web UI Architecture

## Status

Accepted

## Context

M5 introduced the first ReplayLab Web UI. The roadmap and PRD 0006 described a
future local interface for inspecting and replaying messages. The first web
milestone needed a smaller architecture boundary so UI work did not pull the
project into hosting, persistence, authentication, private adapters, or a broad
frontend redesign.

ReplayLab already had the core replay model, CSV parser, sequential replay
engine, and mock sender needed for a local inspection workflow. M5 proved
that these existing pieces can be composed from a browser UI before adding more
sender surfaces or workflow concepts.

## Decision

M5 Web UI was implemented as `src/ReplayLab.Web`, an ASP.NET Core Razor
Pages app.

The app is local-only. Users upload a CSV file through the browser, the app
parses it with the existing CSV parser, replays messages through the existing
mock sender, and shows preview and replay results in the browser.

M5 did not add durable persistence. Uploaded CSV contents, parsed messages,
and replay results were kept in short-lived local workflow state only when
needed to support the upload → preview → replay flow. M5 did not add database
storage, uploaded file storage, saved replay history, background processing, or
multi-user workflow state.

## Outcome

M5 shipped with:
- Razor Pages app in `src/ReplayLab.Web`
- Browser CSV upload and parsed message preview
- Tabulator.js data table (replaced initial card-based approach via issue #50)
- Mock replay execution from the UI with per-message results
- No persistence - short-lived workflow state only
- HTTP sender not exposed in UI (deferred to future milestone)

## Rationale

Razor Pages is the smallest fit for a server-rendered local UI in the existing
.NET solution. It avoids adding a separate frontend stack while still giving
ReplayLab a browser workflow for CSV upload, parsed-message preview, mock
replay, and result display.

Using the existing CSV parser and mock sender keeps M5 focused on UI
composition rather than new replay semantics. Keeping the app local-only and
limited to short-lived workflow state avoids premature decisions about
authentication, storage, deployment, multi-user behavior, and long-running jobs.

## Consequences

- Add a separate `ReplayLab.Web` project rather than putting UI code in
  `ReplayLab.Core`, `ReplayLab.Cli`, parser projects, or sender projects.
- Reuse public parser, core, replay engine, and mock sender contracts.
- Keep browser upload as the only M5 input path.
- Treat parsed-message preview and mock replay as the first supported Web UI
  workflow.
- Defer HTTP sender exposure in the UI even if the HTTP sender exists in the
  solution.
- Do not add durable persistence, saved history, uploaded file storage,
  database storage, multi-user workflow state, or background processing for M5.

## Alternatives Considered

### React SPA

Rejected for M5. A separate JavaScript frontend would add build, routing, state,
and API-surface decisions before the product has proven a local web workflow.

### Blazor

Rejected for M5. Blazor is a valid .NET UI option, but it introduces component
and hosting choices that are not needed for a minimal local upload-preview-run
workflow.

### Extend The CLI Instead

Rejected for M5 because the milestone is specifically intended to prove a local
browser UI. CLI behavior remains important, but it does not answer the UI
composition question.

## Out Of Scope

- React.
- Blazor.
- Authentication.
- Database or file persistence.
- Docker.
- Deployment or hosted service work.
- HTTP sender UI.
- Configuration DSL.
- WCF.
- Private adapters.
- Broad UI, CLI, core, parser, or sender redesign.

## Guidance For Future Agents

- Do not use M5 to introduce a frontend framework decision beyond Razor Pages.
- Do not expose HTTP sender configuration in the Web UI during M5.
- Do not store uploaded data or replay results beyond the local workflow needed
  to preview and run a mock replay.
- Split implementation into small vertical slices that keep existing CLI,
  parser, replay engine, and sender behavior intact.
