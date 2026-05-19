# M5 Retrospective - Minimal Web UI

## What Shipped

M5 added a local browser UI for uploading CSV files, previewing parsed messages, running mock replay, and displaying results. The implementation used ASP.NET Core Razor Pages in `ReplayLab.Web`.

Key deliverables:
- Local Razor Pages web app shell in `src/ReplayLab.Web`
- Browser CSV upload with parsed message preview
- Mock replay execution from the UI with per-message results
- Table-based workflow (replaced initial card-based approach)

## What Worked

- Razor Pages was the right choice for a minimal local UI - lightweight and no separate frontend stack needed
- Reusing existing CSV parser and mock sender kept M5 focused on UI composition
- Small vertical slices kept the milestone controllable, but M5 also showed that UI slices need explicit UX acceptance criteria early. Functional acceptance alone was not enough.
- Replacing card-based UI with Tabulator grid workflow improved usability significantly
- Keeping Web project separate from Core, CLI, and adapters preserved clean boundaries
- Short-lived workflow state only (no persistence) was the correct constraint for M5

## What Changed From Plan

- Card-based UI was replaced with a data table workflow (Tabulator.js) based on follow-up issue #50
- The initial card approach was deemed less practical for message inspection
- M5 did not expose HTTP sender in the UI - stayed mock-sender-only as planned

## Decisions Made

- Use Tabulator.js for data table rendering in the Web UI
- Keep `ReplayLab.Web` as a separate project with dependency on Core, parsers, and adapters
- No persistence - short-lived workflow state only
- Browser CSV upload as the only M5 input path
- Defer HTTP sender UI to future milestone (not M5)

## Risks That Remain

- The Web app is now a second entry point alongside the CLI, so distribution and local execution expectations need to stay aligned.
- HTTP sender support is still CLI-only; exposing it in the Web UI will require explicit sender configuration UX.
- The table workflow is more usable than the initial card design, but richer data interactions such as filtering, sorting, keyboard selection, and sender-parameter editing remain follow-up work.
- Packaging the Web app for distribution is still TBD and should not be mixed into feature work accidentally.

## What We Learned

- A browser UI is not just a visual wrapper around CLI output. It needs its own interaction model.
- Message inspection is naturally table-oriented, not card-oriented.
- For replay workflows, row selection and per-row feedback are core UX concepts, not polish.
- Keeping state short-lived was correct, but forcing request-only state would have made the workflow awkward.
- UI work can quickly pull the project toward product decisions, so explicit scope boundaries remain necessary.

## Follow-Ups

- M4 HTTP sender CLI work does not conflict with M5 scope
- Future work could add HTTP sender to Web UI once M4 sender is stable
- Consider richer table interactions if users need them for large CSV files: advanced filtering, keyboard selection, sender-parameter editing, and result export.
- Web executable distribution may need its own planning (separate from CLI distribution)

## Process Notes

- M5 closed with 4 issues and 3 PRs merged
- The milestone closure workflow was exercised successfully.

## Next Milestone Recommendation

M6 should focus on documenting the private adapter extension model. This is the final roadmap milestone and should remain documentation-focused rather than adding implementation. M6 should clarify how teams can extend ReplayLab with private adapters outside the public repository without adding business-specific code to the public repo.