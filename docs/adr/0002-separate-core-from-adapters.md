# ADR 0002: Separate Core From Adapters

## Status

Accepted

## Context

ReplayLab must stay generic and open-source friendly. Sender implementations may target different systems, but the core project should not inherit adapter-specific assumptions.

## Decision

Keep `ReplayLab.Core` limited to generic models and contracts. Put sender implementations in separate adapter projects, starting with `ReplayLab.Adapters.Mock`.

WCF, proprietary, customer-specific, certificate-specific, and business-specific adapters must live outside the public repository.

## Consequences

- Core remains small and easier to review.
- Public contracts can be tested without external systems.
- Adapter packages can evolve independently.
- Future composition belongs in applications such as CLI or UI, not in core contracts.

## Assumptions

- Adapters will depend on core, never the reverse.
- The mock adapter is enough to validate the dependency direction in the scaffold.

## Open Questions

- Should future official adapters live in this repository or separate repositories?
- How should adapter capability metadata be exposed?
