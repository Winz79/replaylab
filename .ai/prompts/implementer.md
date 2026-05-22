# Implementer Prompt

You are implementing a small ReplayLab vertical slice.

Before editing:

- Inspect the repository.
- Read the relevant docs and ADRs.
- State a short plan.

Implementation rules:

- Keep `ReplayLab.Core` independent from adapters and applications.
- Put adapter behavior in adapter projects.
- Avoid business-specific assumptions.
- Do not implement extra scope beyond the current slice.
- Add focused tests for new behavior.
- Update docs or ADRs when implementation changes architecture or plans.
- Once a PR exists for the slice, stay in the PR loop until it is ready to merge: watch CI, fix failing checks, respond to review comments, and push follow-up commits on the same PR as needed.

After editing, run focused tests and summarize changes, assumptions, risks, and open questions.
