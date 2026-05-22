# M8: Desktop AppHost with WebView2

## Status

Current milestone.

## Goal

Build a desktop AppHost that launches and embeds the ReplayLab Web UI inside
WebView2, while the desktop shell owns startup, shutdown, windowing, and
composition-root responsibilities.

## User Value

- Users can launch ReplayLab as a local desktop tool without manually running a
  CLI command or managing a browser-hosted Web app.
- Private hosts can package a ReplayLab-powered local workbench with a clearer
  launch and lifecycle model.
- The product shell can make ReplayLab feel like a tool rather than a developer
  sample.

## Context From M7

M7 delivered:

- `ReplayLab.Cli.Hosting`
- `ReplayLab.Web.Hosting`
- external host sample
- consumption model docs

The M7 closeout also shipped Web external composition (PR #87), decoupling the
Web parser workflow from CSV-specific assumptions via `IWebReplayParser`. This
means the Web UI is now fully host-provided for parser, sender, and workflow
services.

## Scope

- Introduce `ReplayLab.Desktop` as a WinUI 3 packaged desktop project.
- Self-host ASP.NET Core / Kestrel in-process from the desktop executable.
- Embed WebView2 and navigate it to the self-hosted local URL.
- Own window lifecycle, startup, and shutdown coordination.
- Use dynamic loopback port selection to avoid collision.
- Keep the desktop surface generic; no business-specific adapters or private
  data models in the public repo.
- Add smoke test proving startup and navigation work.

## Explicit Non-Goals

- Cross-platform desktop shell (remains a future candidate).
- WebView2 runtime bundling (assume preinstalled or evergreen installer).
- Private adapter registration inside the public desktop shell.
- RFC-compliant CSV parser strategy.
- Editable Web grid behavior.
- Package publishing or release automation.
- Installer creation.
- GitHub Actions release automation.

## Completed Work

- Photino.NET desktop shell scaffolding (`ReplayLab.Desktop`).
- In-process Kestrel self-host with dynamic loopback port.
- Native web view navigation to the local server.
- Graceful shutdown on window close.
- Smoke test asserting the Web host starts and responds.
- Self-contained single-file publishing support for `win-x64`, `linux-x64`, and `osx-x64`.
- Publish verification script (`eng/verify-published-desktop.ps1`).
- Updated `README.md` with Desktop publish instructions.

## Candidate Architecture

```text
ReplayLab.Core
   ^
   |
Parsers / Adapters / Replay Engine
   ^
   |
ReplayLab.Web.Hosting  (M7 seam)
   ^
   |
ReplayLab.Desktop  (WinUI 3 + WebView2 + in-process Kestrel)
```

The desktop app:
1. Creates a `WebApplicationBuilder`.
2. Registers default parser/sender services (CSV + mock, same as public Web).
3. Calls `AddReplayLabWeb()` and `MapReplayLabWeb()`.
4. Starts Kestrel on a free loopback port.
5. Navigates WebView2 to `http://localhost:<port>`.
6. On window close, stops the Web host cleanly.

## Candidate Vertical Slices

### Slice 1: Scaffold WinUI 3 desktop project

- Create `src/ReplayLab.Desktop` (WinUI 3 packaged desktop).
- Add WebView2 and ASP.NET Core hosting package references.
- Add minimal `App.xaml`, `MainWindow.xaml`, and entry code.
- Register in `ReplayLab.sln`.
- Verify `dotnet build` passes.

### Slice 2: Self-host ReplayLab Web inside the desktop process

- Implement in-process Kestrel bootstrap on a free loopback port.
- Integrate `ReplayLab.Web.Hosting` (`AddReplayLabWeb`, `MapReplayLabWeb`).
- Expose the discovered local URL to the window code.
- Add smoke test asserting the host starts and responds.

### Slice 3: Embed WebView2 and wire navigation

- Add WebView2 control to `MainWindow.xaml`.
- Navigate to the self-hosted URL after host is ready.
- Handle graceful shutdown on window close.
- Add UI smoke test proving WebView2 reaches the local endpoint.

### Slice 4: Document the new boundary

- Update `README.md` to reference `ReplayLab.Desktop`.
- Update `docs/roadmap.md` when M8 is complete.
- Document WebView2 runtime expectations.

## ADR Need

Create `docs/adr/0010-desktop-apphost-strategy.md` to record:
- WinUI 3 vs WPF vs cross-platform decision
- In-process Kestrel vs alternative bridge
- Dynamic port vs fixed port
- Ownership boundary between desktop shell and Web app

## Success Criteria

- `ReplayLab.Desktop` compiles and runs from Visual Studio or `dotnet run`.
- The desktop window shows the ReplayLab Web UI via WebView2.
- Close button shuts down the Kestrel host without leaks.
- Smoke test passes on Windows.
- Docs describe the new project and its runtime expectations.

## Risks

- WinUI 3 project system adds build complexity; CI may need Windows SDK.
- WebView2 runtime missing on target machines; document requirement clearly.
- Kestrel port discovery race during startup; probe before bind.
- Desktop tests requiring UI automation can be flaky; keep smoke tests minimal.

## Dependency On Previous Work

- Requires M7 hostable Web entry points (`ReplayLab.Web.Hosting`).
- Benefits from M7 Web external composition (`IWebReplayParser`) so the desktop
  shell can use host-provided parser services cleanly.
