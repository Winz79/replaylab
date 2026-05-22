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

## Playbooks

Agents should use `.ai/playbooks/` before asking for custom instructions or inventing a new workflow.

- Use `discovery-to-slice.md` when the product direction is fuzzy and the team needs one prioritized, implementation-ready slice.
- Use `plan-milestone.md` for milestone planning.
- Use `create-issue-from-plan.md` when turning reviewed issue drafts into GitHub issues.
- Use `implement-issue.md` for implementation.
- Use `review-pr.md` for PR review.
- Use `close-milestone.md` when closing a milestone.

## Routing

Use the smallest route that fits the work.

- Use Product Strategist when the question is "what / why / for whom".
- Use Conductor when the question is "how to split / route / sequence".
- Use Implementer when the task is already scoped.
- Use Reviewer after implementation.
- Use QA for runnable verification.
- Do not invoke Product Strategist for small obvious code, test, or doc changes.

| Situation | Route |
| --- | --- |
| Fuzzy product direction or MVP boundary | Product Strategist + `discovery-to-slice.md` |
| Small, self-contained implementation slice | Single-slice implementation with `.ai/templates/task-contract.md` |
| A few cleanly separable subtasks that can run independently | `delegate_task` / swarm |
| Repo dispatch, coordination, or review queue work | Kanban |
| Durable work item that should survive sessions | GitHub issue |
| Shared files, ordered steps, or unclear boundaries | Do not parallelize |

GitHub issues remain the durable source of truth, but tiny execution slices may use a lightweight task contract instead of opening an issue.

## Architecture Style

ReplayLab starts as a modular toolkit architecture with vertical-slice delivery.

Vertical slices are delivery units. They should guide issues, PRs, implementation order, and review scope; they do not require one physical project per feature.

The source layout remains modular and package-oriented for now:

- `ReplayLab.Core` contains generic contracts and models and must remain independent from adapters, CLI, Web, UI, persistence, Docker, WCF, and business-specific concerns.
- `ReplayLab.Parsers.*` projects contain parser implementations.
- `ReplayLab.Adapters.*` projects contain sender adapter implementations.
- `ReplayLab.Cli` and future `ReplayLab.Web` compose core, parsers, and adapters.

Adapters remain separate projects that depend on core. Core must not depend on adapters.

Do not introduce a full modular monolith structure unless an ADR explicitly changes the architecture direction.

If a slice touches multiple projects, keep the change small, coherent, and reviewable.


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
7. After opening or updating a PR, stay in the loop until CI is green or any remaining failures are clearly out of scope, and address review comments on the same PR with follow-up commits.
8. When done, summarize:
   - files changed
   - tests run
   - assumptions
   - risks
   - follow-up issues
