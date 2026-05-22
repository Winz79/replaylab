# M8: Desktop AppHost with Photino.NET

## Status

Current milestone.

## Goal

Build a desktop AppHost that launches and embeds the ReplayLab Web UI inside a
native web view, while the desktop shell owns startup, shutdown, windowing, and
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

- Introduce `ReplayLab.Desktop` as a Photino.NET desktop project.
- Self-host ASP.NET Core / Kestrel in-process from the desktop executable.
- Embed the platform-native web view and navigate it to the self-hosted local URL.
- Own window lifecycle, startup, and shutdown coordination.
- Use dynamic loopback port selection to avoid collision.
- Keep the desktop surface generic; no business-specific adapters or private
  data models in the public repo.
- Add smoke test proving startup and hosted Web response work.

## Explicit Non-Goals

- WebView/runtime bundling beyond platform prerequisites.
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
- Self-contained publish configuration for `win-x64`, `linux-x64`, and `osx-x64`.
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
ReplayLab.Desktop  (Photino.NET + native web view + in-process Kestrel)
```

The desktop app:
1. Creates a `WebApplicationBuilder`.
2. Registers default parser/sender services (CSV + mock, same as public Web).
3. Calls `AddReplayLabWeb()` and `MapReplayLabWeb()`.
4. Starts Kestrel on a free loopback port.
5. Navigates the embedded native web view to `http://localhost:<port>`.
6. On window close, stops the Web host cleanly.

## Candidate Vertical Slices

### Slice 1: Scaffold Photino.NET desktop project

- Create `src/ReplayLab.Desktop` (Photino.NET desktop project).
- Add Photino.NET and ASP.NET Core hosting package references.
- Add minimal entry code.
- Register in `ReplayLab.sln`.
- Verify `dotnet build` passes.

### Slice 2: Self-host ReplayLab Web inside the desktop process

- Implement in-process Kestrel bootstrap on a free loopback port.
- Integrate `ReplayLab.Web.Hosting` (`AddReplayLabWeb`, `MapReplayLabWeb`).
- Expose the discovered local URL to the window code.
- Add smoke test asserting the host starts and responds.

### Slice 3: Embed the native web view and wire navigation

- Create the Photino window and navigate to the self-hosted URL after host is ready.
- Handle graceful shutdown on window close.
- Keep public tests focused on bootstrap/address discovery rather than full UI automation.

### Slice 4: Document the new boundary

- Update `README.md` to reference `ReplayLab.Desktop`.
- Update `docs/roadmap.md` when M8 is complete.
- Document Windows WebView2 and Linux WebKitGTK runtime expectations.

## ADR Need

Create `docs/adr/0010-desktop-apphost-strategy.md` to record:
- Photino.NET vs WinUI 3 vs WPF decision
- In-process Kestrel vs alternative bridge
- Dynamic port vs fixed port
- Ownership boundary between desktop shell and Web app

## Success Criteria

- `ReplayLab.Desktop` compiles and runs from Visual Studio or `dotnet run`.
- The desktop window shows the ReplayLab Web UI via the native web view.
- Close button shuts down the Kestrel host without leaks.
- Smoke test passes at the desktop bootstrap seam.
- Docs describe the new project and its runtime expectations.

## Risks

- Linux machines may miss `libwebkit2gtk-4.0`; document requirement clearly.
- WebView2 runtime may be missing on target Windows machines; document requirement clearly.
- Kestrel port discovery race during startup; probe before bind.
- Full desktop UI automation can be flaky; keep public smoke tests minimal.

## Dependency On Previous Work

- Requires M7 hostable Web entry points (`ReplayLab.Web.Hosting`).
- Benefits from M7 Web external composition (`IWebReplayParser`) so the desktop
  shell can use host-provided parser services cleanly.
