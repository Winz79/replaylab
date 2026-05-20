# Private Host Composition Sample Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a small synthetic sample host under `samples/` that owns DI composition and proves ReplayLab hostable CLI and Web entry points can be consumed without modifying ReplayLab product code.

**Architecture:** Create one sample host project with only two modes, `cli` and `web`, and keep its composition explicit in sample-owned extension methods and synthetic services. Prove CLI DI consumption directly in tests, and prove Web hosting through `AddReplayLabWeb()` and `MapReplayLabWeb()` without expanding Web internals if the current Web workflow does not yet resolve parser/sender behavior from DI.

**Tech Stack:** .NET 10, ASP.NET Core, Microsoft.Extensions.DependencyInjection, ReplayLab.Cli.Hosting, ReplayLab.Web.Hosting, xUnit, Microsoft.AspNetCore.TestHost

---

## File Structure

- Create: `samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj`
  Responsibility: runnable sample host project referencing the hostable ReplayLab libraries.
- Create: `samples/ReplayLab.HostSample/Program.cs`
  Responsibility: sample-owned mode selection (`cli` or `web`) and top-level app startup.
- Create: `samples/ReplayLab.HostSample/SampleServiceCollectionExtensions.cs`
  Responsibility: explicit sample composition-root registration helpers.
- Create: `samples/ReplayLab.HostSample/SyntheticMessageParser.cs`
  Responsibility: sample-owned observable parser implementation.
- Create: `samples/ReplayLab.HostSample/SyntheticReplaySender.cs`
  Responsibility: sample-owned observable sender implementation.
- Create: `samples/ReplayLab.HostSample/SyntheticReplaySenderFactory.cs`
  Responsibility: sample-owned CLI sender factory implementation.
- Create: `samples/ReplayLab.HostSample/SyntheticServiceLog.cs`
  Responsibility: tiny observable log used only by the sample and its tests to prove DI consumption.
- Create: `tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj`
  Responsibility: focused sample tests.
- Create: `tests/ReplayLab.HostSample.Tests/CliSampleCompositionTests.cs`
  Responsibility: prove `CliApplication.RunAsync(...)` consumes sample-owned parser and sender factory from DI.
- Create: `tests/ReplayLab.HostSample.Tests/WebSampleHostingTests.cs`
  Responsibility: prove the sample can host `ReplayLab.Web.Hosting` through `AddReplayLabWeb()` and `MapReplayLabWeb()`.
- Modify: `samples/README.md`
  Responsibility: add short run instructions only if needed.
- Modify: `ReplayLab.sln`
  Responsibility: include the new sample and test project.

### Task 1: Write The Failing Sample Tests First

**Files:**
- Create: `tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj`
- Create: `tests/ReplayLab.HostSample.Tests/CliSampleCompositionTests.cs`
- Create: `tests/ReplayLab.HostSample.Tests/WebSampleHostingTests.cs`
- Modify: `ReplayLab.sln`

- [ ] **Step 1: Write the failing sample test project**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.TestHost" Version="10.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\samples\ReplayLab.HostSample\ReplayLab.HostSample.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Write a failing CLI composition proof test**

```csharp
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Cli.Hosting;
using ReplayLab.HostSample;

namespace ReplayLab.HostSample.Tests;

public sealed class CliSampleCompositionTests
{
    [Fact]
    public async Task Cli_mode_uses_sample_owned_parser_and_sender_factory_from_di()
    {
        var services = new ServiceCollection();
        services.AddReplayLabHostSample();
        using var provider = services.BuildServiceProvider();

        var inputPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(inputPath, "sample input");

        var output = new StringWriter(new StringBuilder());
        var error = new StringWriter(new StringBuilder());

        try
        {
            var exitCode = await CliApplication.RunAsync([inputPath], output, error, provider);

            Assert.Equal(0, exitCode);

            var log = provider.GetRequiredService<SyntheticServiceLog>();
            Assert.Contains("parser:parse", log.Entries);
            Assert.Contains("sender-factory:create-mock", log.Entries);
            Assert.Contains("sender:send:sample-1", log.Entries);
            Assert.Contains("Synthetic sample payload", output.ToString());
        }
        finally
        {
            File.Delete(inputPath);
        }
    }
}
```

- [ ] **Step 3: Write a failing Web hosting proof test**

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using ReplayLab.HostSample;
using ReplayLab.Web.Hosting;

namespace ReplayLab.HostSample.Tests;

public sealed class WebSampleHostingTests
{
    [Fact]
    public async Task Sample_host_can_mount_replaylab_web_hosting_surface()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddReplayLabHostSample();
        builder.Services.AddReplayLabWeb();

        await using var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets(Path.Combine(AppContext.BaseDirectory, "ReplayLab.HostSample.Tests.staticwebassets.endpoints.json"));
        app.MapReplayLabWeb();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var home = await client.GetAsync("/");
        using var css = await client.GetAsync("/_content/ReplayLab.Web.Hosting/css/site.css");

        Assert.Equal(System.Net.HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, css.StatusCode);
    }
}
```

- [ ] **Step 4: Run the new sample tests to verify they fail for the missing sample host**

Run: `dotnet test tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj`

Expected: FAIL because `ReplayLab.HostSample` and `AddReplayLabHostSample()` do not exist yet.

- [ ] **Step 5: Add the test project to the solution**

Run: `dotnet sln ReplayLab.sln add tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj`

Expected: the test project is added for continued TDD cycles.

### Task 2: Implement The Small Sample Host And Observable Services

**Files:**
- Create: `samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj`
- Create: `samples/ReplayLab.HostSample/Program.cs`
- Create: `samples/ReplayLab.HostSample/SampleServiceCollectionExtensions.cs`
- Create: `samples/ReplayLab.HostSample/SyntheticMessageParser.cs`
- Create: `samples/ReplayLab.HostSample/SyntheticReplaySender.cs`
- Create: `samples/ReplayLab.HostSample/SyntheticReplaySenderFactory.cs`
- Create: `samples/ReplayLab.HostSample/SyntheticServiceLog.cs`

- [ ] **Step 1: Create the sample host project**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\ReplayLab.Cli.Hosting\ReplayLab.Cli.Hosting.csproj" />
    <ProjectReference Include="..\..\src\ReplayLab.Web.Hosting\ReplayLab.Web.Hosting.csproj" />
    <ProjectReference Include="..\..\src\ReplayLab.Core\ReplayLab.Core.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create the observable sample service log**

```csharp
namespace ReplayLab.HostSample;

public sealed class SyntheticServiceLog
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    public void Record(string entry)
    {
        _entries.Add(entry);
    }
}
```

- [ ] **Step 3: Create the sample-owned DI registrations**

```csharp
using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Cli.Hosting;
using ReplayLab.Core;

namespace ReplayLab.HostSample;

public static class SampleServiceCollectionExtensions
{
    public static IServiceCollection AddReplayLabHostSample(this IServiceCollection services)
    {
        services.AddSingleton<SyntheticServiceLog>();
        services.AddSingleton<IMessageParser, SyntheticMessageParser>();
        services.AddSingleton<IReplaySenderFactory, SyntheticReplaySenderFactory>();
        return services;
    }
}
```

- [ ] **Step 4: Create the sample-owned parser**

```csharp
using System.Text;
using ReplayLab.Core;

namespace ReplayLab.HostSample;

public sealed class SyntheticMessageParser : IMessageParser
{
    private readonly SyntheticServiceLog _log;

    public SyntheticMessageParser(SyntheticServiceLog log)
    {
        _log = log;
    }

    public async Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);
        _ = await reader.ReadToEndAsync(cancellationToken);
        _log.Record("parser:parse");

        return new ReplayBatch([
            new ReplayMessage("sample-1", "{\"source\":\"sample\",\"payload\":\"Synthetic sample payload\"}")
        ]);
    }
}
```

- [ ] **Step 5: Create the sample-owned sender and sender factory**

```csharp
using ReplayLab.Cli.Hosting;
using ReplayLab.Core;

namespace ReplayLab.HostSample;

public sealed class SyntheticReplaySenderFactory : IReplaySenderFactory
{
    private readonly SyntheticServiceLog _log;

    public SyntheticReplaySenderFactory(SyntheticServiceLog log)
    {
        _log = log;
    }

    public IReplaySender CreateMockSender()
    {
        _log.Record("sender-factory:create-mock");
        return new SyntheticReplaySender(_log);
    }

    public IReplaySender CreateHttpSender(Uri endpointUrl)
    {
        _log.Record($"sender-factory:create-http:{endpointUrl}");
        return new SyntheticReplaySender(_log);
    }
}

public sealed class SyntheticReplaySender : IReplaySender
{
    private readonly SyntheticServiceLog _log;

    public SyntheticReplaySender(SyntheticServiceLog log)
    {
        _log = log;
    }

    public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
    {
        _log.Record($"sender:send:{message.Id}");
        return Task.FromResult(ReplayResult.Success(message.Id));
    }
}
```

- [ ] **Step 6: Create the sample program with only `cli` and `web` modes**

```csharp
using ReplayLab.Cli.Hosting;
using ReplayLab.Web.Hosting;

namespace ReplayLab.HostSample;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            await Console.Error.WriteLineAsync("Usage: hostsample <cli|web> [args]");
            return 2;
        }

        return args[0] switch
        {
            "cli" => await RunCliAsync(args[1..]),
            "web" => await RunWebAsync(args[1..]),
            _ => 2
        };
    }

    private static async Task<int> RunCliAsync(string[] args)
    {
        var services = new ServiceCollection();
        services.AddReplayLabHostSample();
        using var provider = services.BuildServiceProvider();
        return await CliApplication.RunAsync(args, Console.Out, Console.Error, provider);
    }

    private static async Task<int> RunWebAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddReplayLabHostSample();
        builder.Services.AddReplayLabWeb();

        var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets();
        app.MapReplayLabWeb();
        await app.RunAsync();
        return 0;
    }
}
```

- [ ] **Step 7: Run the sample test project to verify the new code turns the tests green**

Run: `dotnet test tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj`

Expected: PASS with the CLI test proving sample-owned DI consumption and the Web test proving hostability through the sample-owned host.

### Task 3: Wire The Sample Into The Solution And Keep Documentation Minimal

**Files:**
- Modify: `ReplayLab.sln`
- Modify: `samples/README.md`

- [ ] **Step 1: Add the sample project to the solution**

Run: `dotnet sln ReplayLab.sln add samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj`

Expected: the sample project is added to the solution.

- [ ] **Step 2: Add only the minimal sample README guidance if needed**

```md
## Hostable entry point sample

Run the synthetic sample CLI host:

```powershell
dotnet run --project samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj -- cli samples/basic.csv
```

Run the synthetic sample Web host:

```powershell
dotnet run --project samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj -- web
```
```

- [ ] **Step 3: Keep the documentation scoped to composition proof only**

```text
Describe that the sample owns DI and host startup.
Do not document it as a new product shell.
Do not add packaging or future-work detail for #76.
```

### Task 4: Full Verification And Delivery

**Files:**
- Modify: only the intended sample, test, solution, and docs files

- [ ] **Step 1: Run focused sample verification**

Run: `dotnet test tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj`

Expected: PASS.

- [ ] **Step 2: Run full solution tests if feasible**

Run: `dotnet test ReplayLab.sln`

Expected: PASS, or document any unrelated pre-existing failure if one appears.

- [ ] **Step 3: Run patch verification**

Run: `git diff --check`

Expected: no output.

- [ ] **Step 4: Commit the implementation**

```bash
git add ReplayLab.sln samples/ReplayLab.HostSample tests/ReplayLab.HostSample.Tests samples/README.md docs/superpowers/specs/2026-05-20-private-host-sample-design.md docs/superpowers/plans/2026-05-20-private-host-sample-implementation.md
git commit -m "feat: add host composition sample"
```

- [ ] **Step 5: Re-fetch and rebase before pushing and opening the PR**

Run: `git fetch origin main`

Then: `git rebase origin/main`

Expected: branch is not behind `main` before PR creation.

- [ ] **Step 6: Push and create the PR**

```bash
git push -u origin <branch-name>
gh pr create --title "feat: add private host composition sample" --body "Closes #75

## Summary
- add a synthetic sample host under samples/
- prove CLI hostability uses sample-owned DI registrations
- prove Web hostability can be mounted from a sample-owned host

## Files Changed
- ReplayLab.sln
- samples/ReplayLab.HostSample/**
- tests/ReplayLab.HostSample.Tests/**
- samples/README.md
- docs/superpowers/specs/2026-05-20-private-host-sample-design.md
- docs/superpowers/plans/2026-05-20-private-host-sample-implementation.md

## Verification
- dotnet test tests/ReplayLab.HostSample.Tests/ReplayLab.HostSample.Tests.csproj
- dotnet test ReplayLab.sln
- git diff --check

## Assumptions
- the current Web workflow hostability proof is limited to external hosting and static/page mapping, not to parser/sender DI consumption through Web internals

## Risks
- the sample could drift into a broader shell if future changes add extra modes or behavior

## Out Of Scope
- #76
- #68
- #69
- #70
- packaging or publishing" 
```
