# Business/Product Analysis Playbook

## Purpose

Use this playbook to turn discovery inputs into clear product direction without
starting implementation too early. The output should help maintainers decide
what deserves a BRD, PRD, ADR, milestone plan, or implementation issue.

Business analysis is not implementation planning. Keep the work concise,
evidence-based, and reversible until a milestone is accepted.

## Inputs

Collect only the inputs needed to understand product direction:

- discovery issues and open questions
- `docs/roadmap.md`
- BRDs, if present
- PRDs in `docs/prd/`
- ADRs in `docs/adr/`
- milestone plans in `docs/milestones/`
- retrospectives in `docs/retrospectives/`
- recent closeout notes, release notes, or merged PR summaries

For each input, capture the current state, the unresolved decision, and whether
it is accepted scope, discovery scope, or historical context.

## Analysis Steps

1. Confirm the current repository and milestone state.
2. Read the roadmap first to understand accepted direction.
3. Review PRDs and ADRs for existing product and architecture commitments.
4. Review milestone plans and retrospectives for shipped scope, deferred work,
   and lessons learned.
5. Review discovery issues for candidate work and unresolved questions.
6. Classify each candidate into a track.
7. Assess value, dependency, risk, and readiness.
8. Decide which artifact, if any, is needed next.
9. Propose candidate milestones without freezing numbering prematurely.
10. Update the roadmap with accepted status and candidate direction only.
11. Produce concise outputs that distinguish recommendations from commitments.

## Classification Model

Classify candidate work by its primary track. Use secondary tracks only when they
change sequencing or artifact needs.

| Track | Use When |
| --- | --- |
| Product | The item changes product positioning, audience, or user value. |
| Technical | The item improves internals without changing visible product behavior. |
| Platform | The item changes hostability, extension seams, packaging, or composition boundaries. |
| Parser Quality | The item changes accepted input behavior, parser compatibility, or data fidelity. |
| UX | The item changes user workflow, screen behavior, interaction, or feedback. |
| Distribution | The item changes how users install, launch, or package ReplayLab. |
| Documentation | The item clarifies usage, boundaries, decisions, or release state. |

## Readiness Levels

Use readiness to avoid creating implementation tickets before the product shape
is clear.

| Level | Meaning | Typical Next Step |
| --- | --- | --- |
| Discovery | The idea is plausible but unresolved. | Keep or refine discovery issue. |
| Directional Candidate | Value and rough shape are clear, but decisions remain. | Business analysis, BRD, PRD, or ADR candidate. |
| Shapable Milestone | Scope, dependencies, and risks are clear enough to plan. | Milestone plan. |
| Implementation Ready | Accepted milestone scope has concrete behavior and boundaries. | Implementation issues. |

## Decision Rules

- Create a BRD when business/user value or market/product direction must be
  clarified before choosing scope.
- Create a PRD when accepted product behavior must be specified.
- Create an ADR when a durable technical boundary or architecture decision is
  needed.
- Create a milestone plan when delivery scope is ready to be shaped.
- Create implementation issues only after milestone scope is accepted.
- Do not turn every discovery idea into an implementation ticket.
- Do not freeze milestone numbering until dependencies and value support the
  sequence.
- Update the roadmap with status, candidate tracks, and recommended sequencing,
  not detailed implementation plans.

## Output Checklist

- Current product position is stated in a few bullets.
- Candidate tracks are classified.
- Dependencies are explicit.
- Priorities distinguish recommendation from commitment.
- Candidate milestones are labeled as candidates unless accepted.
- Needed BRDs, PRDs, ADRs, and milestone plans are listed without creating them
  by default.
- Deferred scope is explicit.
- Recommended next action is clear and small.
- Roadmap updates match the analysis.
- No product code is changed.
- No implementation issues are created during analysis.

## Anti-Patterns

- No implementation during business analysis.
- No issue explosion.
- No fake milestone for every idea.
- No PRD or ADR unless a real product or architecture decision is needed.
- No broad docs bureaucracy.
- No treating discovery labels as accepted scope.
- No hiding dependencies by assigning premature milestone numbers.
