using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using ReplayLab.Web.Hosting;

namespace ReplayLab.Web.Hosting.Tests;

public sealed class EditableGridGuiTests
{
    [Fact]
    public async Task Clicking_payload_data_cell_selects_row_when_row_is_not_editing()
    {
        await using var app = await CreateKestrelAppAsync();
        var browserExecutable = FindBrowserExecutable();

        if (browserExecutable is null)
        {
            return;
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = browserExecutable
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(GetServerAddress(app));
        await page.Locator("#Upload").SetInputFilesAsync(new FilePayload
        {
            Name = "messages.csv",
            MimeType = "text/csv",
            Buffer = Encoding.UTF8.GetBytes("kind,name\nCreated,alpha\n")
        });
        await page.Locator("form.upload-form").EvaluateAsync("form => form.submit()");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.WaitForFunctionAsync("() => window.ReplayLabGrid?.getRows?.().length === 1");
        await page.WaitForFunctionAsync("() => document.querySelector('#selected-count')?.textContent?.includes('0 selected')");

        await page.EvaluateAsync("""
            () => {
              const cells = Array.from(document.querySelectorAll('.tabulator-cell'));
              const cell = cells.find((candidate) => candidate.textContent.trim() === 'alpha');
              if (!cell) {
                throw new Error('Expected alpha payload cell.');
              }
              cell.click();
            }
            """);

        await page.WaitForFunctionAsync("() => document.querySelector('#selected-count')?.textContent?.includes('1 selected')");
    }

    private static async Task<WebApplication> CreateKestrelAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddReplayLabWeb();

        var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets(Path.Combine(AppContext.BaseDirectory, "ReplayLab.Web.Hosting.Tests.staticwebassets.endpoints.json"));
        app.MapReplayLabWeb();
        await app.StartAsync();
        return app;
    }

    private static string GetServerAddress(WebApplication app)
    {
        var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
        return addresses?.Addresses.Single() ?? throw new InvalidOperationException("Expected a Kestrel server address.");
    }

    private static string? FindBrowserExecutable()
    {
        var configured = Environment.GetEnvironmentVariable("PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH");
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var candidates = new[]
        {
            "/usr/bin/chromium",
            "/usr/bin/chromium-browser",
            "/usr/bin/google-chrome",
            "/usr/bin/google-chrome-stable",
            "/usr/bin/microsoft-edge",
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
