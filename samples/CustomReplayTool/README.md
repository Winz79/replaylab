# CustomReplayTool Sample

This sample demonstrates the ReplayLab developer adoption story:

1. Reference ReplayLab through local NuGet packages (not project references).
2. Provide a custom message parser.
3. Provide a custom replay sender.
4. Compose ReplayLab Web hosting into a minimal ASP.NET Core application.

## Prerequisites

From the repository root, produce the local NuGet packages:

```powershell
./eng/pack-local.ps1
```

If PowerShell is unavailable, run the equivalent `dotnet pack` commands for:
`ReplayLab.Core`, `ReplayLab.Parsers.Csv`, `ReplayLab.Adapters.Mock`,
`ReplayLab.Adapters.Http`, `ReplayLab.Cli.Hosting`, and `ReplayLab.Web.Hosting`.

## Build and Run

```powershell
cd samples/CustomReplayTool
dotnet restore CustomReplayTool.slnx
dotnet build CustomReplayTool.slnx --configuration Release
dotnet run --project src/CustomReplayTool.Web/CustomReplayTool.Web.csproj
```

The Web app starts on `http://localhost:5500` by default.

Open the URL in a browser, choose the sample file `samples/tickets.replay`,
and select rows to send. Because the custom sender is local and deterministic,
nothing is transmitted to an external system.

## Custom Parser

`TicketReplayMessageParser` implements `IMessageParser` from `ReplayLab.Core`.

It reads the custom `.tickets` text format:

```text
ticketId|operation|target|description
```

Example:

```text
TCK-001|Create|Alpha|Create alpha item
TCK-002|Update|Beta|Update beta item
```

Each valid line becomes a `ReplayMessage` with a JSON payload containing the
four fields. Lines that are empty, start with `#`, or do not have exactly four
fields are skipped.

## Custom Sender

`TicketReplaySender` implements `IReplaySender` from `ReplayLab.Core`.

It inspects the `operation` field in the message payload:

- If the operation is `Fail`, the sender returns a failed result.
- Otherwise, it returns a successful result.

This makes the sample fully deterministic and safe to run locally.

## Package Consumption Note

This sample uses `PackageReference` to consume ReplayLab packages from the local
feed at `../../artifacts/packages`. It does **not** use `ProjectReference` to the
ReplayLab source projects. This proves the external consumer story.

## Desktop Hosting

A reusable Desktop hosting seam for external tools is tracked by issue #101.
It is intentionally out of scope for this sample.
