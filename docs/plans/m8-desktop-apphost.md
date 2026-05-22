# M8 Desktop AppHost with Photino.NET Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Build a desktop AppHost that launches and embeds the ReplayLab Web UI inside the platform-native web view, while the desktop shell owns startup, shutdown, windowing, and composition-root responsibilities.

**Architecture:** M7 already provides hostable CLI/Web surfaces; M8 uses that seam to create a desktop shell around the Web experience. The desktop app owns the composition root and local lifetime management, starts the web host on loopback, and points the native web view at the local UI. Keep the desktop surface generic and avoid business-specific adapters or product data models in the public repo.

**Tech Stack:** .NET 10, Photino.NET, ASP.NET Core, ReplayLab.Web.Hosting, xUnit, platform runtime checks.

---

### Task 1: Decide the desktop host contract and document it

**Objective:** Lock the implementation shape before adding code so the desktop shell has a clear boundary and dependency model.

**Files:**
- Create: `docs/adr/0010-desktop-apphost-strategy.md`
- Modify: `docs/milestones/m8-desktop-apphost.md` if the repository adds a milestone tracker for M8
- Modify: `docs/roadmap.md`
- Reference: `docs/adr/0009-hostable-entry-points.md`, `docs/roadmap.md`, `issues/70`

**Step 1: Write the decision draft**

Document the unresolved questions from issue #70:
- native desktop shell choice
- fixed port vs dynamic loopback port
- in-process Kestrel/self-hosted Web UI vs alternative bridge model
- whether packaging is included in M8 or deferred

**Step 2: Run a document review**

Confirm the decision keeps the desktop shell outside the public/private adapter boundary and only depends on the M7 hostable Web entry points.

**Step 3: Finalize the ADR**

Record the accepted desktop hosting model and the ownership boundary between the desktop host, the Web app, and the composition root.

**Step 4: Commit**

```bash
git add docs/adr/0010-desktop-apphost-strategy.md docs/roadmap.md
git commit -m "docs: define desktop apphost strategy"
```

### Task 2: Scaffold the desktop project

**Objective:** Add the desktop app project and solution wiring without yet embedding the Web UI.

**Files:**
- Create: `src/ReplayLab.Desktop/ReplayLab.Desktop.csproj`
- Create: `src/ReplayLab.Desktop/Program.cs` or the desktop framework equivalent entry file
- Modify: `ReplayLab.sln`
- Modify: `src/ReplayLab.Desktop/README.md` if the project needs a local note

**Step 1: Write the failing build expectation**

Add the desktop project skeleton to the solution and verify the repo does not yet compile it.

**Step 2: Create the minimal shell**

Create the window bootstrap, app entry, and project file using the accepted Photino.NET shell from Task 1.

**Step 3: Build the solution**

Confirm the new project compiles alongside the existing solution.

**Step 4: Commit**

```bash
git add src/ReplayLab.Desktop ReplayLab.sln
git commit -m "feat: scaffold desktop apphost"
```

### Task 3: Self-host ReplayLab Web inside the desktop process

**Objective:** Start the local Web app from the desktop shell and expose it on loopback for the embedded browser.

**Files:**
- Create: `src/ReplayLab.Desktop/Hosting/DesktopWebHost.cs` or a small bootstrap helper
- Create: `src/ReplayLab.Desktop/Hosting/DesktopHostOptions.cs` if the bootstrap needs extracted options
- Modify: `src/ReplayLab.Desktop/Program.cs`
- Reference: `src/ReplayLab.Web.Hosting/*`

**Step 1: Write the failing integration test or smoke harness**

Add a test that starts the desktop host, waits for the local HTTP endpoint, and asserts the Web host comes up.

**Step 2: Implement the local host bootstrap**

Start the Web host on a loopback port chosen by the desktop shell, then surface the final URL to the browser control.

**Step 3: Verify startup and shutdown**

Confirm the Web host starts before navigation and shuts down cleanly when the desktop app exits.

**Step 4: Commit**

```bash
git add src/ReplayLab.Desktop tests/ReplayLab.Desktop.Tests
git commit -m "feat: host replaylab web from desktop shell"
```

### Task 4: Embed the native web view and wire navigation

**Objective:** Render the ReplayLab Web UI inside the desktop shell and make the local lifecycle usable.

**Files:**
- Modify: `src/ReplayLab.Desktop/Program.cs`
- Modify: `src/ReplayLab.Desktop/ReplayLab.Desktop.csproj`
- Test: `tests/ReplayLab.Desktop.Tests/*`

**Step 1: Write the failing UI smoke test**

Create a test that proves the desktop bootstrap exposes the local ReplayLab UI endpoint for the embedded native web view.

**Step 2: Add the browser control**

Create the native window and navigate it to the self-hosted Web URL from Task 3.

**Step 3: Add basic lifecycle handling**

Ensure close/restart flows stop the local Web host cleanly and do not leak the browser process.

**Step 4: Re-run the desktop smoke tests**

Verify the embedded shell loads the ReplayLab UI and exits without errors.

**Step 5: Commit**

```bash
git add src/ReplayLab.Desktop tests/ReplayLab.Desktop.Tests
git commit -m "feat: embed replaylab web ui in desktop apphost"
```

### Task 5: Validate packaging and document the new boundary

**Objective:** Make the desktop host discoverable and ensure the repo docs describe the new milestone accurately.

**Files:**
- Modify: `README.md`
- Modify: `docs/roadmap.md`
- Modify: `docs/milestones/m8-desktop-apphost.md` if it exists
- Modify: `docs/adr/0010-desktop-apphost-strategy.md` if implementation diverges from the initial decision

**Step 1: Review the docs against the code**

Confirm the desktop host, the Web host, and the composition root ownership are described consistently.

**Step 2: Add runtime notes**

Document any platform runtime expectations and whether a packaged app or a developer-run app is the intended M8 deliverable.

**Step 3: Update the roadmap linkage**

Make sure the roadmap points at the M8 plan and marks M8 as the active milestone.

**Step 4: Commit**

```bash
git add README.md docs/roadmap.md docs/milestones/m8-desktop-apphost.md docs/adr/0010-desktop-apphost-strategy.md
git commit -m "docs: document desktop apphost milestone"
```

## Recommended Order

1. Desktop host decision and ADR
2. Desktop project scaffold
3. Self-hosted Web bootstrap
4. Native web view embedding and lifecycle
5. Packaging/docs cleanup

## Why this is the next work

- M7 hostability is the prerequisite, and the roadmap now treats it as complete.
- Issue #70 already frames desktop hosting as the next product-shell discovery path.
- Desktop AppHost is the smallest meaningful step beyond hostable CLI/Web that materially changes the product experience.
- It can be built without introducing private business-specific adapters into the public repo.
