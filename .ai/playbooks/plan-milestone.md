# Plan Milestone Playbook

Use this when planning a milestone before creating implementation issues.

## Inputs

Start by reading:

- current roadmap or milestone notes
- relevant PRDs or PRD-light drafts
- existing ADRs and architecture notes
- recently closed issues and PRs
- open questions from prior milestones

## Planning Flow

1. Define the milestone goal in one or two sentences.
2. Write a PRD-light summary covering users, outcomes, non-goals, constraints, and success criteria.
3. Identify ADR candidates for architecture decisions that need explicit agreement.
4. Break the work into small vertical slices.
5. Draft issues for each slice instead of creating issues by default.
6. Add verification expectations and risks to every issue draft.

## Issue Draft Template

Each draft should include:

- title
- goal
- scope
- acceptance criteria
- linked docs or ADRs
- implementation notes
- test expectations
- risks
- out of scope

## Quality Bar

- Keep slices small enough for focused PRs.
- Prefer sequencing that proves risky assumptions early.
- Separate planning, architecture decisions, implementation, and cleanup.
- Do not create GitHub issues unless explicitly asked.
