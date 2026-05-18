using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using ReplayLab.Web.Pages;

namespace ReplayLab.Web.Tests;

public sealed class WebUiFlowTests : IClassFixture<WebApplicationFactory<IndexModel>>
{
    private readonly WebApplicationFactory<IndexModel> _factory;

    public WebUiFlowTests(WebApplicationFactory<IndexModel> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_home_page_positions_the_app_as_local_only()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("ReplayLab Web", html);
        Assert.Contains("local-only", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Upload CSV", html);
    }

    [Fact]
    public async Task Post_upload_parses_csv_and_shows_preview()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/", CreateUploadContent("""
            kind,name
            Created,alpha
            Updated,beta
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Parsed message preview", html);
        Assert.Contains("record-1", html);
        Assert.Contains("record-2", html);
        Assert.Contains("{&quot;kind&quot;:&quot;Created&quot;,&quot;name&quot;:&quot;alpha&quot;}", html);
        Assert.Contains("Run mock replay", html);
    }

    [Fact]
    public async Task Post_upload_shows_parse_error_for_invalid_csv()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/", CreateUploadContent("""
            kind,name
            Created,alpha,extra
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("CSV parse failed:", html);
        Assert.Contains("header row has 2 fields", html);
    }

    [Fact]
    public async Task Post_replay_runs_mock_sender_and_shows_per_message_results()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/?handler=Replay", CreateReplayContent("""
            kind,name
            Created,alpha
            Updated,beta
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Replay results", html);
        Assert.Contains("Sent 2 message(s): 2 succeeded, 0 failed.", html);
        Assert.Contains("record-1", html);
        Assert.Contains("record-2", html);
        Assert.Contains("Succeeded", html);
    }

    private static MultipartFormDataContent CreateUploadContent(string csv)
    {
        var content = new MultipartFormDataContent();
        content.Add(CreateFileContent(csv), "Upload", "messages.csv");
        return content;
    }

    private static MultipartFormDataContent CreateReplayContent(string csv)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent("Replay"), "handler" },
            { new StringContent(csv), "UploadedCsv" }
        };
        return content;
    }

    private static StreamContent CreateFileContent(string csv)
    {
        var streamContent = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv)));
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        return streamContent;
    }
}
