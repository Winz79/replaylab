# Post-M7 Product Direction

## Current Product Position After M7

ReplayLab now has a usable local replay foundation and a clearer extension path:

- core replay model exists
- CLI replay path exists
- local Web UI exists
- private adapter extension model exists
- hostable CLI and Web entry points exist
- external host sample exists
- CLI hostability consumes external parser and sender services through DI
- Web is hostable through `AddReplayLabWeb()` and `MapReplayLabWeb()`
- Web workflow still uses the internal CSV parser and mock sender path
- external Web parser/sender DI consumption is deferred

M7 completed the main hostability milestone, but it also exposed an asymmetry:
CLI composition is externally useful today, while Web hosting is structurally
hostable but not yet externally configurable for parser/sender workflow services.

## Product Direction

The near-term direction should keep ReplayLab focused as a local replay workbench
for developers who need to inspect, adjust, and replay messages without moving
business-specific adapters or data into the public repo.

The next product steps should strengthen the seams that make that workbench
credible:

- complete Web composition so hostability is not only structural
- improve parser trustworthiness before broader input expectations grow
- add Web editing only after value and semantics are clear
- defer Desktop AppHost until the Web composition seam is strong enough to reuse

## Candidate Tracks

| Track | Classification | Value | Readiness |
| --- | --- | --- | --- |
| Web External Composition | Platform / architecture | Completes the hostability story and closes the M7 Web asymmetry. | Directional candidate, close to shapable milestone. |
| Parser Quality / RFC-compliant CSV (`#69`) | Parser quality / technical | Makes ReplayLab credible with realistic CSV files and reduces parser surprises. | Directional candidate; needs parser strategy decision. |
| Editable Replay Workspace / Web Grid Editing (`#68`) | Product / UX | Lets users correct or prepare replay payloads before replay. | Discovery to directional candidate; needs PRD-light behavior definition. |
| Desktop AppHost / Product Shell (`#70`) | Product / distribution / platform | Improves launchability and packaged local-user experience. | Discovery candidate; depends on stronger Web hostability. |

## Dependency Analysis

- Web External Composition depends on the M7 hostable Web surface and should
  refine the `ReplayLab.Web.Hosting` composition boundary.
- Parser Quality is mostly independent of Web External Composition, but its
  chosen parser contract should be usable from CLI, Web, and private hosts.
- Editable Replay Workspace depends on Web UI behavior decisions and may be
  easier to implement cleanly after Web workflow services are externally
  composable.
- Desktop AppHost depends on M7 Web hostability and likely benefits from Web
  External Composition before desktop hosting choices are locked in.

## Prioritization

Recommended order is dependency-aware but value-oriented:

1. Web External Composition, because it completes the hostability story after M7
   and unblocks stronger Desktop/AppHost usage.
2. Parser Quality or Editable Replay Workspace, depending on whether near-term
   value should prioritize robust real-world inputs or visible Web workflow UX.
3. Desktop AppHost later, after Web hostability and composition seams are
   stronger.

Parser Quality has strong technical leverage and may reduce future UX surprises.
Editable Replay Workspace has stronger visible product value but needs clearer
editing semantics before implementation.

## Recommended Next Milestone Candidates

These are candidate milestones, not final commitments:

- Candidate M8: Web External Composition
  - Goal: make `ReplayLab.Web.Hosting` consume parser/sender/workflow services
    from DI where the Web workflow requires them.
  - Rationale: closes the M7 limitation and strengthens the public/private host
    boundary.
  - Likely artifact need: ADR update or small ADR if the Web composition boundary
    changes.

- Candidate M9: Parser Quality / RFC-compliant CSV or Editable Replay Workspace
  - Parser Quality should be selected first if input robustness is the priority.
  - Editable Replay Workspace should be selected first if Web workflow value and
    user-visible replay preparation are the priority.

- Candidate later milestone: Desktop AppHost / Product Shell
  - Should wait until Web hostability and service composition are proven enough
    for a desktop shell to reuse confidently.

## Required BRD/PRD/ADR Outputs

Do not create these artifacts until the related candidate is promoted or the
decision is needed:

- ADR candidate: Web workflow DI composition boundary.
- ADR candidate: CSV parser strategy.
- PRD candidate: Editable Replay Workspace.
- PRD/ADR candidate: Desktop AppHost.
- Optional BRD candidate: ReplayLab Product Direction / Local replay workbench
  positioning, if product positioning needs a broader decision before selecting
  between parser robustness, Web editing, and desktop packaging.

## Deferred Scope

- No implementation of `#68`, `#69`, or `#70`.
- No implementation issues from this analysis.
- No full PRDs, ADRs, or BRDs from this analysis.
- No package publishing or release automation.
- No business-specific adapters, mappings, or private payload examples.
- No Desktop AppHost implementation before Web composition is stronger.

## Recommended Next Action

Promote Web External Composition to the next candidate milestone discussion.
Start with a small ADR update or ADR candidate that defines whether the Web
workflow should resolve parser, sender, and replay services from host-owned DI,
then shape a milestone plan only after that boundary is accepted.
