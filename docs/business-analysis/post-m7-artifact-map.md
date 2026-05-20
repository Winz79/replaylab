# Post-M7 Artifact Map

This map shows which planning artifacts should be created for the post-M7 roadmap
and when to create them. It avoids creating PRDs, ADRs, or issues before the
candidate work is ready.

## M8 Web External Composition

### Create

- ADR 0010 candidate: Web Workflow DI Composition Boundary.
- Possible PRD update only if visible Web behavior changes.

### Why

M8 changes the Web hostability seam from structural mounting to workflow service
composition. If the implementation direction changes the durable boundary for
parser, sender, factory, default registration, or workflow orchestration, an ADR
should capture it before implementation begins.

### When To Create It

- Create ADR 0010 before implementation if the team needs to decide direct DI
  resolution versus workflow service/factory/options composition.
- Skip a PRD update if the Web UI behaves the same and only composition changes.
- Create or update a PRD only if users see new parser/sender selection behavior,
  changed replay semantics, or changed error presentation.

### Do Not Create Yet

- Implementation issues until M8 scope is accepted.
- Full PRD if visible Web behavior remains unchanged.
- Desktop AppHost ADR or PRD as part of M8.

## M9A Parser Quality / RFC-Compliant CSV

### Create

- ADR candidate: CSV parser strategy.
- PRD update: accepted parser behavior.

### Why

Parser strategy is both a durable technical choice and a product behavior choice.
The project needs to decide custom parser versus CsvHelper or another library,
then define accepted CSV behavior for realistic files.

### When To Create It

- Create the ADR before implementation to choose the parser strategy.
- Update PRD 0002 or create a successor parser PRD before implementation to
  define accepted behavior for quoted fields, embedded commas, embedded newlines,
  encoding, errors, and diagnostics.

### Do Not Create Yet

- Implementation issues before the ADR and parser behavior are accepted.
- Non-CSV parser PRDs.
- Business-specific mapping docs.

## M9B Editable Replay Workspace

### Create

- PRD candidate: Editable Replay Workspace.
- Possible ADR only if architecture changes materially.

### Why

Editing parsed values changes user workflow and replay semantics. The team must
define original versus edited values, validation, reset, replay payload behavior,
result state, and UX feedback before implementation.

### When To Create It

- Create the PRD before implementation issues are drafted.
- Create an ADR only if the accepted editing model changes core replay contracts,
  persistence expectations, or Web architecture boundaries.

### Do Not Create Yet

- Implementation issues from discovery issue `#68` before PRD behavior is clear.
- Persistence or saved-session design docs unless explicitly promoted.
- Business-specific validation artifacts.

## M10 Desktop AppHost / Product Shell

### Create

- PRD candidate: Desktop AppHost / Product Shell.
- ADR candidate: WebView2 + self-hosted Web strategy.
- Optional BRD candidate if product positioning needs clarification.

### Why

Desktop AppHost affects product shape, distribution, host lifecycle, local
security, packaging, and Web hosting architecture. It should not be treated as a
small implementation detail after M7.

### When To Create It

- Create the PRD after M8 proves Web External Composition and before desktop
  implementation issues are drafted.
- Create the ADR before choosing WebView2/self-hosting lifecycle, port strategy,
  WebView runtime expectations, and local-only security posture.
- Create the optional BRD only if the team needs to decide whether ReplayLab is a
  developer toolkit, local replay workbench, desktop product shell, or some blend
  before investing in Desktop AppHost.

### Do Not Create Yet

- Desktop implementation issues before M8 is complete and PRD/ADR direction is
  accepted.
- Installer or publishing plans before the launch model is chosen.
- Cross-platform shell plans unless product positioning requires them.

## General Issue Timing Rule

Create GitHub implementation issues only after a candidate milestone is accepted
and its required artifacts are clear. Discovery issues may remain open as
decision containers until they are promoted, split, or closed by accepted
planning work.
