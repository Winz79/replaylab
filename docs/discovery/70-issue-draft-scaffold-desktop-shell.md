# Issue Draft: Scaffold WinUI 3 Desktop Shell with WebView2

> Parent discovery issue: #70
> Derived from: `docs/discovery/70-desktop-apphost-discovery.md`

---

## Goal

Build a minimal WinUI 3 desktop application that embeds the ReplayLab Web UI inside
WebView2 by self-hosting ASP.NET Core in-process via the existing M7 hostable Web
seam (`ReplayLab.Web.Hosting`).

## Scope

1. Create `src/ReplayLab.Desktop` — WinUI 3 packaged desktop project targeting `net10.0`.
2. Add package references:
   - `Microsoft.WindowsAppSDK` / WinUI 3 SDK
   - `Microsoft.Web.WebView2`
   - `Microsoft.AspNetCore.App` (or host builder packages)
   - `ReplayLab.Web.Hosting`
3. Implement minimal shell:
   - `App.xaml` + `App.xaml.cs` (WinUI 3 entry)
   - `MainWindow.xaml` + `MainWindow.xaml.cs`
   - In-process `WebApplication` bootstrap on a free loopback port
   - WebView2 navigation to the discovered local URL after host is listening
   - Graceful host shutdown on window close
4. Register `ReplayLab.Desktop` in `ReplayLab.sln`.
5. Add a smoke / integration test proving the host starts and WebView2 navigates.

## Acceptance Criteria

- [ ] `dotnet build ReplayLab.sln` includes the desktop project and passes.
- [ ] Desktop app launches and renders the ReplayLab Web UI inside the window.
- [ ] Close button stops the Kestrel host cleanly (no process leaks).
- [ ] Smoke test passes on Windows.
- [ ] `README.md` and roadmap docs reference the new project accurately.

## Out of Scope

- Cross-platform desktop shell.
- Single-file / self-contained publishing.
- WebView2 runtime bundling (assume preinstalled or evergreen).
- Private adapter registration in the public desktop shell (uses M7 safe defaults).
- Editable grid, parser quality, HTTP sender UI changes.
- Installer creation (MSIX, WiX, etc.).
- NuGet packaging of the desktop project.

## Test Expectations

- Focused test: start the desktop host, assert the local HTTP endpoint responds,
  assert WebView2 `Source` matches the local URL.
- Keep test lightweight; avoid full UI automation where possible.

## Risks

- WinUI 3 project system adds build complexity; verify CI can build it.
- WebView2 runtime may be missing on some dev machines; document requirement.
- Kestrel port discovery may race during startup; probe availability before binding.

## Linked Docs

- `docs/discovery/70-desktop-apphost-discovery.md`
- `docs/plans/m8-desktop-apphost.md`
- ADR 0009 (M7 hostable entry points)

## Suggested Title

`feat: scaffold WinUI 3 desktop AppHost with WebView2 and self-hosted Web UI`
