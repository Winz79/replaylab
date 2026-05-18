# ADR 0004: Architecture Style

## Status

Accepted

## Context

ReplayLab is a small .NET solution with core contracts, parser projects, sender adapter projects, a CLI placeholder, and tests.

The project uses vertical slices to plan and review work, but the repository also needs package-friendly reusable projects. Without an explicit decision, future work could confuse vertical slices with physical source modules or introduce a strict modular monolith structure too early.

## Decision

ReplayLab will start as a modular toolkit architecture with vertical-slice delivery.

Reusable projects should remain package-friendly:

- `ReplayLab.Core`
- `ReplayLab.Parsers.*`
- `ReplayLab.Adapters.*`
- `ReplayLab.Cli`
- future `ReplayLab.Web`

Vertical slices are primarily planning and delivery units. They should guide issues, PRs, implementation order, and review scope. They do not require one physical project per feature.

ReplayLab will not start as a strict modular monolith. If the application layer becomes complex later, CLI or Web projects may evolve toward capability modules, but that change requires a future ADR.

## Rationale

ReplayLab's public value is reusable toolkit components. A package-oriented layout keeps core contracts, parser implementations, adapters, and application hosts independently understandable and easier to package later.

Vertical-slice delivery still keeps work focused. A slice may touch core, parser, adapter, CLI, and tests when needed, but it should remain small enough to review as one coherent behavior.

## Consequences

- Core remains independent from adapters, applications, UI, persistence, Docker, WCF, and business-specific concerns.
- Parsers and adapters remain separate projects that depend on core.
- Delivery planning should stay slice-oriented even when code changes cross project boundaries.
- New projects should be added for package or host boundaries, not automatically for every feature.
- A future modular monolith structure for CLI or Web requires a separate ADR.

## Alternatives Considered

### Strict Modular Monolith

ReplayLab could organize around application modules with explicit module boundaries and feature ownership. This may be useful later for a complex app, but it is too heavy for the current toolkit foundation and could make package boundaries less clear.

### Layered Technical Architecture Only

ReplayLab could organize only by technical layers such as core, infrastructure, and presentation. This would be simple, but it would not give enough guidance for issue scope, PR review, or incremental delivery.

### Modular Toolkit Architecture With Vertical-Slice Delivery

ReplayLab can keep reusable projects package-friendly while using vertical slices for planning and delivery. This matches the current repository shape and keeps future work small, reviewable, and open-source friendly.

## Guidance for Future Agents

- Treat vertical slices as delivery units, not automatic project boundaries.
- Keep source layout modular and package-oriented unless an ADR changes that direction.
- Keep `ReplayLab.Core` generic and independent.
- Keep parser and sender adapters separate from core and from each other.
- Do not introduce a full modular monolith structure without a future ADR.
- When a slice touches multiple projects, keep the change small, coherent, and covered by focused tests.
