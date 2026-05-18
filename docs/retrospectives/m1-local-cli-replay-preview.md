# M1 Retrospective - Local CLI Replay Preview

## What worked

- The AI-first workflow was useful: docs/PRDs/ADRs/issues gave the agents enough context to work in small slices instead of improvising.
- The issue-driven flow worked well:
  - PRD / ADR / vertical slice plan
  - GitHub issue
  - branch
  - PR
  - verification
  - merge
- The public/private boundary stayed clean:
  - no WCF
  - no proprietary formats
  - no customer data
  - no business-specific mappings
- `ReplayLab.Core` stayed independent from CLI, parser implementation, mock adapter, UI, Docker, persistence, and WCF concerns.
- The first end-to-end local flow now exists:
  - CSV sample
  - CSV parser
  - `ReplayBatch`
  - sequential replay engine
  - mock sender
  - CLI summary
  - smoke tests
- The PR summaries were useful and mostly well structured: files changed, tests run, assumptions, risks, and out-of-scope were visible.
- The milestone approach helped make the work feel like a product increment rather than random commits.

## What was too heavy

- Too many planning artifacts were created early compared to the amount of actual product behavior.
- The BRD/PRD/ADR structure is useful, but it can become heavy if every tiny change gets a full document.
- Some issues were very detailed, almost implementation plans rather than simple executable work items.
- The agent sometimes needed very explicit constraints to avoid expanding the scope.
- Some documentation work was split into several issues; that was clean, but slightly bureaucratic for such a small project.
- The first slices consumed a lot of AI quota because the workflow itself was being designed while the product was being built.

## What agents did well

- They followed explicit scope boundaries when those boundaries were clearly written.
- They kept WCF, business-specific formats, customer data, and private mappings out of the public repository.
- They produced useful PR descriptions with assumptions, risks, test commands, and out-of-scope sections.
- They added tests alongside implementation, especially for parser behavior, replay engine behavior, and CLI smoke testing.
- They respected the modular toolkit direction:
  - Core contracts
  - parser project
  - adapter project
  - CLI composition
- They updated docs when the implementation made the current state clearer.
- They were effective when given one issue at a time.

## What agents overdid

- They tended to produce very extensive issue bodies and plans.
- They sometimes leaned toward documentation completeness before product usefulness.
- They needed repeated reminders not to introduce future concerns such as UI, Docker, HTTP, WCF, persistence, plugin systems, or mapping configuration.
- They could easily turn a small task into a mini-framework if not constrained.
- Some outputs were very formal for a small early-stage tool.
- They sometimes assumed that “better engineering” means adding more structure, while the real goal was a small working vertical slice.

## What should be improved in .ai/AGENTS.md

- Add stronger guidance that documentation should be proportional to the size of the change.
- Add a rule: for small implementation issues, prefer updating existing docs over creating new documents.
- Add a rule: do not create new abstractions unless at least two concrete use cases require them.
- Add a rule: one issue should normally produce one coherent PR.
- Add a rule: if an issue is documentation-only, do not modify product code.
- Add a rule: if an issue is implementation-only, update docs only when behavior or architecture changed.
- Add a rule: prefer “small useful slice” over “complete framework”.
- Add a rule: before adding a new project, folder, or abstraction, explain why existing structure is not enough.
- Add a rule: keep future features as explicit follow-up issues instead of partially implementing them.

Suggested addition:

```md
## Proportionality

Keep the amount of documentation, abstraction, and process proportional to the size of the change.

For small issues:
- prefer updating existing documents over creating new ones
- avoid new abstractions unless clearly justified
- avoid future-proofing beyond the issue scope
- keep the PR focused on one coherent outcome

If a useful idea appears but is outside the current issue, capture it as a follow-up issue instead of implementing it immediately.
```
