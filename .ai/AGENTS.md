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

## GitHub Issues

GitHub Issues are executable work items derived from planning artifacts.

Issues should normally come from:

- vertical slice plans
- architecture briefs
- ADR follow-ups
- review findings
- open questions that became actionable

### Planner Behavior

The planner may propose GitHub issues from planning artifacts.

By default, the planner should generate issue drafts, not create issues directly.

The planner may create actual GitHub issues only when explicitly asked.

Issue drafts must include:

- title
- goal
- scope
- acceptance criteria
- linked docs / ADRs
- implementation notes
- test expectations
- risks
- out of scope

When drafting issues:

- keep them small and slice-oriented
- avoid broad "build everything" issues
- split large issues before implementation
- keep issues linked to the relevant planning artifact

### Implementer Behavior

When working from an issue:

1. Read the issue description.
2. Identify linked docs, ADRs, or planning artifacts.
3. Inspect the repository before editing.
4. Restate the intended slice before implementation.
5. Keep the change scoped to the issue.
6. If the issue is too large, propose a split instead of implementing everything.
7. When done, summarize:
   - files changed
   - tests run
   - assumptions
   - risks
   - follow-up issues
