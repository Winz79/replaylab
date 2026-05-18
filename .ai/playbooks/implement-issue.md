# Implement Issue Playbook

Use this when implementing a GitHub issue or a small issue batch.

## Before Editing

1. Read the issue goal, scope, acceptance criteria, and linked docs.
2. Read any linked PRDs, ADRs, milestone plans, or prior review notes.
3. Inspect the relevant code and tests before changing files.
4. Restate the intended vertical slice in a short task plan.
5. If the issue is too large or unclear, propose a split or ask for the missing decision before implementing.

## Completion Requirement

The task is not complete until:

- scope matches the issue or issue batch
- changes are implemented
- focused verification has been run or explicitly skipped with a reason
- the diff is reviewable and avoids unrelated changes
- changes are committed
- if a dedicated branch is being used, the branch is pushed
- if the user has not explicitly told you not to create one, a pull request is created
- if a pull request is created, it links every completed issue with `Closes #...`
- if a pull request is created, the PR body includes:
  - linked issue or issue batch
  - summary
  - files changed
  - verification performed
  - assumptions
  - risks
  - out of scope
  - follow-up issues or drafts, if needed

Do not stop after local edits.
Do not stop after summarizing changes.
If a pull request is required but cannot be created, explain exactly why and provide the command the user should run.

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


## PR Creation

Always create a pull request unless explicitly told not to.

Use a title that describes the implemented slice.
Use `Closes #...` for every completed issue.
