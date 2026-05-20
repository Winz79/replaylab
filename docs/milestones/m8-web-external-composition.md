# M8: Web External Composition

## Status

Proposed / recommended next milestone.

This is a planning artifact, not an accepted implementation commitment until the
milestone is explicitly approved.

## Goal

Make `ReplayLab.Web.Hosting` consume host-provided parser, sender, and workflow
services through DI where appropriate, while preserving the current Web behavior
through safe defaults.

## User Value

- Private hosts can reuse the ReplayLab Web workflow with their own parsers and
  senders instead of only mounting the public Web shell.
- The Web hostability story becomes symmetric with CLI hostability.
- Future Desktop AppHost work can reuse a stronger Web composition seam.
- Existing local Web users keep the current CSV/mock workflow unless they choose
  to override it.

## Context From M7

M7 delivered:

- `ReplayLab.Cli.Hosting`
- `ReplayLab.Web.Hosting`
- external host sample
- consumption model docs

The M7 closeout identified one important limitation: CLI hostability consumes
external parser and sender services through DI, but the Web workflow still uses
the internal CSV parser and mock sender path. `AddReplayLabWeb()` and
`MapReplayLabWeb()` make the Web app mountable, but not yet externally
configurable for workflow services.

## Scope

- Define the Web workflow DI composition boundary.
- Make the Web workflow resolve an `IMessageParser` or equivalent parser service
  from host-owned DI.
- Make the Web workflow resolve an `IReplaySender`, sender factory, or equivalent
  replay service from host-owned DI.
- Preserve current Web CSV/mock behavior through safe default registrations.
- Add a sample or focused test proving external Web parser/sender composition.
- Update docs for supported Web external composition.
- Keep `ReplayLab.Web` as a runnable public shell and `ReplayLab.Web.Hosting` as
  the reusable host surface.

## Explicit Non-Goals

- Desktop AppHost / WebView2.
- RFC-compliant CSV parser strategy.
- Editable Web grid behavior.
- Business-specific adapters, mappings, contracts, or payload examples.
- Package publishing or release automation.
- Authentication, persistence, hosted deployment, or multi-user workflow state.
- Replacing Razor Pages or redesigning the Web UI.

## Candidate Architecture

M8 should extend the M7 host boundary rather than inventing a second composition
model.

Candidate direction:

- `AddReplayLabWeb()` registers Web UI services and safe defaults only when the
  host has not already registered an equivalent service.
- The Web upload and replay workflow resolves parser and sender dependencies from
  DI instead of constructing or selecting internal concrete implementations
  directly in page models.
- The public `ReplayLab.Web` shell registers the same default CSV parser and mock
  sender behavior users have today.
- Private hosts can override parser and sender registrations before or after
  `AddReplayLabWeb()` according to the accepted DI ordering rule.
- Workflow-specific orchestration may move behind a small Web workflow service if
  that keeps page models thin and makes host-provided services easier to test.

Likely shape:

```text
Private host composition root
  -> host registers parser/sender/workflow services
  -> AddReplayLabWeb() fills safe defaults
  -> MapReplayLabWeb() maps pages/endpoints
  -> Web workflow resolves services from DI
```

## Open Decisions

- Should `AddReplayLabWeb()` use `TryAdd*` defaults, explicit options, or both?
- Is a direct `IReplaySender` enough for Web replay, or does the Web workflow need
  a sender factory/selector to support future sender choices?
- Should parser and sender overrides be scoped, transient, or singleton in the
  default Web composition?
- How should the Web workflow report missing or invalid host registrations?
- Should the sample prove external composition through `samples/ReplayLab.HostSample`
  or through a smaller Web hosting test fixture?

## ADR Need

Create an ADR only if the accepted M8 direction changes the durable Web
composition boundary.

ADR candidate:

- Title: ADR 0010: Web Workflow DI Composition Boundary
- Purpose: define how `ReplayLab.Web.Hosting` consumes host-owned parser, sender,
  and workflow services while preserving safe defaults.
- Decision to be made: whether Web workflow dependencies are resolved directly
  from DI, through a workflow service, through a sender factory, or through an
  options-based composition model.
- When to create it: before implementation begins if the team needs a durable
  boundary decision; otherwise keep this milestone plan as the planning source.

## PRD Impact

No PRD is required if M8 preserves visible Web behavior and only changes
composition. Update or create a PRD only if M8 changes accepted Web behavior,
visible sender selection, parser selection UX, error presentation, or replay
semantics.

## Candidate Vertical Slices

### Slice 1: Define Web Workflow DI Composition Boundary

- settle default registration behavior
- decide direct sender versus sender factory
- decide error behavior for missing host services
- capture ADR 0010 if needed

### Slice 2: Introduce DI-Resolved Parser Service For Web Workflow

- remove direct parser construction from Web workflow code
- resolve parser dependency from DI
- preserve current CSV parser default

### Slice 3: Introduce DI-Resolved Replay Sender Or Sender Factory

- remove direct mock sender path from Web replay workflow
- resolve sender dependency from DI
- preserve current mock sender default

### Slice 4: Preserve Current Web Defaults

- ensure the public `ReplayLab.Web` shell still works without host customization
- document registration order and override behavior
- cover default CSV/mock behavior with focused tests where practical

### Slice 5: Prove External Web Parser/Sender Composition

- add a synthetic external composition proof using a sample or integration test
- avoid business-specific adapters or payloads
- prove host-owned services are used by the Web workflow

### Slice 6: Update Documentation

- update host consumption docs
- update M8 closeout notes when complete
- keep limitations and deferred scope explicit

## Draft Implementation Issues

Do not create GitHub issues from this plan until M8 is accepted. Draft titles and
scopes:

### M8: Define Web workflow DI composition boundary

Scope: decide default registration, override ordering, direct sender versus
factory, and whether ADR 0010 is required.

### M8: Extract Web parser service from PageModel

Scope: make Web upload/parse workflow resolve parser behavior from DI while
preserving current CSV defaults.

### M8: Resolve replay sender/factory from Web DI

Scope: make Web replay workflow consume host-provided sender behavior while
preserving current mock defaults.

### M8: Preserve default Web CSV/mock behavior

Scope: verify the public Web shell remains runnable and behavior-compatible when
no external host overrides are provided.

### M8: Prove external Web parser/sender composition

Scope: add a synthetic sample or focused test showing Web workflow uses
host-provided parser and sender services.

### M8: Document Web external composition model

Scope: document supported host registration, override behavior, defaults, and
known limitations.

## Success Criteria

- A private Web host can register its own parser service and have the Web
  workflow consume it.
- A private Web host can register its own sender or sender factory and have the
  Web replay workflow consume it.
- The public Web shell still supports the current CSV upload, preview, and mock
  replay workflow with no external configuration.
- Web composition rules are documented, including defaults and override order.
- The solution includes a synthetic proof that external Web parser/sender
  composition works.
- M8 does not implement Desktop AppHost, parser strategy changes, editable grid
  behavior, package publishing, or business-specific adapters.

## Risks

- The Web workflow may need a small orchestration service to avoid pushing DI
  complexity into Razor PageModels.
- Sender selection could expand beyond M8 if factory and UI concerns are mixed.
- Default registration order could surprise private hosts if not documented and
  tested.
- Trying to solve parser quality, editable workspace, or desktop packaging inside
  M8 would blur the milestone.

## Deferred Scope

- Parser Quality / RFC-compliant CSV remains a future candidate.
- Editable Replay Workspace remains a future product/UX candidate.
- Desktop AppHost remains a later product shell candidate.
- Package publishing and release automation remain separate from Web external
  composition.
- Full PRD and ADR work is deferred unless the composition boundary decision
  requires ADR 0010 before implementation.
