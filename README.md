# ReplayLab

ReplayLab is an early-stage .NET toolkit for loading structured replay messages and sending them through configurable adapters. The current repository is focused on the public foundation: generic core contracts, a CSV parser slice, a mock sender adapter, tests, and a first CLI preview.

## What ReplayLab Is

- A small, open-source-friendly replay/testing foundation.
- A set of generic message, batch, parser, and sender contracts.
- A place for public parser and adapter packages that do not depend on business-specific systems.
- A testable scaffold for future vertical slices.

## What ReplayLab Is Not

- It is not a production replay engine yet.
- It does not provide a UI.
- It does not provide Docker assets.
- It does not include an HTTP sender.
- It does not include WCF, proprietary, customer-specific, certificate-specific, or business-specific adapters.
- It does not contain private mapping rules or business contract models.

## Current Status

ReplayLab is a foundation scaffold. The solution currently targets `net10.0` and is pinned with `global.json` to the installed .NET SDK line used for this repository.

Implemented today:

- `ReplayLab.Core` with generic replay models and contracts.
- `ReplayLab.Parsers.Csv` with a deliberately small first CSV parser slice.
- `ReplayLab.Adapters.Mock` with a sender adapter for tests and local development.
- `ReplayLab.Cli` with a first usable CSV-to-mock-sender preview flow.
- xUnit tests for core, the CSV parser, and the mock adapter.
- GitHub Actions CI for restore, build, and test.

## Architecture Overview

The dependency direction is intentionally simple:

```text
ReplayLab.Core
  ^          ^
  |          |
Parsers   Adapters
  ^
  |
Applications such as CLI or future composition hosts
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

## CLI Preview

The current CLI preview accepts either one CSV file path or an explicit CSV
format plus a file path, parses it with the CSV parser, prints a concise
inspection summary, replays the messages through the mock sender, and prints a
replay summary.

Run the preview against the synthetic sample:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- samples/basic.csv
```

Or use the explicit M3 format option:

```powershell
dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- --format csv samples/basic.csv
```

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

The CLI returns `0` when all parsed messages are replayed successfully. It
returns non-zero for command-level failures such as a missing file, invalid CSV,
unsupported input format, or replay failures.

## Local Executable Publish

ReplayLab's first local executable distribution path is a framework-dependent
publish of `ReplayLab.Cli`. This keeps M2 focused on a local, repeatable output
folder and assumes the required .NET runtime is available on the machine.

Publish the CLI:

```powershell
dotnet publish src/ReplayLab.Cli/ReplayLab.Cli.csproj --configuration Release --output ./artifacts/publish/replaylab
```

Run the published executable against the synthetic sample:

```powershell
./artifacts/publish/replaylab/ReplayLab.Cli.exe samples/basic.csv
```

The same M3 explicit format command also works with the published executable:

```powershell
./artifacts/publish/replaylab/ReplayLab.Cli.exe --format csv samples/basic.csv
```

On non-Windows systems, run the apphost without the `.exe` extension:

```bash
./artifacts/publish/replaylab/ReplayLab.Cli samples/basic.csv
```

Or use the explicit M3 format option:

```bash
./artifacts/publish/replaylab/ReplayLab.Cli --format csv samples/basic.csv
```

To publish and verify the sample output in one step:

```powershell
./eng/verify-published-cli.ps1
```

This local publish path does not add Docker images, NuGet publishing, GitHub
release automation, a Web UI, HTTP senders, WCF/private adapters, persistence,
or configuration DSL support.

Both command shapes keep the mock sender as the default sender. Unsupported
formats fail early with a clear non-zero CLI error before parsing or replay.

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
