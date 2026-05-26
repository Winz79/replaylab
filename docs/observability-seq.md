# Seq Observability Guide

This guide walks you through connecting ReplayLab to Seq — a local, self-hosted
structured log server. When you finish, every log event emitted by the replay
engine (`SequentialReplayEngine`), the CSV parser (`CsvParser`), and the HTTP
sender (`HttpSender`) will appear in the Seq UI with structured properties you
can filter, search, and correlate.

ReplayLab uses `Microsoft.Extensions.Logging` (`ILogger<T>`) throughout. Seq is
one consumer of those logs. The same abstractions work with other providers
(Loki, ELK, Datadog — see [Alternatives](#6-alternatives)).

> **Precondition:** This guide assumes the Docker Compose stack from `docker-compose.yml`
> is already running (set up in issues #141 and #152). The Seq container must be
> reachable at `http://localhost` (UI) and `http://localhost:5341` (ingest API).
> The Web app must be running from source so you can add the NuGet package and
> rebuild.

## 1. Prerequisites

- **Docker host** — running ReplayLab Web + Seq via `docker compose up`. The
  Seq service exposes port 80 (UI) and port 5341 (ingest API).
- **.NET 10 SDK** — version `10.0.203` or later (pinned in
  [`global.json`](../global.json)).
- **Source checkout** — you will add a NuGet package to
  `src/ReplayLab.Web` and rebuild.

## 2. Add the Seq logging provider

Add the `Seq.Extensions.Logging` package to the Web project:

```bash
dotnet add src/ReplayLab.Web/ReplayLab.Web.csproj package Seq.Extensions.Logging
```

> `Seq.Extensions.Logging` targets `netstandard2.0`, which is compatible with
> `net10.0`. If you see a version conflict warning, pin a specific version:
>
> ```bash
> dotnet add src/ReplayLab.Web/ReplayLab.Web.csproj package Seq.Extensions.Logging --version 8.0.0
> ```

## 3. Activate Seq logging

ReplayLab uses a runtime opt-in pattern: if the `SEQ_SERVER_URL` environment
variable is set, the application automatically sends logs to Seq. If the
variable is absent, behavior is unchanged — logs go to the console only.

### Docker Compose (automatic)

When running the full stack with `docker compose up`, the `web` service already
has `SEQ_SERVER_URL=http://seq:5341` set in `docker-compose.yml`. No additional
configuration is needed — Seq logging activates automatically.

### Running outside Docker (manual)

Set the environment variable before starting the application:

```bash
SEQ_SERVER_URL=http://localhost:5341 dotnet run --project src/ReplayLab.Web/ReplayLab.Web.csproj
```

> **Port 5341** is the Seq ingest API endpoint (default). The UI is on port 80.
> The application sends structured logs to the ingest endpoint.

### SDK consumers

If you are embedding ReplayLab in your own application, set the `SEQ_SERVER_URL`
environment variable before application startup. The reference implementations
(Web, CLI, Desktop) check for this variable and call `AddSeq` when it is set.
You can replicate this pattern in your own `Program.cs`:

```csharp
var seqUrl = Environment.GetEnvironmentVariable("SEQ_SERVER_URL");
if (!string.IsNullOrWhiteSpace(seqUrl))
{
    builder.Logging.AddSeq(seqUrl);
}
```

No `appsettings.json` changes are needed — the Seq server URL is controlled
entirely through the environment variable.

## 4. Verifying Seq is connected

The call to `AddSeq` registers Seq as an additional `ILoggerProvider`. All
`ILogger<T>` instances resolved by the container will now send structured logs
to Seq in addition to the console.

## 5. Verify Seq is receiving logs

### Step 1: Rebuild

```bash
dotnet build src/ReplayLab.Web/ReplayLab.Web.csproj
```

### Step 2: Start the stack

If Seq is not already running:

```bash
docker compose up seq
```

### Step 3: Run the Web app

```bash
dotnet run --project src/ReplayLab.Web/ReplayLab.Web.csproj
```

### Step 4: Open the Seq UI

Navigate to `http://localhost` in your browser. Seq may show an initial setup
screen the first time — accept the defaults.

### Step 5: Generate replay activity

Open the ReplayLab Web UI (the URL printed in the terminal, typically
`http://localhost:5213`). Upload a CSV file, inspect the parsed messages, and
click **Replay Selected** to trigger a replay run.

### Step 6: Check the Seq stream

Return to the Seq UI (`http://localhost`). Click the **Stream** tab in the
sidebar. You should see log events appearing in real time. Use the search bar to
filter:

| Filter | What it shows |
| --- | --- |
| `@Level = 'Information'` | All Info-level events |
| `@Level = 'Warning'` | Warnings only |
| `@Level = 'Error'` | Errors only |
| `MessageId` | Events with a specific message ID |
| `SourceContext` | Events from a specific class |

Click any log event to expand it and inspect its structured properties
(`MessageCount`, `SuccessCount`, `FailureCount`, `TotalElapsedMs`, `MessageId`, `StatusCode`, etc.).

## 6. What you should see

The following log events are already emitted by the replay pipeline via
`ILogger<T>`. Once Seq is connected, every one of them appears as a structured
event in the Seq UI.

### SequentialReplayEngine

| Event | Level | Template |
| --- | --- | --- |
| Replay started | Information | `Starting sequential replay of {MessageCount} messages` |
| Message canceled | Warning | `Message {MessageId} was canceled` |
| Message failed | Error | `Message {MessageId} failed: {ErrorMessage}` |
| Replay complete | Information | `Replay complete: {SuccessCount} succeeded, {FailureCount} failed out of {TotalMessages} in {TotalElapsedMs}ms` |

### CsvReplayMessageParser

| Event | Level | Template |
| --- | --- | --- |
| Parse started | Information | `Starting CSV parse` |
| Row mismatch | Warning | `CSV row {RawRow} has {ActualFieldCount} fields but header has {HeaderFieldCount} fields` |
| Parse complete | Information | `CSV parse complete: {RecordCount} messages from {TotalRows} rows` |

### HttpReplaySender

| Event | Level | Template |
| --- | --- | --- |
| Non-2xx response | Warning | `HTTP POST {MessageId} returned {StatusCode} ({ReasonPhrase})` |
| Send failure | Error | `HTTP POST {MessageId} failed: {ErrorMessage}` |

> At `Debug` level, additional events are emitted: individual message send
> start/completion, HTTP POST details, and per-row CSV parse details. To enable
> Debug events, change `"Default": "Information"` to `"Default": "Debug"` in
> `appsettings.json`. Be aware that Debug output is verbose and is best used
> temporarily for investigation.

## 7. Alternatives

Seq is the recommended local observability tool for ReplayLab because it is
lightweight, self-hosted, and Docker-native. If your environment already has a
different log aggregator, the same `ILogger<T>` infrastructure works with these
providers:

| Provider | NuGet package | Notes |
| --- | --- | --- |
| **Loki / Grafana** | `Serilog.Sinks.Grafana.Loki` | Good fit if you already run Grafana. Use via Serilog bridge. |
| **Elasticsearch / ELK** | `Serilog.Sinks.Elasticsearch` | Mature ecosystem. Heavier than Seq for local use. |
| **Datadog** | `Serilog.Sinks.Datadog.Logs` | Cloud-hosted. Requires a Datadog agent or API key. |
| **OpenTelemetry** | `OpenTelemetry.Extensions` | Future path for multi-provider observability. Not yet integrated with ReplayLab, but the `ILogger<T>` abstraction is compatible with OTel log bridges. |

To switch providers, replace the `AddSeq` call with the equivalent Serilog or
OTel registration. The log events themselves do not change — only the sink
changes.

## Troubleshooting

### Seq is not receiving any events

Check the following:

1. **Is Seq running?** `docker compose ps` should show the `seq` service as `Up`.
2. **Is port 5341 reachable?** `curl http://localhost:5341/api/events?clef` should
   not return an error.
3. **Is `AddSeq` called before `builder.Build()`?** The provider must be
   registered during host construction.
4. **Is the `ServerUrl` correct?** In `appsettings.json`, the `Seq.ServerUrl`
   must use the ingest port (`5341`), not the UI port (`80`).

### No log events appear, but Seq is running

Check the log level configuration. If `"Default"` is set to `"None"` or
`"Warning"`, Information-level events from the replay engine and parser will not
be emitted. Set `"Default": "Information"` at minimum, or `"Debug"` for more
detail.

### Package version mismatch

If `dotnet add package Seq.Extensions.Logging` produces a NuGet error about
incompatible frameworks:

1. The package targets `netstandard2.0`, which is compatible with `net10.0`.
   This is not a framework incompatibility — check your NuGet source
   configuration.
2. Pin an explicit compatible version:
   ```bash
   dotnet add src/ReplayLab.Web/ReplayLab.Web.csproj package Seq.Extensions.Logging --version 8.0.0
   ```

### Events appear but are missing structured properties

If events show up in Seq but clicking them does not reveal structured properties
like `MessageId` or `StatusCode`, the logging provider may not be receiving the
source context. Verify:

- `AddSeq` is called on `builder.Logging` (not a standalone logging factory).
- The `ILogger<T>` instances are resolved from the DI container (the replay
  engine, parser, and sender all accept `ILogger<T>?` constructor parameters).

### Seq UI says "No connection could be made"

If you see a connection error when opening `http://localhost`, the Seq container
may have stopped or the UI port may be in use. Run `docker compose up seq` and
verify with `docker compose logs seq`.

## What's next

- **Cloudflare Tunnel** — [docs/cloudflare-tunnel.md](cloudflare-tunnel.md)
  exposes the Web UI and Seq UI over HTTPS through Cloudflare Tunnel.
- **Deploy workflow** — The [deploy-web workflow](../.github/workflows/deploy-web.yml)
  automates Docker image builds and deployment.
- **Architecture** — [docs/architecture.md](architecture.md) explains how
  parsers, adapters, and hosting layers compose.
- **Getting started** — [docs/getting-started.md](getting-started.md) guides SDK
  consumers through building their own replay tool.
