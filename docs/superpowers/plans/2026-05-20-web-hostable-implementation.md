# Web Hostable Surface Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract the current ReplayLab Web UI into a reusable `ReplayLab.Web.Hosting` surface with `AddReplayLabWeb()` and `MapReplayLabWeb()` while keeping `ReplayLab.Web` as the runnable shell.

**Architecture:** Introduce a companion Razor-based host library that owns the generic ReplayLab Web pages, page model behavior, and static assets, then have the runnable `ReplayLab.Web` shell reference that library and only apply shell-owned app configuration. Prove the boundary with a minimal in-memory host test that references `ReplayLab.Web.Hosting` directly so Razor Pages discovery and static web asset behavior are exercised without depending on `ReplayLab.Web.Program`.

**Tech Stack:** .NET 10, ASP.NET Core Razor Pages, Razor SDK or compatible Razor Class Library behavior, Microsoft.AspNetCore.Mvc.Testing, Microsoft.AspNetCore.TestHost, xUnit

---

## File Structure

- Create: `src/ReplayLab.Web.Hosting/ReplayLab.Web.Hosting.csproj`
  Responsibility: reusable Razor-based host library for ReplayLab Web.
- Create: `src/ReplayLab.Web.Hosting/ReplayLabWebServiceCollectionExtensions.cs`
  Responsibility: `AddReplayLabWeb(this IServiceCollection services)` registration entry point.
- Create: `src/ReplayLab.Web.Hosting/ReplayLabWebEndpointRouteBuilderExtensions.cs`
  Responsibility: `MapReplayLabWeb(this IEndpointRouteBuilder endpoints)` mapping entry point.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/Index.cshtml`
  Responsibility: reusable ReplayLab upload/grid/replay UI markup.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/Index.cshtml.cs`
  Responsibility: reusable ReplayLab page behavior.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/Error.cshtml`
  Responsibility: reusable error page markup if required by the extracted Razor Pages surface.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/Error.cshtml.cs`
  Responsibility: reusable error page model if required.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/Shared/_Layout.cshtml`
  Responsibility: reusable shell layout and asset references.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/_ViewImports.cshtml`
  Responsibility: Razor Pages imports for the reusable library.
- Create or move: `src/ReplayLab.Web.Hosting/Pages/_ViewStart.cshtml`
  Responsibility: shared view start configuration.
- Create or move: `src/ReplayLab.Web.Hosting/wwwroot/css/site.css`
  Responsibility: reusable CSS for the ReplayLab Web UI.
- Create or move: `src/ReplayLab.Web.Hosting/wwwroot/js/replay-grid.js`
  Responsibility: reusable grid behavior.
- Create or move: `src/ReplayLab.Web.Hosting/wwwroot/lib/tabulator/**/*`
  Responsibility: reusable third-party grid assets already used by the current Web UI.
- Modify: `src/ReplayLab.Web/ReplayLab.Web.csproj`
  Responsibility: reference `ReplayLab.Web.Hosting` and stop owning the reusable Razor surface.
- Modify: `src/ReplayLab.Web/Program.cs`
  Responsibility: thin runnable shell that configures shell-owned middleware and delegates registration/mapping to the host library.
- Modify or remove moved files under `src/ReplayLab.Web/Pages/**` and `src/ReplayLab.Web/wwwroot/**`
  Responsibility: keep the shell thin and avoid duplicated UI ownership.
- Create: `tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj`
  Responsibility: focused hostability tests against a minimal host referencing only `ReplayLab.Web.Hosting`.
- Create: `tests/ReplayLab.Web.Hosting.Tests/HostableWebTests.cs`
  Responsibility: verify page discovery, static asset behavior, and upload/replay flow through the hostable boundary.
- Modify: `ReplayLab.sln`
  Responsibility: include the new host library and test project.

### Task 1: Prove The Missing Hostable Boundary With Failing Tests

**Files:**
- Create: `tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj`
- Create: `tests/ReplayLab.Web.Hosting.Tests/HostableWebTests.cs`
- Modify: `ReplayLab.sln`

- [ ] **Step 1: Write the failing test project**

```xml
<Project Sdk="Microsoft.NET.Sdk">

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
    <ProjectReference Include="..\..\src\ReplayLab.Web.Hosting\ReplayLab.Web.Hosting.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Write failing hostability tests before creating the library**

```csharp
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Web.Hosting;

namespace ReplayLab.Web.Hosting.Tests;

public sealed class HostableWebTests
{
    [Fact]
    public async Task Minimal_host_serves_replaylab_home_page_through_hosting_extensions()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("ReplayLab Web", html);
        Assert.Contains("Choose file", html);
        Assert.Contains("replay-grid.js", html);
        Assert.Contains("tabulator.min.css", html);
    }

    [Fact]
    public async Task Minimal_host_runs_upload_and_replay_flow_without_replaylab_web_program()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var response = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
            kind,name
            Created,alpha
            Updated,beta
            """, "record-1"));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sent 1 selected row(s): 1 succeeded, 0 failed.", html);
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddReplayLabWeb();

        var app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapReplayLabWeb();
        await app.StartAsync();
        return app;
    }
}
```

- [ ] **Step 3: Run the new tests to verify they fail for the missing hostable surface**

Run: `dotnet test tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj`

Expected: FAIL because `ReplayLab.Web.Hosting` and the `AddReplayLabWeb` / `MapReplayLabWeb` APIs do not exist yet, or the project reference cannot resolve.

- [ ] **Step 4: Add the new project to the solution**

Run: `dotnet sln ReplayLab.sln add tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj`

Expected: project added to the solution for continued TDD runs.

### Task 2: Extract The Hostable Web Surface Minimally

**Files:**
- Create: `src/ReplayLab.Web.Hosting/ReplayLab.Web.Hosting.csproj`
- Create: `src/ReplayLab.Web.Hosting/ReplayLabWebServiceCollectionExtensions.cs`
- Create: `src/ReplayLab.Web.Hosting/ReplayLabWebEndpointRouteBuilderExtensions.cs`
- Create or move: `src/ReplayLab.Web.Hosting/Pages/**`
- Create or move: `src/ReplayLab.Web.Hosting/wwwroot/**`
- Modify: `src/ReplayLab.Web/ReplayLab.Web.csproj`
- Modify: `src/ReplayLab.Web/Program.cs`

- [ ] **Step 1: Create the minimal host library project with Razor support**

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\ReplayLab.Adapters.Mock\ReplayLab.Adapters.Mock.csproj" />
    <ProjectReference Include="..\ReplayLab.Core\ReplayLab.Core.csproj" />
    <ProjectReference Include="..\ReplayLab.Parsers.Csv\ReplayLab.Parsers.Csv.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Create the public composition hooks only**

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ReplayLab.Web.Hosting;

public static class ReplayLabWebServiceCollectionExtensions
{
    public static IServiceCollection AddReplayLabWeb(this IServiceCollection services)
    {
        services.AddRazorPages()
            .AddApplicationPart(typeof(ReplayLabWebServiceCollectionExtensions).Assembly);

        return services;
    }
}

public static class ReplayLabWebEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapReplayLabWeb(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRazorPages();
        return endpoints;
    }
}
```

- [ ] **Step 3: Move the reusable Razor Pages and assets into the host library using the smallest strategy that works**

```text
Move from src/ReplayLab.Web/Pages/** to src/ReplayLab.Web.Hosting/Pages/**
Move from src/ReplayLab.Web/wwwroot/** to src/ReplayLab.Web.Hosting/wwwroot/**
Keep namespaces and model references consistent with the new assembly
```

- [ ] **Step 4: Refactor the page model only as needed to compile in the new assembly**

```csharp
namespace ReplayLab.Web.Hosting.Pages;

public sealed class IndexModel : PageModel
{
    // Preserve the current parse, grid-state, and mock replay behavior.
}
```

- [ ] **Step 5: Thin down the runnable shell to composition and middleware**

```csharp
using ReplayLab.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReplayLabWeb();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapReplayLabWeb();
app.Run();

public partial class Program;
```

- [ ] **Step 6: Run the hostability tests to verify Razor discovery and static assets work**

Run: `dotnet test tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj`

Expected: PASS after the extracted library exposes the page surface successfully through the minimal host.

### Task 3: Keep The Runnable Shell Behavior Green

**Files:**
- Modify: `tests/ReplayLab.Web.Tests/ReplayLab.Web.Tests.csproj`
- Modify: `tests/ReplayLab.Web.Tests/WebUiFlowTests.cs` only if namespace or test references need adjustment after extraction
- Modify: `src/ReplayLab.Web/ReplayLab.Web.csproj`

- [ ] **Step 1: Update the runnable shell test project reference if the page model type moves**

```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\ReplayLab.Web\ReplayLab.Web.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Adjust the shell tests to keep booting the runnable app, not the host library**

```csharp
public sealed class WebUiFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebUiFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
}
```

- [ ] **Step 3: Run the runnable shell tests**

Run: `dotnet test tests/ReplayLab.Web.Tests/ReplayLab.Web.Tests.csproj`

Expected: PASS with the same behavioral assertions still validating the repo-owned shell.

- [ ] **Step 4: Refine extraction details only if a shell regression appears**

```text
Keep fixes scoped to page discovery, namespace updates, or shell wiring.
Do not add #75 or #76 work.
```

### Task 4: Full Verification And Delivery

**Files:**
- Modify: `docs/superpowers/specs/2026-05-20-web-hostable-design.md` only if the final implementation needs a small note about the chosen Razor extraction strategy
- Modify: git metadata only for the intended files

- [ ] **Step 1: Run both focused test projects together**

Run: `dotnet test tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj; if ($?) { dotnet test tests/ReplayLab.Web.Tests/ReplayLab.Web.Tests.csproj }`

Expected: both projects PASS.

- [ ] **Step 2: Run the full solution tests if feasible**

Run: `dotnet test ReplayLab.sln`

Expected: PASS, or document any unrelated pre-existing failure if one appears.

- [ ] **Step 3: Run whitespace and patch verification**

Run: `git diff --check`

Expected: no output.

- [ ] **Step 4: Commit the implementation**

```bash
git add ReplayLab.sln src/ReplayLab.Web.Hosting src/ReplayLab.Web tests/ReplayLab.Web.Hosting.Tests tests/ReplayLab.Web.Tests docs/superpowers/specs/2026-05-20-web-hostable-design.md docs/superpowers/plans/2026-05-20-web-hostable-implementation.md
git commit -m "feat: extract hostable web surface"
```

- [ ] **Step 5: Re-fetch and rebase before pushing and opening the PR**

Run: `git fetch origin main`

Then: `git rebase origin/main`

Expected: branch remains up to date with `main` before PR creation.

- [ ] **Step 6: Push and create the PR**

```bash
git push -u origin <branch-name>
gh pr create --title "feat: extract hostable Web entry point" --body "Closes #74

## Summary
- extract ReplayLab.Web.Hosting as the reusable Web surface
- keep ReplayLab.Web as the runnable shell
- add minimal-host hostability tests

## Files Changed
- ReplayLab.sln
- src/ReplayLab.Web.Hosting/**
- src/ReplayLab.Web/**
- tests/ReplayLab.Web.Hosting.Tests/**
- tests/ReplayLab.Web.Tests/**

## Verification
- dotnet test tests/ReplayLab.Web.Hosting.Tests/ReplayLab.Web.Hosting.Tests.csproj
- dotnet test tests/ReplayLab.Web.Tests/ReplayLab.Web.Tests.csproj
- dotnet test ReplayLab.sln
- git diff --check

## Assumptions
- Razor hostability can be satisfied by a companion Razor-based library with explicit application-part support only if needed

## Risks
- Razor Pages discovery and static web asset behavior are the main extraction risks

## Out Of Scope
- #75
- #76" 
```
