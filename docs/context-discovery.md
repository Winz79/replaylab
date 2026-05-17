# Context Discovery

## Repository Context

- New repository: `replaylab`.
- Existing starter files: `README.md`, `LICENSE`, `.gitignore`, and `.ai/intial-prompt.md`.
- .NET SDKs available locally: .NET 9 and .NET 10.
- Initial scaffold uses the current default SDK templates and targets `net10.0`.

## Methodology Context

The project follows this local AI-first flow:

1. Intent
2. Context
3. Requirements
4. Impact
5. Design
6. Delivery Plan
7. Build / Review / Learn

The toolkit repository at `../ai-engineering-toolkit` is conceptual guidance only. ReplayLab does not depend on it.

## Assumptions

- The public repository should remain domain-neutral.
- Early documentation should capture boundaries and decisions before adding behavior.
- Local AI instructions should guide future agents to preserve small vertical slices.

## Open Questions

- What license obligations should future adapter packages follow?
- Should non-public adapters live in separate private repositories or separate packages?
- What compatibility target should be preferred if .NET 10 is too new for early adopters?
