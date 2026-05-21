using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
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
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var home = await client.GetAsync("/");
        using var css = await client.GetAsync("/_content/ReplayLab.Web.Hosting/css/site.css");

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(HttpStatusCode.OK, css.StatusCode);
    }

    [Fact]
    public async Task Sample_host_web_flow_uses_host_provided_parser_and_sender_factory()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        using var request = await CreateReplayRequestAsync(client, """
            kind,name
            Created,alpha
            """, "sample-1");

        using var response = await client.SendAsync(request);
        var html = await response.Content.ReadAsStringAsync();
        var log = app.Services.GetRequiredService<SyntheticServiceLog>().Entries;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sent 1 selected row(s): 1 succeeded, 0 failed.", html);
        Assert.Contains("parser:parse", log);
        Assert.Contains("sender-factory:create-mock", log);
        Assert.Contains("sender:send:sample-1", log);
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddReplayLabHostSample();
        builder.Services.AddReplayLabWeb();

        var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets(Path.Combine(AppContext.BaseDirectory, "ReplayLab.HostSample.Tests.staticwebassets.endpoints.json"));
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

    private sealed record AntiforgeryState(string Token, string Cookie);
}
