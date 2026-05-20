# M7: Hostable Entry Points

## Goal

Refactor `ReplayLab.Cli` and `ReplayLab.Web` into hostable entry points so a
private project can register its own adapters and parsers through DI and call
ReplayLab entry points without modifying the public repo.

## Status

Complete - M7 delivered the hostable CLI runner, Web hosting hooks, sample
composition proof, and consumption-model docs. See the retrospective at
`docs/retrospectives/m7-hostable-entry-points.md`.

## User Value

- Teams can compose ReplayLab-powered CLI and Web experiences around their own
  private adapters and parsers.
- Private projects can reuse public ReplayLab entry points instead of cloning or
  rewriting the current repo-owned application shells.
- ReplayLab gets a clearer boundary between reusable hostable behavior and
  repo-owned app startup.

## Context From M6

M6 completed the private adapter extension model around `ReplayLab.Core`, DI
registration helpers, `ReplayLab.Adapters.Example`, and packageable
`ReplayLab.Core` at version `0.6.0`.

M6 explicitly kept private projects responsible for their own composition root.
ADR 0008 and PRD 0008 both define hostable CLI and Web entry points as M7 work.
ADR 0009 captures the accepted hostable entry point architecture direction for
M7. M7 therefore starts from a stable public extension seam and focuses on
making the existing CLI and Web experiences reusable without pulling
business-specific composition into the public repo.

## Scope

- Define the hostable architecture for CLI and Web entry points.
- Define the reusable API surface for a hostable CLI runner.
- Define the reusable API surface for hostable Web composition hooks.
- Define ownership of the composition root between ReplayLab and private hosts.
- Define how a private project registers adapters/parsers and calls the hostable
  entry points.
- Document the private consumption model for hostable entry points.
- Keep the runnable `ReplayLab.Cli` and `ReplayLab.Web` shells separate from the
  reusable hostable API surface.
- Keep the hostable model generic and free of business-specific adapter logic.

## Explicit Non-Goals

- Editable Web grid values before replay (`#68`).
- RFC-compliant CSV parser strategy (`#69`).
- Desktop AppHost with WebView2 (`#70`).
- New parser library adoption.
- WebView2 desktop shell work.
- Product UX expansion beyond current CLI/Web workflows.
- Business-specific adapters, mappings, contracts, or composition.
- NuGet publication, installer creation, or release automation.

## Constraints

- `ReplayLab.Core` must remain independent from CLI, Web, adapters, and
  business-specific concerns.
- M6's DI registration pattern remains the baseline. Private projects keep
  registering their own adapters and parsers.
- M7 should expose reusable host seams, not absorb private composition logic.
- CLI and Web hostability should reuse the existing public replay model and
  parser/adapter abstractions where possible.
- The API surface should stay small enough to version and document clearly.
- M7 must not absorb unrelated parser-quality, Web UX, or desktop-shell work.

## Candidate Architecture

ReplayLab should separate app startup shells from reusable hostable behavior.

Candidate direction:

- Introduce `ReplayLab.Cli.Hosting` as the reusable CLI surface. It should own a
  thin `RunAsync(string[] args, TextWriter output, TextWriter error,
  IServiceProvider services, CancellationToken cancellationToken = default)`
  runner and leave DI creation to the caller.
- Introduce `ReplayLab.Web.Hosting` as the reusable Web surface. It should own
  ASP.NET Core composition hooks such as `AddReplayLabWeb` and
  `MapReplayLabWeb`.
- Keep `ReplayLab.Cli` and `ReplayLab.Web` as thin repo-owned shells that prove
  the hostable libraries still run the current public workflows.
- Keep ReplayLab responsible for generic CLI/Web workflow behavior while the
  private project remains responsible for registering private adapters, parsers,
  and any private configuration.
- Keep the composition root boundary explicit: ReplayLab provides hostable entry
  points; the private project chooses how services are assembled.

Likely shape:

```text
ReplayLab.Core
   ^
   |
Parsers / Adapters / Replay Engine
   ^
   |
Hostable CLI / Hostable Web entry points
   ^
   |
Private project composition root
```

## Candidate Vertical Slices

### Slice 1: Decide Hostable Entry Point Architecture

- define the boundary between repo-owned workflow logic and private composition
- define CLI and Web host API shape
- define ownership of service registration and startup
- define the companion library/project boundary for the reusable host surface

### Slice 2: Extract Hostable CLI Entry Point

- refactor current CLI startup into a reusable hostable surface
- preserve current CLI workflow semantics while removing assumptions about a
  repo-owned composition root

### Slice 3: Extract Hostable Web Entry Point

- refactor current Web startup into a reusable hostable surface
- preserve current local Web workflow while removing assumptions about a
  repo-owned composition root

### Slice 4: Add Private Host Composition Sample

- prove that a private project can register its own adapters/parsers and invoke
  the hostable CLI/Web entry points
- keep the sample generic and synthetic

### Slice 5: Document Private Consumption Model

- document how a private project consumes the hostable entry points
- clarify supported ownership boundaries and M7 limitations

## ADR Candidates

- `docs/adr/0009-hostable-entry-points.md` as the accepted hostable entry point
  architecture direction for CLI and Web
- Composition root ownership and DI boundary between ReplayLab and private hosts
- Packaging later, after the hostable API shape is validated

ADR 0009 accepts the architecture direction in this planning phase. Any
follow-up ADR work should stay focused on unresolved API shape, packaging
boundary, or composition questions.

## PRD Impacts

- `docs/prd/0005-cli-experience.md` may need an update later if the CLI becomes
  both a repo-owned executable and a hostable surface.
- `docs/prd/0006-local-web-ui.md` may need a follow-up or successor document
  later if hostable Web usage changes how the Web experience is described.
- `docs/prd/0008-private-adapter-extension-model.md` may need a follow-up or
  successor document later to reflect the shift from M6-owned private
  composition roots to M7 hostable entry point consumption.

Do not create or update PRD files in this planning PR.

## Success Criteria

- A private project can register its own adapters/parsers and invoke a hostable
  CLI entry point without modifying the ReplayLab repo.
- A private project can register its own adapters/parsers and invoke a hostable
  Web entry point without modifying the ReplayLab repo.
- The ownership of the composition root is explicit and documented.
- The hostable API surface is small, generic, and consistent with M6's
  extension-model boundaries.
- CLI/Web hostability does not add business-specific types or private adapter
  assumptions to public ReplayLab projects.
- The reusable hostable surface is documented as companion libraries, not as a
  new packaging/release initiative.
- M7 documentation clearly distinguishes included scope from deferred parser,
  Web UX, and desktop-shell candidates.

## Risks

- The hostable API surface may become too broad if current app startup logic is
  not separated carefully.
- CLI and Web may require different hostability seams, which could make the M7
  abstraction uneven.
- Packaging expectations could expand M7 if the milestone tries to solve
  publication and hostability at the same time.
- Without a clear ADR, composition-root ownership may drift back into ambiguous
  public/private responsibilities.
- Pulling in `#68`, `#69`, or `#70` would blur the milestone and slow the core
  hostability refactor.

## Future Candidates Not Included In M7

- `#68` Editable Web grid values before replay
  - Web UX/product candidate
  - independent from M7
  - not part of M7 unless explicitly promoted later

- `#69` RFC-compliant CSV parser strategy
  - parser-quality candidate
  - independent from M7
  - not part of M7 unless explicitly promoted later

- `#70` Desktop AppHost with WebView2 and self-hosted Web UI
  - desktop/product-shell candidate
  - depends on M7 hostable Web entry points
  - not part of M7 unless explicitly promoted later

## Issue Drafts

### Draft 1: Decide hostable entry point architecture

**Goal:** Define the hostable CLI/Web architecture boundary and draft the ADR
inputs for composition-root ownership.

**Scope:**

- analyze current CLI and Web startup boundaries
- define the reusable API surface for hostable entry points
- define which services ReplayLab owns and which private hosts own

**Acceptance Criteria:**

- hostable architecture boundary is documented
- composition-root ownership is explicit
- CLI and Web hostability assumptions are aligned

### Draft 2: Extract hostable CLI entry point

**Goal:** Refactor the current CLI startup path into a reusable hostable entry
surface.

**Scope:**

- extract repo-owned CLI startup logic into a hostable form
- preserve current generic CLI behavior
- remove assumptions that only ReplayLab owns the CLI composition root

**Acceptance Criteria:**

- private host can invoke CLI behavior through a stable public entry surface
- existing generic CLI workflow behavior remains intact

### Draft 3: Extract hostable Web entry point

**Goal:** Refactor the current Web startup path into a reusable hostable entry
surface.

**Scope:**

- extract repo-owned Web startup logic into a hostable form
- preserve the existing local Web workflow
- remove assumptions that only ReplayLab owns the Web composition root

**Acceptance Criteria:**

- private host can invoke Web behavior through a stable public entry surface
- existing generic Web workflow behavior remains intact

### Draft 4: Add generic private host sample

**Goal:** Prove the private consumption model with a generic host sample.

**Scope:**

- add a generic sample that registers adapters/parsers through DI
- invoke the hostable entry points from a private-style composition root
- keep the sample synthetic and business-agnostic
- demonstrate both CLI and Web consumption through `samples/ReplayLab.HostSample`

**Acceptance Criteria:**

- the sample demonstrates private composition without modifying ReplayLab code
- the sample stays generic and free of business-specific types
- the sample proves CLI DI consumption and Web external hosting/mounting

### Draft 5: Document private consumption model

**Goal:** Document how private projects consume hostable CLI and Web entry
points after M7.

**Scope:**

- describe service-registration expectations
- describe composition-root ownership
- describe M7-supported scenarios and deferred scenarios
- reference `samples/ReplayLab.HostSample` as the practical example

**Delivered M7 consumption model:**

- CLI hosts can register parser and sender services through DI and pass the
  provider to `ReplayLab.Cli.Hosting`.
- Web hosts can mount `ReplayLab.Web.Hosting` through `AddReplayLabWeb()` and
  `MapReplayLabWeb()`.
- The current Web workflow remains hostable, but it still uses its existing
  internal CSV parser and mock sender path; external Web parser/sender DI
  consumption is not part of M7.

**Acceptance Criteria:**

- M7 documentation is clear enough for a private project author to follow
- deferred candidates remain clearly out of scope
