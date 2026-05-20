# Future Candidate Milestones

This file collects post-M8 candidate milestone dossiers. These are planning
artifacts, not final commitments. Promote a candidate only after its value,
dependencies, risks, and required artifacts are accepted.

## Candidate M9A: Parser Quality / RFC-Compliant CSV

### Goal

Choose and implement robust CSV parsing behavior for realistic CSV inputs.

### User Value

- Users can load CSV files from common external tools without surprising parser
  failures.
- Parser behavior becomes predictable enough for CLI, Web, samples, and private
  hosts.
- ReplayLab can distinguish supported CSV behavior from malformed input with
  clearer diagnostics.

### Dependency On Previous Work

- Builds on the current minimal CSV parser and PRD 0002 file parsing behavior.
- Independent from M8 for sequencing, but M8 ensures the Web workflow can consume
  whichever parser strategy is accepted.

### Required Artifacts

- ADR candidate: CSV parser strategy.
- PRD update: accepted parser behavior in `docs/prd/0002-file-parsing.md` or a
  successor parser-quality PRD.

### Candidate Slices

- Compare keeping a custom parser with adopting CsvHelper or another proven CSV
  library.
- Define accepted behavior for quoted fields, escaped quotes, embedded commas,
  embedded newlines, duplicate headers, blank lines, comments, encoding, malformed
  rows, errors, and diagnostics.
- Decide whether the parser still emits whole-row JSON payloads using CSV column
  names.
- Implement the accepted parser strategy behind the existing parser contract or a
  documented successor contract.
- Update CLI, Web, samples, and parser tests to reflect accepted behavior.

### Explicit Non-Goals

- Non-CSV parser formats.
- Business-specific mappings or contract transforms.
- Editable Web grid behavior.
- Desktop AppHost.
- Broad parser plugin redesign beyond the accepted CSV strategy.

### Readiness

Directional candidate. It needs an ADR and PRD update before implementation
issues are created.

### Recommended Next Action

Draft the CSV parser strategy ADR with a focused comparison of custom parser,
CsvHelper, and any other credible library option.

## Candidate M9B: Editable Replay Workspace

### Goal

Allow Web users to edit parsed row values before replay.

### User Value

- Users can correct input mistakes or prepare synthetic replay scenarios without
  editing and re-uploading source files.
- The Web UI becomes a replay workspace rather than only a preview surface.
- Edited payloads can support exploratory testing while preserving original input
  context.

### Dependency On Previous Work

- Based on discovery issue `#68`.
- May depend on M8 Web External Composition so edited values replay through the
  same host-provided workflow services as unedited values.
- Benefits from Parser Quality if accepted parsing behavior affects editable cell
  values and diagnostics.

### Required Artifacts

- PRD candidate: Editable Replay Workspace.
- Possible ADR only if the editing model materially changes architecture,
  persistence, or core replay contracts.

### Candidate Slices

- Define original versus edited value semantics.
- Define whether edits mutate `ReplayMessage.Payload`, create a derived payload,
  or stay in Web-only workflow state until replay.
- Define validation rules before replay.
- Define reset behavior for cell, row, and full workspace.
- Define replay payload semantics for selected rows and edited cells.
- Define result-state behavior when rows are edited after replay.
- Define dirty-state and validation feedback in the Web UI.

### Explicit Non-Goals

- Persistence or saved replay sessions.
- Business-specific validation or mappings.
- Parser strategy replacement.
- Desktop AppHost.
- Spreadsheet-scale data modeling or formula support.

### Readiness

Discovery to directional candidate. It needs PRD-light behavior definition before
implementation issues are created.

### Recommended Next Action

Draft the Editable Replay Workspace PRD with tight behavior semantics and a small
first slice.

## Candidate M10: Desktop AppHost / Product Shell

### Goal

Provide a desktop host that runs the ReplayLab Web UI through WebView2 while
self-hosting the Web application from a desktop executable.

### User Value

- Users can launch ReplayLab as a local desktop tool without manually running a
  CLI command or managing a browser-hosted Web app.
- Private hosts can package a ReplayLab-powered local workbench with a clearer
  launch and lifecycle model.
- The product shell can make ReplayLab feel like a tool rather than a developer
  sample.

### Dependency On Previous Work

- Depends on M8 Web External Composition so the desktop shell can reuse hostable
  Web workflow services.
- Benefits from Parser Quality if realistic CSV input is a target workflow.
- Benefits from Editable Replay Workspace if the product shell should emphasize
  interactive local replay preparation.

### Required Artifacts

- PRD candidate: Desktop AppHost / Product Shell.
- ADR candidate: WebView2 + self-hosted Web strategy.
- Optional BRD candidate if product positioning needs clarification before
  choosing Windows-only WebView2 versus broader distribution goals.

### Candidate Slices

- Decide Windows-only WebView2 scope versus cross-platform alternatives.
- Define self-hosted ASP.NET Core lifecycle inside the desktop executable.
- Define dynamic port, fixed port, or alternate local bridge behavior.
- Define local-only security assumptions and browser/WebView access boundaries.
- Define startup, shutdown, error recovery, and log access.
- Define packaging shape, WebView2 runtime expectations, and user launch model.
- Prove a minimal shell using synthetic data and current Web workflow.

### Explicit Non-Goals

- Cloud hosting or multi-user deployment.
- Business-specific adapter implementation.
- Installer ecosystem expansion before launch model is accepted.
- Web UX expansion unrelated to the desktop shell.
- Parser strategy decisions unless required for the selected product slice.

### Readiness

Discovery candidate. It should not become implementation-ready until M8 is
complete and PRD/ADR direction is accepted.

### Recommended Next Action

After M8, draft a Desktop AppHost PRD and ADR brief that decide product shell
scope, hosting lifecycle, local security, and packaging expectations.
