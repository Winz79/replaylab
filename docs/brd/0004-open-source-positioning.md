# BRD 0004: Open Source Positioning

## Status

Draft

## Positioning

ReplayLab should be open-source friendly by default. The public repository should be understandable, buildable, and testable without private infrastructure, proprietary data, or internal contracts.

## Principles

- Public examples use synthetic data.
- Core concepts remain generic.
- Adapters are isolated from core.
- Private integrations stay outside the public repository.
- Documentation names boundaries clearly instead of hiding them.
- Decisions that affect architecture are captured in ADRs.

## Contributor Expectations

Contributions should:

- Preserve core independence.
- Avoid business-specific assumptions.
- Include focused tests for changed behavior.
- Update PRDs, BRDs, ADRs, or plans when scope or design changes.
- Keep vertical slices small enough to review.

## Release Expectations

Before a first public release, the project should clarify:

- Supported target framework.
- Stable public contracts.
- Minimal supported parser.
- Minimal replay engine behavior.
- CLI or library-first positioning.
- Package naming.

## Assumptions

- Public trust depends on clear boundaries and synthetic examples.
- The project can document private extension patterns without shipping private implementations.

## Open Questions

- What contribution guide should be added before external contributors are invited?
- Should packages be released before CLI usability exists?
- Should the project support multiple target frameworks for adoption?
