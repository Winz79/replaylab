# M10A Packageable ReplayLab SDK Implementation Plan

**Goal:** Make ReplayLab consumable as local NuGet packages by external solutions so developers can reference ReplayLab packages, provide custom parsers and adapters, and quickly ship a replay tool.

**Architecture:** Build on the M6 packageable core and M7 hostable entry points by adding package metadata and a local pack workflow for all public projects that should be externally referenceable. Keep the public/private adapter boundary intact and avoid packaging business-specific or proprietary concerns.

**Tech Stack:** .NET 10, NuGet, PowerShell, local feed verification.

---

### Task 1: Identify and normalize package metadata

**Objective:** Decide which projects become packages and ensure their metadata is consistent.

**Files:**
- Inspect: `src/*/*.csproj` for packageable candidates
- Create or modify: `Directory.Build.props` or per-project `.csproj` metadata

**Step 1: Confirm package candidates**

- `ReplayLab.Core`
- `ReplayLab.Parsers.Csv`
- `ReplayLab.Adapters.Mock`
- `ReplayLab.Adapters.Http`
- `ReplayLab.Cli.Hosting`
- `ReplayLab.Web.Hosting`
- Decide whether `ReplayLab.Desktop` stays an executable app or whether a reusable `ReplayLab.Desktop.Hosting` library is extracted (see #101).

**Step 2: Normalize metadata**

Add or align `PackageId`, `Version`, `Authors`, `Description`, and `PackageTags` across candidates. Keep versions consistent with the repo's current tagging scheme.

**Step 3: Commit**

```bash
git add src/**/*.csproj Directory.Build.props
git commit -m "build: normalize package metadata for SDK projects"
```

### Task 2: Create a local pack script

**Objective:** Provide a single script that produces all local packages into a known output directory.

**Files:**
- Create: `eng/pack-local.ps1`

**Step 1: Write the script**

The script should:
- Build the solution in Release.
- Pack each candidate project to `artifacts/packages`.
- Verify that the output directory contains the expected `.nupkg` files.

**Step 2: Test the script locally**

Run `eng/pack-local.ps1` and confirm packages appear in `artifacts/packages`.

**Step 3: Commit**

```bash
git add eng/pack-local.ps1
git commit -m "build: add local pack script for SDK packages"
```

### Task 3: Verify local feed restore

**Objective:** Prove that an external-style project can restore these packages from a local feed.

**Files:**
- Create: temporary or sample `NuGet.config` for verification

**Step 1: Add a local feed config**

Create a minimal `NuGet.config` that points to `artifacts/packages` as a package source.

**Step 2: Restore from the local feed**

Create a temporary project that references one or more ReplayLab packages via `PackageReference` and confirm `dotnet restore` succeeds using the local feed.

**Step 3: Document the workflow**

Add a short section to the README or a dedicated doc explaining how to produce and consume local packages.

**Step 4: Commit**

```bash
git add README.md docs/
git commit -m "docs: document local NuGet package workflow"
```

### Task 4: Update documentation boundaries

**Objective:** Ensure the roadmap, README, and plan docs describe the packageable SDK consistently.

**Files:**
- Modify: `docs/roadmap.md`
- Modify: `README.md`
- Modify: `docs/plans/m10-packageable-sdk.md` if scope changed during implementation

**Step 1: Review docs against the actual package set**

Confirm the docs list the same candidates as the script.

**Step 2: Update only affected language**

Keep changes minimal and consistent with the existing docs style.

**Step 3: Commit**

```bash
git add docs/roadmap.md README.md docs/plans/m10-packageable-sdk.md
git commit -m "docs: document packageable SDK milestone"
```

## Recommended Order

1. Identify and normalize package metadata.
2. Create the local pack script.
3. Verify local feed restore.
4. Update documentation boundaries.

## Linked Issues

- #99 — Package ReplayLab SDK for local NuGet consumption
- #101 — Extract reusable Desktop hosting seam (if the desktop package shape changes)

## Out Of Scope

- Publishing to nuget.org.
- Signing packages.
- Release automation.
- Dynamic plugin loading.
- Business-specific adapter packages.

## Risks

- `ReplayLab.Web.Hosting` may require extra care because it includes Razor/static assets and currently references default parser/adapter projects.
- Desktop reuse may require extracting a hosting library instead of packaging the executable app directly.
- Package metadata drift if not centralized through `Directory.Build.props`.
