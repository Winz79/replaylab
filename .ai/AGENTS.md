# ReplayLab Agent Instructions

## Working Rules

- Inspect the repository before editing.
- Do not code before a short plan exists.
- Work in small vertical slices.
- Keep `ReplayLab.Core` independent from adapters, CLI, UI, persistence, Docker, WCF, and business-specific concerns.
- Do not introduce proprietary, customer-specific, or business-specific assumptions.
- Keep public models and interfaces generic.
- Update ADRs for meaningful architecture decisions.
- Update docs when implementation changes the plan.
- Summarize risks and assumptions after each task.

## Project Boundaries

- Public repo scope: generic replay engine, generic message model, parser direction, mock sender, and future generic adapters.
- Out of public repo scope: WCF, proprietary formats, certificates, customer data, company mappings, and business-specific adapters.

## Preferred Workflow

1. Read relevant docs in `docs/`.
2. Inspect existing projects and tests.
3. Write a short task plan.
4. Make the smallest coherent change.
5. Run focused tests.
6. Update docs or ADRs if the design changes.
7. Report what changed, assumptions, risks, and open questions.
