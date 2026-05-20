using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
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
}
