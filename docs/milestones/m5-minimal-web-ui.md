# M5: Minimal Web UI

## Goal

Add a local browser UI for uploading a CSV file, previewing parsed replay
messages, running a mock replay, and seeing replay results.

M5 should prove one small UI composition workflow using existing ReplayLab
building blocks. It should not expand sender scope, persistence, hosting,
configuration, or private-adapter boundaries.

## Planning Inputs

- Roadmap: `docs/roadmap.md` marks M5 as the Minimal Web UI candidate after
  CLI and sender concepts have started to settle.
- PRD: `docs/prd/0006-local-web-ui.md` defines the broader future Web UI
  direction.
- Parser requirements: `docs/prd/0002-file-parsing.md` keeps CSV parsing
  generic and synthetic.
- Replay requirements: `docs/prd/0003-replay-engine.md` keeps replay transport
  independent and result-oriented.
- CLI context: `docs/prd/0005-cli-experience.md` and prior milestones keep the
  CLI as the proven local workflow, with Web UI optional and separate.
- Architecture decisions:
  - `docs/adr/0002-separate-core-from-adapters.md`
  - `docs/adr/0004-architecture-style.md`
  - `docs/adr/0007-m5-web-ui-architecture.md`
- Prior milestone learning:
  - `docs/retrospectives/m2-local-executable-distribution.md` warns against
    excessive process for small milestones.
  - `docs/retrospectives/m3-configurable-replay-inputs.md` reinforces keeping
    scope narrow and public-repo-safe.

## PRD-Light Summary

### Users

- Developers who want to inspect synthetic replay input visually.
- Maintainers validating sample CSV behavior without reading CLI summaries.
- Contributors checking parser and mock replay behavior through a local browser.

### Outcomes

- A user can open a local Razor Pages app.
- A user can upload a CSV file from the browser.
- The UI shows parsed message IDs and payload previews.
- The UI can run the existing mock replay flow.
- The UI shows per-message replay results.

### Non-Goals

- React, Blazor, or separate frontend application architecture.
- Authentication, user accounts, or authorization.
- Database, file, or replay-history persistence.
- Docker, deployment, hosted service, or packaging changes.
- HTTP sender UI.
- Config DSL, WCF, private adapters, or business-specific mappings.
- Broad redesign of core, parser, sender, or CLI behavior.

### Constraints

- Web UI project path is `src/ReplayLab.Web`.
- Web stack is ASP.NET Core Razor Pages.
- The app is local-only.
- Browser CSV upload is the only M5 input flow.
- Use the existing CSV parser.
- Use the existing mock sender.
- Keep uploaded CSV contents, parsed messages, and replay results in short-lived
  local workflow state only when needed for the upload → preview → replay flow.
- Do not add durable persistence, database storage, uploaded file storage,
  saved replay history, background processing, or multi-user workflow state.
- Keep product behavior outside the Web UI unchanged.

### Success Criteria

- `dotnet test ReplayLab.sln` passes after M5 implementation.
- Existing CLI, parser, core, mock sender, and HTTP sender tests still pass.
- The Web UI can upload `samples/basic.csv` or equivalent synthetic CSV data.
- The preview page shows parsed messages before replay.
- The replay action uses the mock sender and shows per-message results.
- No out-of-scope technology or persistence surface is introduced.

## ADR Candidates

Resolved first:

- `docs/adr/0007-m5-web-ui-architecture.md` records that M5 Web UI is
  `src/ReplayLab.Web`, ASP.NET Core Razor Pages, local-only, browser CSV
  upload, existing CSV parser, existing mock sender, and no persistence.

No additional M5 ADRs are planned unless implementation reveals a durable
architecture decision that cannot be handled inside an implementation issue.

Non-candidates for M5:

- React versus Blazor beyond the accepted Razor Pages decision.
- Web authentication.
- Persistence strategy.
- Hosted deployment strategy.
- HTTP sender UI architecture.
- Configuration DSL.
- WCF or private adapter extension model.

## Vertical Slices

### Slice 1: Add Local Razor Pages Web App Shell

Create the separate local web app and prove it can be built and tested without
changing product behavior elsewhere.

### Slice 2: Upload CSV And Preview Parsed Messages

Add browser CSV upload and preview parsed messages using the existing CSV
parser.

### Slice 3: Run Mock Replay From The UI And Show Results

Replay the uploaded/parsed messages through the existing mock sender and render
per-message results.

## Issue Drafts

### Draft 1: Add local Razor Pages web app shell

**Goal:** Add `src/ReplayLab.Web` as a local ASP.NET Core Razor Pages app shell
without changing existing product behavior.

**Scope:**

- Create `src/ReplayLab.Web`.
- Add the project to `ReplayLab.sln`.
- Configure a minimal Razor Pages app.
- Add a simple home page that states the Web UI is local-only.
- Reference only the ReplayLab projects needed by later M5 slices.
- Keep the shell free of upload, parsing, replay, persistence, authentication,
  deployment, or sender-configuration behavior.

**Acceptance Criteria:**

- The solution builds with the new Web project.
- The app starts locally with a Razor Pages home page.
- Existing CLI, core, parser, and sender behavior is unchanged.
- The page clearly positions the app as a local-only Web UI.
- No React, Blazor, auth, DB, Docker, deployment, HTTP sender UI, config DSL,
  WCF, private adapters, or broad redesign is introduced.

**Linked Docs or ADRs:**

- `docs/adr/0007-m5-web-ui-architecture.md`
- `docs/prd/0006-local-web-ui.md`
- `docs/roadmap.md`
- `docs/milestones/m5-minimal-web-ui.md`

**Implementation Notes:**

- Use ASP.NET Core Razor Pages.
- Keep project boundaries separate from `ReplayLab.Core`, `ReplayLab.Cli`,
  parser projects, and sender projects.
- Prefer the smallest default local app setup that supports later upload and
  replay pages.

**Test Expectations:**

- Add focused Web project smoke coverage if the repository has a suitable test
  pattern by implementation time.
- Run `dotnet build ReplayLab.sln`.
- Run `dotnet test ReplayLab.sln`.

**Risks:**

- The app shell could grow into UI design work before the workflow exists.
- New project setup could accidentally add hosting, deployment, or frontend
  dependencies that M5 does not need.

**Out Of Scope:**

- CSV upload.
- Parsed-message preview.
- Mock replay.
- HTTP sender UI.
- Persistence.
- Authentication.
- React or Blazor.
- Docker or deployment.

### Draft 2: Upload CSV and preview parsed messages

**Goal:** Let a local browser user upload a CSV file and preview the messages
parsed by the existing CSV parser.

**Scope:**

- Add a Razor Pages upload flow for CSV files.
- Parse uploaded CSV content with `CsvReplayMessageParser`.
- Show parsed message IDs and payload previews.
- Show clear validation or parse errors in the page.
- Keep uploaded content in short-lived local workflow state only as needed for
  the upload → preview flow.
- Do not persist uploaded files or parsed messages to disk or a database.
- Use synthetic CSV examples in tests and documentation references.

**Acceptance Criteria:**

- A user can upload a valid CSV file from the browser.
- Parsed messages are shown before replay.
- Message IDs and payloads match existing CSV parser behavior.
- Invalid CSV input shows a clear page-level error.
- Uploaded files and parsed messages are not persisted to disk or a database.
- No sender execution happens in this slice.

**Linked Docs or ADRs:**

- `docs/adr/0007-m5-web-ui-architecture.md`
- `docs/prd/0002-file-parsing.md`
- `docs/prd/0006-local-web-ui.md`
- `docs/milestones/m5-minimal-web-ui.md`

**Implementation Notes:**

- Reuse the existing parser instead of adding a web-specific CSV parser.
- Keep preview rendering simple: message ID plus readable payload text is
  enough for M5.
- Avoid filtering, selection, pagination, export, or saved history unless a
  later issue explicitly scopes them.

**Test Expectations:**

- Add Web-level tests for valid CSV upload, invalid CSV upload, and preview
  rendering if the chosen test setup supports it.
- Keep existing parser tests as the source of truth for detailed CSV behavior.
- Run focused Web tests, then `dotnet test ReplayLab.sln`.

**Risks:**

- Browser upload handling could drift into persistence or file-management
  behavior.
- Preview UI could start implying filtering or selection commitments that are
  not in M5.

**Out Of Scope:**

- Replay execution.
- HTTP sender UI.
- Filtering or selection.
- Export.
- Persistence.
- Auth.
- Alternate input formats.
- Config DSL.

### Draft 3: Run mock replay from the UI and show results

**Goal:** Let a local browser user run the previewed messages through the
existing mock replay flow and see per-message results.

**Scope:**

- Compose the existing `SequentialReplayEngine` with the existing
  `MockReplaySender` from the Web UI.
- Run replay only for messages produced by the browser CSV upload flow.
- Show per-message replay status and message IDs.
- Show a concise success/failure summary.
- Keep replay results in short-lived local workflow state only as needed to
  render the result page.
- Do not persist replay results, saved history, or background job state.

**Acceptance Criteria:**

- A user can run mock replay after uploading and previewing a CSV file.
- The UI shows one result per parsed message.
- Result status and message ID match existing replay engine and mock sender
  behavior.
- Replay failures, if produced by existing contracts, are visible in the page.
- Existing CLI mock replay behavior remains unchanged.
- The UI does not expose HTTP sender configuration.

**Linked Docs or ADRs:**

- `docs/adr/0007-m5-web-ui-architecture.md`
- `docs/prd/0003-replay-engine.md`
- `docs/prd/0006-local-web-ui.md`
- `docs/milestones/m5-minimal-web-ui.md`

**Implementation Notes:**

- Use the existing mock sender rather than adding a web-specific sender.
- Keep replay orchestration in the Web project composition layer.
- Do not add background jobs, saved history, scheduling, or persistence.
- Do not surface HTTP sender options in M5.

**Test Expectations:**

- Add Web-level tests proving mock replay renders per-message results.
- Keep core replay engine tests responsible for detailed replay semantics.
- Run focused Web tests, then `dotnet test ReplayLab.sln`.

**Risks:**

- UI replay could duplicate CLI orchestration instead of composing existing
  contracts cleanly.
- Result rendering could tempt broader result-model redesign that is not needed
  for mock replay.

**Out Of Scope:**

- HTTP sender UI.
- Sender selection.
- Persistence.
- Background processing.
- Scheduling.
- Filtering or selection.
- Export.
- Auth.
- WCF or private adapters.

## Recommended Sequence

1. Implement the local Razor Pages shell.
2. Add CSV upload and parsed-message preview.
3. Add mock replay execution and result rendering.

Do not start HTTP sender UI, persistence, authentication, deployment, or
frontend-framework work during M5.
