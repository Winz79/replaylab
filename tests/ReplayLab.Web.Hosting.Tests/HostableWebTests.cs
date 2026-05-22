using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReplayLab.Core;
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
        Assert.Contains("tabulator.min.css", html);
        Assert.Contains("replay-grid.js", html);
    }

    [Fact]
    public async Task Minimal_host_serves_static_web_assets_from_hosting_library()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var cssResponse = await client.GetAsync("/_content/ReplayLab.Web.Hosting/css/site.css");
        using var jsResponse = await client.GetAsync("/_content/ReplayLab.Web.Hosting/js/replay-grid.js");
        using var tabulatorCssResponse = await client.GetAsync("/_content/ReplayLab.Web.Hosting/lib/tabulator/css/tabulator.min.css");
        using var tabulatorJsResponse = await client.GetAsync("/_content/ReplayLab.Web.Hosting/lib/tabulator/js/tabulator.min.js");

        Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, tabulatorCssResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, tabulatorJsResponse.StatusCode);
    }

    [Fact]
    public async Task Minimal_host_runs_upload_and_replay_flow_without_replaylab_web_program()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        using var request = await CreateReplayRequestAsync(client, """
            kind,name
            Created,alpha
            Updated,beta
            """, "record-1");
        using var response = await client.SendAsync(request);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sent 1 selected row(s): 1 succeeded, 0 failed.", html);
        Assert.Equal("succeeded", FindRow(ReadGridState(html), "record-1")["_status"]?.GetValue<string>());
    }

    [Fact]
    public async Task Minimal_host_shows_parse_error_for_invalid_default_csv_input()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        using var request = await CreateUploadRequestAsync(client, """
            kind,name
            Created,alpha,extra
            """);
        using var response = await client.SendAsync(request);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Parse failed:", html);
        Assert.Empty(ReadGridState(html).Rows);
    }

    [Fact]
    public async Task Minimal_host_uses_services_from_the_composition_root()
    {
        var parser = new RecordingParser();
        var sender = new RecordingSender();

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton<IMessageParser>(parser);
            services.AddSingleton<IReplaySender>(sender);
        });

        using var client = app.GetTestClient();
        using var request = await CreateReplayRequestAsync(client, "irrelevant", "web-custom-1");
        using var response = await client.SendAsync(request);

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(parser.WasUsed);
        Assert.True(sender.WasUsed);
        Assert.Equal("succeeded", FindRow(ReadGridState(html), "web-custom-1")["_status"]?.GetValue<string>());
    }

    [Fact]
    public async Task Minimal_host_uses_host_provided_web_parser_for_upload_workflow()
    {
        var parser = new RecordingWebReplayParser();

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton<IWebReplayParser>(parser);
        });

        using var client = app.GetTestClient();
        using var request = await CreateUploadRequestAsync(client, "ignored-by-custom-parser");
        using var response = await client.SendAsync(request);

        var html = await response.Content.ReadAsStringAsync();
        var gridState = ReadGridState(html);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(parser.WasUsed);
        Assert.Contains("kind", gridState.CsvColumns);
        Assert.Equal("web-custom-1", FindRow(gridState, "web-custom-1")["_msgId"]?.GetValue<string>());
    }

    private static async Task<WebApplication> CreateAppAsync(Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddReplayLabWeb();
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets(Path.Combine(AppContext.BaseDirectory, "ReplayLab.Web.Hosting.Tests.staticwebassets.endpoints.json"));
        app.MapReplayLabWeb();
        await app.StartAsync();
        return app;
    }

    private static async Task<HttpRequestMessage> CreateReplayRequestAsync(
        HttpClient client,
        string csv,
        params string[] selectedRows)
    {
        var content = CreateReplayContent(csv, selectedRows);
        var antiforgery = await GetAntiforgeryAsync(client);
        content.Add(new StringContent(antiforgery.Token), "__RequestVerificationToken");

        var request = new HttpRequestMessage(HttpMethod.Post, "/?handler=Replay")
        {
            Content = content
        };
        request.Headers.Add("Cookie", antiforgery.Cookie);
        return request;
    }

    private static async Task<HttpRequestMessage> CreateUploadRequestAsync(HttpClient client, string csv, string fileName = "sample.csv")
    {
        var antiforgery = await GetAntiforgeryAsync(client);
        var content = new MultipartFormDataContent();
        var fileContent = new StringContent(csv);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "Upload", fileName);
        content.Add(new StringContent(antiforgery.Token), "__RequestVerificationToken");

        var request = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = content
        };
        request.Headers.Add("Cookie", antiforgery.Cookie);
        return request;
    }

    private static MultipartFormDataContent CreateReplayContent(string csv, params string[] selectedRows)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("Replay"), "handler" },
            { new StringContent(csv), "UploadedCsv" }
        };

        foreach (var selectedRow in selectedRows)
        {
            content.Add(new StringContent(selectedRow), "SelectedMessageIds");
        }

        return content;
    }

    private static async Task<AntiforgeryState> GetAntiforgeryAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "Expected an antiforgery token on the page.");
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders), "Expected an antiforgery cookie.");

        return new AntiforgeryState(
            WebUtility.HtmlDecode(match.Groups[1].Value),
            cookieHeaders.Single(header => header.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal)).Split(';', 2)[0]);
    }

    private static GridState ReadGridState(string html)
    {
        var match = Regex.Match(
            html,
            "<script id=\"replay-grid-data\" type=\"application/json\">([\\s\\S]*?)</script>",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "Expected serialized grid state.");

        var document = JsonNode.Parse(WebUtility.HtmlDecode(match.Groups[1].Value))!.AsObject();
        var rows = document["rows"]!.AsArray().Select(node => node!.AsObject()).ToArray();
        var csvColumns = document["csvColumns"]!.AsArray().Select(node => node!.GetValue<string>()).ToArray();
        var selectedIds = document["selectedIds"]!.AsArray().Select(node => node!.GetValue<string>()).ToArray();

        return new GridState(rows, csvColumns, selectedIds, WebUtility.HtmlDecode(match.Groups[1].Value));
    }

    private static JsonObject FindRow(GridState gridState, string messageId)
    {
        return gridState.Rows.Single(row => row["_msgId"]?.GetValue<string>() == messageId);
    }

    private sealed record AntiforgeryState(string Token, string Cookie);

    private sealed record GridState(JsonObject[] Rows, string[] CsvColumns, string[] SelectedIds, string RawJson);

    private sealed class RecordingParser : IMessageParser
    {
        public bool WasUsed { get; private set; }

        public Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
        {
            WasUsed = true;
            return Task.FromResult(new ReplayBatch([
                new ReplayMessage(
                    "web-custom-1",
                    "{\"kind\":\"Custom\",\"name\":\"override\"}",
                    new Dictionary<string, string>(),
                    new Dictionary<string, string>())]));
        }
    }

    private sealed class RecordingWebReplayParser : IWebReplayParser
    {
        public bool WasUsed { get; private set; }

        public Task<WebReplayParseResult> ParseAsync(string input, CancellationToken cancellationToken = default)
        {
            WasUsed = true;
            return Task.FromResult(WebReplayParseResult.Success([
                new ReplayMessage(
                    "web-custom-1",
                    "{\"kind\":\"Custom\",\"name\":\"override\"}",
                    new Dictionary<string, string>(),
                    new Dictionary<string, string>())]));
        }
    }

    private sealed class RecordingSender : IReplaySender
    {
        public bool WasUsed { get; private set; }

        public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
        {
            WasUsed = true;
            return Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id,
            });
        }
    }
}
