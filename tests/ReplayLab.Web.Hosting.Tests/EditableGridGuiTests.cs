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
        var serverAddress = GetServerAddress(app).TrimEnd('/');
        await page.SetContentAsync($$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <link rel="stylesheet" href="{{serverAddress}}/_content/ReplayLab.Web.Hosting/lib/tabulator/css/tabulator.min.css" />
            </head>
            <body>
                <span id="selected-count" data-total="1">0 selected / 1 row(s)</span>
                <button type="button" id="select-all">Select all</button>
                <button type="button" id="deselect-all">Deselect all</button>
                <button type="button" id="reset-all">Reset all</button>
                <button type="button" id="columns-menu">Columns</button>
                <button type="submit" id="replay-selected">Send selected</button>
                <input id="Upload" type="file" />
                <span id="selected-file-name"></span>
                <div id="resend-warning" hidden></div>
                <form id="replay-form">
                    <input id="ReplayStateJson" type="hidden" />
                    <input id="ConfirmResendSucceeded" type="hidden" value="false" />
                    <input id="EditedPayloadsJson" type="hidden" />
                    <div id="selected-message-fields"></div>
                </form>
                <div id="replay-grid" style="height: 24rem;"></div>
                <script id="replay-grid-data" type="application/json">
                {"rows":[{"_msgId":"record-1","_status":"pending","_result":"","_error":"","_originalPayload":"{\"kind\":\"Created\",\"name\":\"alpha\"}","kind":"Created","name":"alpha"}],"csvColumns":["kind","name"],"columnState":{},"selectedIds":[]}
                </script>
                <script src="{{serverAddress}}/_content/ReplayLab.Web.Hosting/lib/tabulator/js/tabulator.min.js"></script>
                <script src="{{serverAddress}}/_content/ReplayLab.Web.Hosting/js/replay-grid.js"></script>
            </body>
            </html>
            """);

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
