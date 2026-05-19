You are working in the ReplayLab repository.

Goal:
Prepare the next milestone after M1.

Target milestone:
M2 - Local Executable Distribution
GitHub milestone:
https://github.com/Winz79/replaylab/milestone/2

Important:
Do not write product implementation code yet.
Do not implement packaging yet.
Do not create tickets before the planning artifacts are updated.
Do not work on M3.
Do not add Docker, NuGet publishing, Web UI, HTTP sender, WCF, persistence, or configuration DSL.

Context:
M1 - Local CLI Replay Preview is complete. ReplayLab can now run a local CLI preview using a synthetic CSV file, parse messages, replay them through the mock sender, and print a concise result summary.

M2 should make ReplayLab easier to run as a local executable without opening the solution in an IDE.

Expected M2 outcome:
- publish strategy clarified
- local executable distribution path documented
- published executable can run the existing CLI preview
- sample CSV can be used with published output
- release/package approach is prepared but not overbuilt

Use the AI-first engineering workflow:
Intent
→ Context
→ Requirements
→ Impact
→ Design
→ Delivery Plan
→ Build / Review / Learn

Task:
Create the planning artifacts required for M2, then draft GitHub issues for implementation.

Phase 1 - Inspect repository:
1. Inspect current repository state.
2. Read:
   - README.md
   - docs/vertical-slice-plan.md
   - docs/prd/
   - docs/brd/
   - docs/adr/
   - .ai/AGENTS.md
3. Identify existing decisions that constrain M2:
   - .NET target
   - CLI behavior
   - public/private boundaries
   - modular toolkit architecture
   - release/distribution assumptions

Phase 2 - Create or update planning docs:
Create the minimum useful planning artifacts for M2.

Create:
- docs/prd/0006-local-executable-distribution.md
- docs/adr/0005-distribution-strategy.md
- docs/milestones/m2-local-executable-distribution.md

The PRD-light must include:
- purpose
- users
- requirements
- acceptance criteria
- out of scope
- assumptions
- open questions

The ADR must decide the first distribution strategy.

Decision direction:
Use `dotnet publish` as the first local executable distribution mechanism.

Consider alternatives:
1. run from source only
2. single-file executable
3. framework-dependent publish
4. self-contained publish
5. NuGet global tool
6. Docker image

The ADR should make a pragmatic first decision, not a final forever decision.

The milestone plan must include:
- goal
- definition of done
- candidate vertical slices
- issue draft list
- risks
- explicit non-goals

Phase 3 - Draft GitHub issues:
After planning artifacts are created, draft 4 to 6 GitHub issues for M2.

Do not create the issues yet unless explicitly asked.

Suggested issue candidates:
1. Docs/Decision: define local executable distribution strategy
2. Build: add publish command or script for local executable output
3. Tests: verify published executable with sample CSV
4. Docs: document local executable usage
5. Release: prepare GitHub release artifact workflow, only if justified
6. Versioning: add basic version output, only if justified

Each issue draft must include:
- title
- target milestone: M2 - Local Executable Distribution
- suggested labels
- goal
- source docs
- scope
- out of scope
- acceptance criteria
- verification
- risks / assumptions

Rules:
- Keep M2 small.
- Prefer local executable distribution over release automation if unsure.
- Do not start with Docker.
- Do not start with NuGet publishing.
- Do not introduce WCF or private adapters.
- Do not add new product features.
- Do not change CLI behavior unless needed for packaging verification.
- Keep docs concise and practical.
- If an implementation concern appears but is not needed for M2, record it as a future consideration.

GitHub issue creation:
At the end, ask for confirmation before creating issues.

Output:
1. Summary of repository state after M1.
2. Files created or updated.
3. Summary of PRD 0006.
4. Summary of ADR 0005.
5. Summary of M2 milestone plan.
6. Proposed issue drafts.
7. Clear recommendation: which issues should be created now.
8. Do not create issues until I explicitly say so.