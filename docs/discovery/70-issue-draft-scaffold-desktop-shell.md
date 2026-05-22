# Issue Draft: Scaffold Cross-Platform Desktop Shell with Photino.NET

> Parent discovery issue: #70
> Derived from: `docs/discovery/70-desktop-apphost-discovery.md`

---

## Goal

Build a minimal Photino.NET desktop application that embeds the ReplayLab Web UI
inside the platform-native web view by self-hosting ASP.NET Core in-process via
the existing M7 hostable Web seam (`ReplayLab.Web.Hosting`).

## Scope

1. Create `src/ReplayLab.Desktop` — Photino.NET desktop project targeting `net10.0`.
2. Add package references:
   - `Photino.NET`
   - `Microsoft.AspNetCore.App` (or host builder packages)
   - `ReplayLab.Web.Hosting`
3. Implement minimal shell:
   - In-process `WebApplication` bootstrap on a free loopback port
   - Native web view navigation to the discovered local URL after host is listening
   - Graceful host shutdown on window close
4. Register `ReplayLab.Desktop` in `ReplayLab.sln`.
 5. Add a smoke / integration test proving the host starts and the hosted Web UI responds.

## Acceptance Criteria

- [ ] `dotnet build ReplayLab.sln` includes the desktop project and passes.
- [ ] Desktop app launches and renders the ReplayLab Web UI inside the window.
- [ ] Close button stops the Kestrel host cleanly (no process leaks).
- [ ] Smoke test passes for the public desktop bootstrap seam.
- [ ] `README.md` and roadmap docs reference the new project accurately.

## Out of Scope

- Additional desktop shells beyond the public Photino.NET app host.
- Packaging beyond the current self-contained publish targets.
- Runtime bundling beyond the OS/browser prerequisites.
- Private adapter registration in the public desktop shell (uses M7 safe defaults).
- Editable grid, parser quality, HTTP sender UI changes.
- Installer creation (MSIX, WiX, etc.).
- NuGet packaging of the desktop project.

## Test Expectations

- Focused test: start the desktop host, assert the local HTTP endpoint responds,
  assert the discovered local URL is the loopback address used by the shell.
- Keep test lightweight; avoid full UI automation where possible.

## Risks

- Linux machines may miss `libwebkit2gtk-4.0`; document requirement.
- Windows machines may miss WebView2 runtime; document requirement.
- Kestrel port discovery may race during startup; probe availability before binding.

## Linked Docs

- `docs/discovery/70-desktop-apphost-discovery.md`
- `docs/plans/m8-desktop-apphost.md`
- ADR 0009 (M7 hostable entry points)

## Suggested Title

`feat: scaffold cross-platform desktop AppHost with Photino.NET`
