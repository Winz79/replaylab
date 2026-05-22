# ADR 0011: Desktop Hosting Seam — Extracted

## Status

Accepted

## Context

Issue #101 asks whether ReplayLab should extract a reusable `ReplayLab.Desktop.Hosting` package so that external consumers can compose a custom Desktop replay tool the same way they compose CLI and Web hosts today.

M10B (NuGet-based custom replay tool sample) proved that the static package/reference composition path works for Web hosting. The open question was whether Desktop needs the same first-class library seam.

## Evaluation

### Attempted extraction

The current `ReplayLab.Desktop` project was a thin executable:

- Creates a `WebApplicationBuilder`.
- Configures a dynamic loopback URL (`http://127.0.0.1:0`).
- Calls `AddReplayLabWeb()` from `ReplayLab.Web.Hosting`.
- Wires ASP.NET Core middleware (`StaticFiles`, `Routing`, `MapStaticAssets`, `MapReplayLabWeb`).
- Starts Kestrel, reads the bound address, and opens a Photino.NET window.
- Handles graceful shutdown on window close.

This bootstrap was ~50 lines. After attempting to extract it:

- The seam is small and obvious.
- The public surface is a single static class: `ReplayLabDesktopHost`.
- External consumers can provide service registrations via `Action<IServiceCollection>`.
- The Photino-specific window lifecycle remains internal.
- No speculative options or configuration model is needed.

### Why extraction is viable now

- The Web sample already proves the package consumer story; Desktop is the last missing hostable surface.
- The extracted library is ~70 lines and introduces no new public abstractions beyond `BuildApp`, `GetLocalUrl`, and `Run`.
- Keeping the Photino window creation internal avoids exposing unstable UI configuration surface.
- The static-asset manifest path resolution is encapsulated in the library; consumers do not need to know about it.

## Decision

Extract `ReplayLab.Desktop.Hosting` as a small, reusable library.

### What the library owns

- `ReplayLabDesktopHost.BuildApp(args, configureServices)` — builds a `WebApplication` with the ReplayLab middleware pipeline, dynamic loopback port, and static asset handling.
- `ReplayLabDesktopHost.GetLocalUrl(app)` — reads the bound address from `IServerAddressesFeature`.
- `ReplayLabDesktopHost.Run(args, configureServices)` — full lifecycle: build app, start Kestrel, open Photino window, wait for close, stop host.

### What the public Desktop app does now

`ReplayLab.Desktop` remains the executable app. It calls:

```csharp
ReplayLabDesktopHost.Run(args);
```

with no custom registrations, acting as the default/reference consumer.

### Consumer API

An external consumer can build a custom Desktop replay tool by referencing `ReplayLab.Desktop.Hosting` and providing service registrations:

```csharp
ReplayLabDesktopHost.Run(args, services =>
{
    services.AddSingleton<IMessageParser, MyParser>();
    services.AddSingleton<IReplaySender, MySender>();
});
```

This mirrors the CLI and Web hosting patterns already established in ADR 0009.

## Consequences

- `ReplayLab.Desktop.Hosting` is added to the packageable project set.
- `ReplayLab.Desktop` loses its direct references to `ReplayLab.Web.Hosting`, `ReplayLab.Parsers.Csv`, and `ReplayLab.Adapters.Mock`; it now depends only on `ReplayLab.Desktop.Hosting`.
- `DesktopBootstrap.cs` is removed; its logic lives in `ReplayLabDesktopHost`.
- External consumers can ship a Desktop replay tool without copying executable app code.
- No dynamic plugins, persistence, installer work, or business-specific concepts are introduced.

## Related

- #101 — Extract reusable Desktop hosting seam
- ADR 0010 — Desktop AppHost Strategy
- ADR 0009 — Hostable Entry Points
- M10B — NuGet-based Custom Desktop/Web Tool Sample
