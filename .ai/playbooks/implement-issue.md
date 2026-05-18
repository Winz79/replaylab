# Implement Issue Playbook

Use this when implementing a GitHub issue or a small issue batch.

## Before Editing

1. Read the issue goal, scope, acceptance criteria, and linked docs.
2. Read any linked PRDs, ADRs, milestone plans, or prior review notes.
3. Inspect the relevant code and tests before changing files.
4. Restate the intended vertical slice in a short task plan.
5. If the issue is too large or unclear, propose a split or ask for the missing decision before implementing.

## Branch

- Use one branch per issue or tightly related issue batch.
- Prefer `codex/<issue-or-slice-name>` unless the repository owner requests another convention.
- Do not mix unrelated cleanup, refactors, or product direction changes into the branch.

## Implementation

- Make the smallest coherent vertical slice that satisfies the issue.
- Keep architecture boundaries intact, especially core independence from adapters, CLI, UI, persistence, Docker, WCF, and business-specific concerns.
- Add or update focused tests for behavior changes.
- Update docs or ADRs only when the implementation changes design, architecture, or user-facing workflow.
- Avoid speculative abstractions and broad rewrites.

## PR

The PR description should include:

- linked issue or issue batch
- summary of the slice
- verification performed
- assumptions
- risks
- out of scope
- follow-up issues or drafts, if needed

## Done Criteria

- Scope matches the issue.
- Focused verification has been run or explicitly marked not required.
- The diff is reviewable and avoids unrelated changes.
- Remaining assumptions and risks are stated.
