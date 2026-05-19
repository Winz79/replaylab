# ReplayLab

ReplayLab is a .NET replay/testing toolkit for loading structured replay messages and sending them through configurable adapters. The public repo now covers the completed M1-M6 foundation: ReplayLab.Core contracts, CSV parsing, sequential replay, mock and HTTP adapters, a CLI, a local Web UI, DI registration helpers, and the extension model for private adapters outside this repository.

## What ReplayLab Is

- A small, open-source-friendly replay/testing foundation.
- A set of generic message, batch, parser, and sender contracts.
- A place for public parser and adapter packages that do not depend on business-specific systems.
- A testable scaffold for future vertical slices.

## What ReplayLab Is Not

- It is not a production replay platform yet.
- It does not include Docker assets.
- It does not include WCF, proprietary, customer-specific, certificate-specific, or business-specific adapters.
- It does not contain private mapping rules or business contract models.
- M7 owns hostable CLI and Web entry point packages.

## Current Status

ReplayLab has completed M1-M6. The solution targets `net10.0` and is pinned with `global.json` to the SDK line used for this repository.

Implemented today:

- `ReplayLab.Core` with generic replay models and contracts.
- `ReplayLab.Parsers.Csv` with a deliberately small first CSV parser slice.
- `SequentialReplayEngine` for generic replay orchestration.
- `ReplayLab.Adapters.Mock` for tests and local development.
- `ReplayLab.Adapters.Http` for minimal HTTP POST replay.
- `ReplayLab.Adapters.Example` as a fictional reference implementation for extension authors.
- DI registration helpers in parser and adapter projects.
- `ReplayLab.Cli` and `ReplayLab.Web`.
- `ReplayLab.Core` package metadata verified with `dotnet pack` at version `0.6.0`.
- GitHub Actions CI for restore, build, and test.

For the completed M6 extension model and rationale, see [ADR 0008](docs/adr/0008-extension-model.md), the [M6 milestone guide](docs/milestones/m6-private-adapter-extension-model.md), [PRD 0008](docs/prd/0008-private-adapter-extension-model.md), and [the roadmap](docs/roadmap.md).

## Architecture Overview

The dependency direction is intentionally simple:

```text
ReplayLab.Core
  ^          ^
  |          |
Parsers   Adapters
  ^
  |
Applications such as CLI and local Web UI
```

`ReplayLab.Core` owns generic contracts and models:

- `ReplayMessage`
- `ReplayBatch`
- `ReplayResult`
- `IMessageParser`
- `IReplaySender`

Parser and adapter projects depend on core. Core must not depend on parser implementations, sender implementations, UI, Docker, WCF, persistence, or business-specific packages.

## Public vs Private Adapter Boundary

Public ReplayLab adapters should stay generic and reusable. They may depend on public protocols, public data formats, or local test utilities, but they should not encode private business semantics.

Private integrations belong outside this repository. A private integration can compose ReplayLab like this:

1. Parse public input into generic `ReplayMessage` values.
2. Apply private mapping rules outside the public repository.
3. Create private business contract objects outside the public repository.
4. Send through a private adapter outside the public repository.

WCF and business-specific adapters are intentionally excluded from this repository.

## Build A Private Adapter

M6 makes private adapters a supported extension path without moving private integration code into this repo: reference `ReplayLab.Core`, implement `IReplaySender`, optionally implement `IMessageParser`, add a project-local `IServiceCollection` extension, and own the composition root in M6.

## Packageable Core

`ReplayLab.Core` is packageable and pack verified at version `0.6.0`. Packageable and pack verified does not mean published.

Create the package locally:

```powershell
dotnet pack src/ReplayLab.Core -c Release
```

Reference it from a private adapter project:

```xml
<PackageReference Include="ReplayLab.Core" Version="0.6.0" />
```

## CLI Preview

The current CLI preview accepts one CSV file path, an explicit CSV format plus
a file path, or an HTTP sender selection with an endpoint URL. It parses the
CSV input, prints a concise inspection summary, replays the messages through
the selected sender, and prints a replay summary.

Run the preview against the synthetic sample:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- samples/basic.csv
```

Or use the explicit format option:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- --format csv samples/basic.csv
```

Use the HTTP preview sender against a local endpoint:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- --sender http --endpoint-url http://localhost:5087/ samples/basic.csv
```

The HTTP preview is intentionally narrow:

- sender selection is `mock` or `http`
- mock remains the default sender
- HTTP sends `POST` requests only
- the request body is `ReplayMessage.Payload`
- the default request `Content-Type` is `application/json`
- method selection, header mapping, body mapping, response capture, auth, certificates, retries, config files, and Docker remain out of scope for the current HTTP adapter

Expected output shape:

```text
Loaded 2 message(s).
Inspected 2 message(s).
- record-1: payload 70 character(s)
- record-2: payload 70 character(s)
Sent 2 message(s): 2 succeeded, 0 failed.
- record-1: succeeded
- record-2: succeeded
```

Expected HTTP success output shape:

```text
Loaded 2 message(s).
Inspected 2 message(s).
- record-1: payload 70 character(s)
- record-2: payload 70 character(s)
Sent 2 message(s): 2 succeeded, 0 failed.
- record-1: succeeded
- record-2: succeeded
```

Expected HTTP failure output shape:

```text
Loaded 2 message(s).
Inspected 2 message(s).
- record-1: payload 70 character(s)
- record-2: payload 70 character(s)
Sent 2 message(s): 0 succeeded, 2 failed.
- record-1: failed - [platform-specific connection error]
- record-2: failed - [platform-specific connection error]
```

The CLI returns `0` when all parsed messages are replayed successfully. It
returns non-zero for command-level failures such as a missing file, invalid CSV,
unsupported input format, unsupported sender, missing endpoint URL, or replay
failures.

## Local Executable Publish

ReplayLab's current local executable distribution path is a framework-dependent
publish of `ReplayLab.Cli`. This keeps distribution local and repeatable and
assumes the required .NET runtime is available on the machine.

Publish the CLI:

```powershell
dotnet publish src/ReplayLab.Cli/ReplayLab.Cli.csproj --configuration Release --output ./artifacts/publish/replaylab
```

Run the published executable against the synthetic sample:

```powershell
./artifacts/publish/replaylab/ReplayLab.Cli.exe samples/basic.csv
```

The same explicit format command also works with the published executable:

```powershell
./artifacts/publish/replaylab/ReplayLab.Cli.exe --format csv samples/basic.csv
```

On non-Windows systems, run the apphost without the `.exe` extension:

```bash
./artifacts/publish/replaylab/ReplayLab.Cli samples/basic.csv
```

Or use the explicit format option:

```bash
./artifacts/publish/replaylab/ReplayLab.Cli --format csv samples/basic.csv
```

To publish and verify the sample output in one step:

```powershell
./eng/verify-published-cli.ps1
```

This local publish path does not add Docker images, NuGet publishing, GitHub
release automation, WCF/private adapters, persistence, or configuration DSL
support.

Both command shapes keep the mock sender as the default sender. Unsupported
formats fail early with a clear non-zero CLI error before parsing or replay.
Unsupported senders and missing HTTP endpoint URLs also fail early with exit
code `2`.

## CSV Parser Limitations

The current CSV parser is intentionally minimal. It is a first slice for loading
tiny structured replay inputs, not a complete RFC 4180 implementation.

Current behavior and limitations:

- The first non-empty, non-comment line is treated as the header row.
- Blank lines are ignored.
- Lines whose first non-whitespace character is `#` are ignored as comments.
- Each data row is split on commas.
- Quoted fields are not supported in the first slice and are rejected.
- Escaped quotes are not supported.
- Embedded commas inside fields are not supported.
- Embedded newlines inside fields are not supported.
- Full RFC 4180 compliance is not currently supported.
- Header names are used as JSON property names exactly as written.
- Duplicate header names are not detected; later values overwrite earlier values in the generated payload object.
- All payload values are serialized as strings.
- Each parsed message uses the whole CSV row as a JSON payload object.
- `ReplayMessage.Headers` remains empty by default.
- Dynamic header mapping from CSV columns is deferred to a later mapping/configuration feature.
- Private, proprietary, customer-specific, and business-specific mappings are out of scope for this repository.

Synthetic sample files live in `samples/`. They are generic examples only and
avoid quoted fields, embedded commas, and business-specific data.

## Build and Test

Use the pinned SDK from `global.json`:

```powershell
dotnet --info
dotnet restore ReplayLab.sln
dotnet build ReplayLab.sln --configuration Release --no-restore
dotnet test ReplayLab.sln --configuration Release --no-build
```

For a single command during local development:

```powershell
dotnet test ReplayLab.sln
```
