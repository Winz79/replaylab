# Discovery to Slice Playbook

## Purpose

Use this playbook when product or business direction is fuzzy and the team needs
one short recommendation before handing work to the Conductor or Implementer.

The goal is not to create a heavy discovery process. The goal is to answer the
minimum useful "what, why, for whom, and what next" questions, then stop.

## When to Use

Use this playbook when the question is upstream of implementation, such as:
- what should we build next
- why it matters
- who it is for
- what the MVP boundary is
- which option should be prioritized now

Skip this playbook when the next implementation slice is already clear.

## Workflow

1. Inspect only the necessary context.
   - `README.md`
   - roadmap
   - current milestone docs
   - relevant PRD / BRD / ADR docs
   - open issues only if needed

2. Clarify product or business intent.
   - State the problem in plain language.
   - Identify the target user or operator.
   - Capture why the work matters now.

3. Compare options.
   - List the realistic options, not every possible idea.
   - Note tradeoffs in value, cost, dependency, and risk.

4. Pick one direction.
   - Choose one recommended path.
   - Say why it wins now over the alternatives.

5. Define MVP boundary.
   - Identify the smallest slice that proves value.
   - Separate must-have from later expansion.

6. Produce one implementation-ready slice.
   - Define one small next slice that can be handed off immediately.
   - Keep it concrete enough for a task contract or issue draft if needed.

7. Hand off to Conductor or Implementer.
   - Use Conductor when the work still needs routing, sequencing, or splitting.
   - Use Implementer when the slice is already scoped and ready.

## Output Expectations

Keep the output short, decision-oriented, and execution-ready.

Include:
1. Product problem
2. Target user / operator
3. Value proposition
4. Options considered
5. Recommended direction
6. MVP boundary
7. Explicit out of scope
8. Risks / unknowns
9. Next implementation-ready slice
10. Suggested task contract or issue draft, only if durable tracking is needed

## Rules

- Discovery is for fuzzy direction, not for obvious tasks.
- Brainstorming must end in prioritization.
- Prefer one recommended path over a menu of equal options.
- Do not create issues by default.
- Do not rewrite roadmap or milestone docs unless explicitly requested.
- Do not spill into conductor-style routing or implementation detail.
- Stop once one implementation-ready slice exists.
