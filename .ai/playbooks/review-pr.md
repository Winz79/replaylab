# Review PR Playbook

Use this when reviewing a pull request.

## Review Setup

1. Read the PR description, linked issues, and stated out-of-scope items.
2. Read linked PRDs, ADRs, milestone plans, and docs before judging design choices.
3. Inspect the diff against the intended vertical slice.
4. Check whether verification evidence matches the changed behavior.

## Review Focus

- Scope: does the PR solve the linked issue without unrelated changes?
- Architecture: are repository boundaries and dependency directions preserved?
- Tests: are important behaviors covered, and are test expectations aligned with the issue?
- Docs: were docs or ADRs updated when behavior or design changed?
- Simplicity: is the implementation direct, or does it introduce premature abstractions?
- Risks: are assumptions, edge cases, and follow-ups clearly stated?

## Findings

Report findings first, ordered by severity.

Each finding should include:

- file and line reference when possible
- what is wrong
- why it matters
- a concrete fix direction

Avoid blocking on style preferences unless they affect correctness, maintainability, architecture, or user-facing behavior.

## Completion

End with:

- residual risks or testing gaps
- whether the PR is in scope
- whether follow-up issues are needed
