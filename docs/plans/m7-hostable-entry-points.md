# M7 Hostable Entry Points Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Extract hostable CLI and Web entry points so private projects can compose their own adapters and parsers through DI without modifying the public ReplayLab repo.

**Architecture:** Keep the repo-owned `ReplayLab.Cli` and `ReplayLab.Web` projects as thin runnable shells, and move reusable host behavior into companion host libraries. The private project owns the composition root and registers concrete adapters/parsers; ReplayLab owns only generic replay workflow behavior and host hooks.

**Tech Stack:** .NET 10, ASP.NET Core, ReplayLab.Core, ReplayLab.Cli, ReplayLab.Web, xUnit, DI abstractions.

---

### Task 1: Capture the CLI host boundary in code

**Objective:** Define the reusable CLI runner surface without changing observable CLI behavior.

**Files:**
- Modify: `src/ReplayLab.Cli/CliApplication.cs`
- Modify: `src/ReplayLab.Cli/Program.cs`
- Test: `tests/ReplayLab.Cli.Tests/*` or the closest CLI test project

**Step 1: Write failing test**

Add a test that exercises the CLI through a supplied `IServiceProvider` so the composition root is not created inside the reusable runner.

**Step 2: Run the test to verify the current structure blocks the desired shape**

Run the closest CLI test scope and confirm the runner still assumes internal composition.

**Step 3: Refactor minimally**

Split the current shell into:
- a tiny repo-owned entry point in `Program.cs`
- a reusable hostable runner that accepts args, writers, and an externally owned service provider

**Step 4: Re-run the CLI tests**

Verify existing CLI behavior still passes.

**Step 5: Commit**

```bash
git add src/ReplayLab.Cli tests/ReplayLab.Cli.Tests docs/plans/m7-hostable-entry-points.md
git commit -m "feat: begin hostable cli entry point"
```

### Task 2: Capture the Web host boundary in code

**Objective:** Define the reusable Web composition hooks while keeping the repo-owned app shell thin.

**Files:**
- Modify: `src/ReplayLab.Web/Program.cs`
- Modify: `src/ReplayLab.Web/*Hosting*` or the closest hosting composition file
- Test: `tests/ReplayLab.Web.Tests/*` or the closest Web test project

**Step 1: Write failing test**

Add a test that builds the Web app using reusable registration and endpoint mapping hooks instead of app-only startup logic.

**Step 2: Run the test to verify current shell-only startup is still coupled**

Confirm the reusable hook shape is not yet available.

**Step 3: Refactor minimally**

Introduce hostable composition methods such as `AddReplayLabWeb` and `MapReplayLabWeb`, then keep `Program.cs` as a thin shell that invokes them.

**Step 4: Re-run the Web tests**

Verify the current local Web workflow still works.

**Step 5: Commit**

```bash
git add src/ReplayLab.Web tests/ReplayLab.Web.Tests docs/plans/m7-hostable-entry-points.md
git commit -m "feat: add hostable web composition hooks"
```

### Task 3: Prove private-host composition works end to end

**Objective:** Demonstrate that a private project can own the composition root while invoking ReplayLab hostable entry points.

**Files:**
- Create or modify: `samples/ReplayLab.HostSample/*`
- Modify: any hostable entry point documentation files
- Test: host sample tests if present

**Step 1: Write failing test or sample harness**

Create a small sample that registers synthetic adapters/parsers in a private composition root and invokes the hostable CLI/Web surface.

**Step 2: Run the sample test or build**

Confirm the hostable surface is callable from outside the repo-owned app shell.

**Step 3: Add minimal integration glue**

Wire the sample to the new hostable surface without introducing business-specific code.

**Step 4: Verify repo workflows still pass**

Run the relevant CLI/Web validation plus the sample.

**Step 5: Commit**

```bash
git add samples/ReplayLab.HostSample docs/plans/m7-hostable-entry-points.md
git commit -m "feat: prove hostable entry points with sample host"
```

### Task 4: Update the documentation boundary

**Objective:** Make the new hostable entry-point model discoverable and explicit.

**Files:**
- Modify: `README.md`
- Modify: `docs/roadmap.md`
- Modify: `docs/milestones/m7-hostable-entry-points.md` if scope details changed
- Modify: `docs/adr/0009-hostable-entry-points.md` only if the implementation reveals an API boundary change

**Step 1: Review the docs against the implemented surface**

Check whether the docs still match the actual hostable API shape.

**Step 2: Update only the affected language**

Keep the docs aligned with the code and avoid broad rewrites.

**Step 3: Verify links and scope statements**

Ensure the roadmap, milestone, and README all describe the same boundary.

**Step 4: Commit**

```bash
git add README.md docs/roadmap.md docs/milestones/m7-hostable-entry-points.md docs/adr/0009-hostable-entry-points.md
git commit -m "docs: document hostable entry point milestone"
```

## Recommended Order

1. CLI host boundary first
2. Web host boundary second
3. Private-host sample third
4. Documentation cleanup last

## Why this is the next work

- It is the current roadmap milestone.
- It is the smallest useful architectural step after M6.
- CLI hostability is the least risky place to prove the pattern before Web.
- The repo already has an accepted ADR, so implementation can start without more architecture debate.
