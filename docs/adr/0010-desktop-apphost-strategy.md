# ADR 0010: Desktop AppHost Strategy

## Status

Accepted

## Context

M7 completed hostable entry points (ADR 0009), making the Web UI reusable via
`ReplayLab.Web.Hosting`. The roadmap accepted M8 as a Desktop AppHost that
wraps the Web UI inside a native OS window while self-hosting ASP.NET Core
in-process.

Issue #70 raised the open questions:
- Windows-only vs cross-platform scope
- Desktop framework choice (WPF, WinUI 3, or cross-platform)
- Self-hosted Web UI vs alternative bridge model
- Fixed vs dynamic loopback port
- Packaging and runtime distribution expectations

## Problem

ReplayLab needs a desktop shell that makes the Web UI accessible without
requiring users to run CLI commands or manually start the Web app. The open
question is which framework and hosting model deliver this with the right
balance of scope, portability, and architectural alignment.

## Decision Direction

Use **Photino.NET** as the desktop shell framework, with in-process Kestrel on a
dynamic loopback port, navigating a native web view to the local URL.

### Why Photino.NET

Photino.NET is a lightweight .NET wrapper around native OS web views:
- **Windows**: Edge WebView2 (same engine as WinUI 3)
- **Linux**: WebKitGTK (system webkit2gtk package)
- **macOS**: WebKit (system framework)

It provides the cross-platform desktop window and web view without requiring
platform-specific UI frameworks, XAML compilers, or Windows-only build tooling.

### Why Not WinUI 3 / WPF

WinUI 3 was the initial candidate, but it is Windows-only and requires the
Windows App SDK + XAML compiler, which cannot build in WSL or non-Windows CI
environments. WPF is similarly Windows-only and represents a legacy framework
with no forward roadmap alignment.

Photino.NET delivers the same WebView2 experience on Windows while also covering
Linux and macOS, with a simpler project structure that builds anywhere.

### Hosting Model

In-process Kestrel via `WebApplication.CreateBuilder` reuses the existing M7
`ReplayLab.Web.Hosting` seam directly:

```text
ReplayLab.Core
   ^
   |
ReplayLab.Web.Hosting (M7 seam)
   ^
   |
ReplayLab.Desktop (Photino window + in-process Kestrel)
```

The desktop app:
1. Creates a `WebApplicationBuilder`.
2. Calls `builder.WebHost.UseUrls("http://127.0.0.1:0")` for dynamic port.
3. Registers default services via `AddReplayLabWeb()`.
4. Maps endpoints via `MapReplayLabWeb()`.
5. Starts the host with `app.StartAsync()`.
6. Reads the bound address from `IServerAddressesFeature`.
7. Opens a Photino window and navigates to the discovered URL.
8. On window close, calls `app.StopAsync()`.

### Port Strategy

Dynamic loopback port (`127.0.0.1:0`) is preferred over a fixed port because:
- No collision with other services.
- Multiple instances can run concurrently.
- No firewall prompts or admin privileges required.

### Composition Boundary

The desktop shell remains generic:
- Uses the same safe defaults as `ReplayLab.Web` (CSV parser + mock sender).
- Does not register private adapters or business-specific services.
- Private hosts can later build their own desktop composition root by referencing
  `ReplayLab.Web.Hosting` directly, just as they do for CLI and Web today.

## Options Considered

| Option | Pros | Cons | Decision |
| --- | --- | --- | --- |
| **Photino.NET** | Cross-platform; native web views; builds anywhere; reuses M7 seam | Requires runtime packages on Linux | **Accepted** |
| **WinUI 3 + WebView2** | Modern Windows-native; same web engine | Windows-only; XAML compiler; WSL/CI-unfriendly | Rejected |
| **WPF + WebView2** | Mature Windows desktop | Windows-only; legacy framework; no future alignment | Rejected |
| **Avalonia + web control** | Cross-platform; .NET-native UI | Web control quality varies; adds new UI framework | Rejected |
| **MAUI Blazor Hybrid** | Microsoft-supported; cross-platform | Heavy project system; Blazor not needed | Rejected |
| **Fixed localhost port** | Simple URL construction | Collision risk; firewall issues | Rejected |
| **In-memory/native bridge** | No TCP surface | Requires new abstraction; breaks M7 seam | Rejected |

## Consequences

- `ReplayLab.Desktop` builds and runs on Windows, Linux, and macOS.
- The M7 hostable Web seam is exercised by a third entry point (Desktop), validating its generality.
- No Windows-only build tooling is required, keeping CI and WSL development possible.
- Linux users need `libwebkit2gtk-4.0` installed; this is documented in `README.md`.
- The desktop shell is thin; most logic remains in `ReplayLab.Web.Hosting` and `ReplayLab.Core`.

## Explicit Non-Goals

- WebView2 runtime bundling.
- Private adapter registration inside the public desktop shell.
- Installer creation (MSIX, WiX, etc.).
- Cross-platform desktop shell alternatives beyond Photino.NET.
- GitHub Actions release automation.

## Resulting Guidance

- Desktop shell work should use Photino.NET for window and web view management.
- Web hosting should reuse `ReplayLab.Web.Hosting` via in-process Kestrel.
- Dynamic port selection is the default; fixed ports require explicit justification.
- The desktop project should remain a thin composition root, not absorb Web UI or
  business-specific logic.
- Self-contained single-file publishing is supported for `win-x64`, `linux-x64`, and `osx-x64`.
- Future packaging work should be a separate slice after the desktop shell is
  proven.
