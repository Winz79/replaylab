You are working in a new empty repository named `replaylab`.

Goal:
Scaffold a new .NET project and its local AI-first engineering workspace.

This project is the first real product using my AI Engineering Toolkit methodology:
../ai-engineering-toolkit

Important:
Do not copy the toolkit repository.
Do not vendor the toolkit.
Do not create a dependency on the toolkit.
Use it only as conceptual methodology guidance.

Product intent:
ReplayLab is a lightweight replay/testing toolkit for loading structured messages from files, inspecting them, selecting/filtering them, and replaying them through configurable sender adapters.

It must stay generic and open-source friendly.

Public scope:
- generic replay engine
- generic message model
- CSV/JSON parser direction
- mock sender
- optional HTTP sender later
- local web UI later
- CLI later
- Docker packaging later

Out of scope:
- proprietary business formats
- internal WCF contracts
- real certificates
- real customer data
- company-specific mappings
- business-specific adapters

Use this methodology flow:
Intent
→ Context
→ Requirements
→ Impact
→ Design
→ Delivery Plan
→ Build / Review / Learn

Task:
Create the initial repository scaffold only.

Create documentation:

docs/
├── intake-brief.md
├── context-discovery.md
├── brd-light.md
├── architecture-brief.md
├── vertical-slice-plan.md
└── adr/
    ├── 0001-use-dotnet.md
    └── 0002-separate-core-from-adapters.md

Create local AI instructions:

.ai/
├── AGENTS.md
└── prompts/
    ├── planner.md
    ├── implementer.md
    └── reviewer.md

Create initial .NET solution skeleton:

src/
├── ReplayLab.Core/
├── ReplayLab.Cli/
└── ReplayLab.Adapters.Mock/

tests/
├── ReplayLab.Core.Tests/
└── ReplayLab.Adapters.Mock.Tests/

Expected .NET setup:
- Create a solution file `ReplayLab.sln`.
- Create class library project `ReplayLab.Core`.
- Create console project `ReplayLab.Cli`.
- Create class library project `ReplayLab.Adapters.Mock`.
- Create test projects for Core and Mock adapter.
- Use current stable .NET SDK available on the machine.
- Prefer simple, standard .NET project layout.
- Do not add unnecessary frameworks.
- Do not add UI yet.
- Do not add Docker yet.
- Do not add real replay implementation yet.

Initial code scope:
Keep code minimal.

In `ReplayLab.Core`, create only basic abstractions:
- `ReplayMessage`
- `ReplayResult`
- `IMessageParser`
- `IReplaySender`
- `ReplayBatch`

In `ReplayLab.Adapters.Mock`, create:
- `MockReplaySender` implementing `IReplaySender`

In `ReplayLab.Cli`, create:
- minimal Program.cs that prints a short placeholder message and exits successfully

Tests:
Add only minimal smoke tests:
- Core types can be instantiated
- MockReplaySender returns a successful ReplayResult

Documentation rules:
- Mark assumptions explicitly.
- Add open questions instead of inventing missing context.
- Keep docs concise.
- Keep project generic.
- Mention that WCF or business-specific adapters must live outside the public repo.

AI instruction rules:
`.ai/AGENTS.md` should tell agents:
- inspect the repository before editing
- do not code before a short plan exists
- work in small vertical slices
- keep core independent from adapters
- do not introduce business-specific assumptions
- update ADRs for meaningful architecture decisions
- update docs when implementation changes the plan
- summarize risks and assumptions after each task

Constraints:
- Do not implement CSV parsing yet.
- Do not implement UI yet.
- Do not implement Docker yet.
- Do not introduce WCF.
- Do not introduce persistence.
- Do not overengineer.
- Keep the first commit small, clean, and reviewable.

After changes:
1. Show the created file tree.
2. Summarize what was created.
3. List assumptions made.
4. List open questions.
5. Suggest the next vertical slice.