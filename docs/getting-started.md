# Getting Started with ReplayLab

This guide walks you from zero to your first custom replay tool using ReplayLab
packages from GitHub Packages. When you are done, you will have a working Web UI
that parses your own input format and replays messages through your own sender.

## 1. Prerequisites

- **.NET 10 SDK** — version `10.0.203` or later (see the repository
  [`global.json`](../global.json) for the pinned version).
- **GitHub account** with a Personal Access Token (PAT) scoped to
  `read:packages`. Create one at
  [github.com/settings/tokens](https://github.com/settings/tokens/new?scopes=read:packages&description=ReplayLab).

## 2. Add the GitHub Packages NuGet source

ReplayLab SDK packages are published to GitHub Packages. Add the source once on
your machine:

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/sebastienwitz/index.json" `
  --name github-replaylab `
  --username <your-github-username> `
  --password <your-github-token> `
  --store-password-in-clear-text
```

> **Note:** Replace `<your-github-username>` and `<your-github-token>` with your
> GitHub credentials. The `--store-password-in-clear-text` flag is required
> because GitHub Packages does not support the encrypted credential store.

## 3. Create your project and add PackageReference

Create a new ASP.NET Core Web project and a class library for your custom
parser and sender:

```powershell
mkdir MyReplayTool
cd MyReplayTool
dotnet new web -n MyReplayTool.Web --framework net10.0
dotnet new classlib -n MyReplayTool.Domain --framework net10.0
dotnet new sln --name MyReplayTool
dotnet sln add MyReplayTool.Web MyReplayTool.Domain
dotnet add MyReplayTool.Web reference MyReplayTool.Domain
```

Add the ReplayLab packages:

```powershell
dotnet add MyReplayTool.Domain package ReplayLab.Core --version 0.13.0-preview.1
dotnet add MyReplayTool.Web package ReplayLab.Web.Hosting --version 0.13.0-preview.1
```

Your `MyReplayTool.Web.csproj` should look similar to:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ReplayLab.Web.Hosting" Version="0.13.0-preview.1" />
    <ProjectReference Include="..\MyReplayTool.Domain\MyReplayTool.Domain.csproj" />
  </ItemGroup>

</Project>
```

**Available packages:**

| Package | Description |
| --- | --- |
| `ReplayLab.Core` | Contracts and models (required) |
| `ReplayLab.Web.Hosting` | Hostable Web UI entry points |
| `ReplayLab.Cli.Hosting` | Hostable CLI entry points |
| `ReplayLab.Desktop.Hosting` | Hostable Desktop entry points |
| `ReplayLab.Parsers.Csv` | Built-in CSV parser |
| `ReplayLab.Adapters.Mock` | Deterministic mock sender |
| `ReplayLab.Adapters.Http` | HTTP POST sender |

This guide uses `ReplayLab.Core` and `ReplayLab.Web.Hosting`. Add parser and
adapter packages as needed for your own format or target.

## 4. Implement a custom message parser

Create a parser by implementing `IMessageParser`. The example below reads a
simple pipe-delimited format and produces a `ReplayMessage` for each valid line:

**`MyReplayTool.Domain/MyCustomParser.cs`**

```csharp
using System.Text.Json;
using ReplayLab.Core;

namespace MyReplayTool.Domain;

public sealed class MyCustomParser : IMessageParser
{
    public async Task<ReplayBatch> ParseAsync(
        Stream input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var reader = new StreamReader(input, leaveOpen: true);
        var messages = new List<ReplayMessage>();
        var lineNumber = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber++;

            // Skip blank lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                continue;

            var fields = line.Split('|');
            if (fields.Length < 2)
                continue;

            var id = fields[0].Trim();
            var value = fields[1].Trim();

            var payload = JsonSerializer.Serialize(new { id, value });

            messages.Add(new ReplayMessage(
                id,
                payload,
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["sourceLine"] = lineNumber.ToString()
                }));
        }

        return new ReplayBatch(messages);
    }
}
```

Key points:

- `ParseAsync` receives a `Stream` and returns a `ReplayBatch`.
- Each parsed record becomes a `ReplayMessage` with a string `Id`, a string
  `Payload` (typically JSON), an `IReadOnlyDictionary<string, string>` of
  `Headers`, and an `IReadOnlyDictionary<string, string>` of `Metadata`.
- The stream is left open after parsing so the caller manages disposal.

## 5. Implement a custom replay sender

Create a sender by implementing `IReplaySender`. The example below writes each
message payload to the console — useful for debugging and local testing:

**`MyReplayTool.Domain/MyLoggingSender.cs`**

```csharp
using ReplayLab.Core;

namespace MyReplayTool.Domain;

public sealed class MyLoggingSender : IReplaySender
{
    public Task<ReplayResult> SendAsync(
        ReplayMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine($"[{message.Id}] {message.Payload}");

        return Task.FromResult(new ReplayResult
        {
            Success = true,
            MessageId = message.Id
        });
    }
}
```

Key points:

- `SendAsync` receives a `ReplayMessage` and returns a `ReplayResult`.
- Set `Success = true` on success, or `Success = false` with an `ErrorMessage`
  on failure.
- Return `Task.FromResult(...)` for synchronous implementations.

## 6. Register your services

Write an extension method that registers your custom parser and sender with the
dependency injection container. This must be called **before** ReplayLab
hosting extensions so its `TryAdd*` pattern preserves your registrations:

**`MyReplayTool.Domain/ServiceCollectionExtensions.cs`**

```csharp
using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace MyReplayTool.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyReplayServices(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IMessageParser, MyCustomParser>();
        services.AddSingleton<IReplaySender, MyLoggingSender>();

        return services;
    }
}
```

## 7. Host the Web UI

Wire everything together in a minimal `Program.cs`:

**`MyReplayTool.Web/Program.cs`**

```csharp
using MyReplayTool.Domain;
using ReplayLab.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Register custom parser and sender BEFORE AddReplayLabWeb
// so the TryAdd* convention preserves your registrations.
builder.Services.AddMyReplayServices();

// Compose ReplayLab Web hosting.
builder.Services.AddReplayLabWeb();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.MapStaticAssets();
app.MapReplayLabWeb();

app.Run();
```

Key points:

- Call your service registration **before** `AddReplayLabWeb()`.
- `AddReplayLabWeb()` registers the Web UI dependencies but uses `TryAdd*` so
  your parser and sender are not overwritten.
- `MapReplayLabWeb()` mounts the ReplayLab Web UI routes.

## 8. Run

Start your tool:

```powershell
dotnet run --project MyReplayTool.Web
```

Open the URL printed in the console (typically `http://localhost:5000`). You
should see the ReplayLab Web workspace.

Upload a file matching your parser format, inspect the parsed messages, select
rows, and click **Replay Selected**. Your custom sender will process each
message and write the output to the console.

### Sample input file

Create a file at `samples/demo.replay` to use with the example parser:

```text
# MyReplayTool demo input
# Format: id|value

MSG-001|hello-world
MSG-002|test-message
MSG-003|another-record
```

## 9. Next steps

- **Architecture** — [docs/architecture.md](architecture.md) explains how
  parsers, adapters, and hosting layers compose.
- **CustomReplayTool sample** — [samples/CustomReplayTool/](../samples/CustomReplayTool/)
  is a working external-style sample with a custom parser, sender, and Web host.
- **Roadmap** — [docs/roadmap.md](roadmap.md) shows what is planned next.
- **ADRs** — [docs/adr/](adr/) captures architecture decisions including the
  extension model ([ADR 0008](adr/0008-extension-model.md)), hostable entry
  points ([ADR 0009](adr/0009-hostable-entry-points.md)), and distribution
  strategy ([ADR 0005](adr/0005-distribution-strategy.md)).
- **Releases** — [docs/releases.md](releases.md) lists published versions and
  the GitHub Packages release workflow.
- **Source repository** — [github.com/sebastienwitz/replaylab](https://github.com/sebastienwitz/replaylab)

## Learn more

- Add structured logging by passing an `ILogger<T>` to
  `SequentialReplayEngine` (the engine, parser, and sender all accept optional
  `ILogger<T>?` parameters).
- Swap in `ReplayLab.Parsers.Csv` for CSV input without writing a custom parser.
- Swap in `ReplayLab.Adapters.Http` to POST messages to an HTTP endpoint.
- Compose a Desktop app with `ReplayLab.Desktop.Hosting` instead of Web.
- Build a CLI tool with `ReplayLab.Cli.Hosting`.
