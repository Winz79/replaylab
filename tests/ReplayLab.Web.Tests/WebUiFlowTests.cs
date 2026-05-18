using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
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
        using var response = await client.PostAsync("/", await CreateUploadContentAsync(client, """
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
        using var response = await client.PostAsync("/", await CreateUploadContentAsync(client, """
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
        using var response = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
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

    [Fact]
    public async Task Post_replay_with_invalid_csv_does_not_show_a_zero_message_summary()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
            kind,name
            Created,alpha,extra
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("CSV parse failed:", html);
        Assert.DoesNotContain("Replay results", html);
        Assert.DoesNotContain("Sent 0 message(s): 0 succeeded, 0 failed.", html);
    }

    [Fact]
    public async Task Post_upload_replaces_prior_replay_results_with_the_new_preview_state()
    {
        using var client = _factory.CreateClient();

        using (var replayResponse = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
            kind,name
            Created,alpha
            Updated,beta
            """)))
        {
            Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
        }

        using var uploadResponse = await client.PostAsync("/", await CreateUploadContentAsync(client, """
            kind,name
            Created,gamma
            """));
        var html = await uploadResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
        Assert.Contains("Parsed message preview", html);
        Assert.Contains("record-1", html);
        Assert.DoesNotContain("record-2", html);
        Assert.DoesNotContain("Replay results", html);
        Assert.DoesNotContain("Sent 2 message(s): 2 succeeded, 0 failed.", html);
    }

    [Fact]
    public async Task Post_without_antiforgery_token_is_rejected()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/", CreateUploadContent("kind,name\nCreated,alpha\n"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<MultipartFormDataContent> CreateUploadContentAsync(HttpClient client, string csv)
    {
        var content = CreateUploadContent(csv);
        content.Add(new StringContent(await GetRequestVerificationTokenAsync(client)), "__RequestVerificationToken");
        return content;
    }

    private static async Task<MultipartFormDataContent> CreateReplayContentAsync(HttpClient client, string csv)
    {
        var content = CreateReplayContent(csv);
        content.Add(new StringContent(await GetRequestVerificationTokenAsync(client)), "__RequestVerificationToken");
        return content;
    }

    private static MultipartFormDataContent CreateUploadContent(string csv)
    {
        var content = new MultipartFormDataContent();
        content.Add(CreateFileContent(csv), "Upload", "messages.csv");
        return content;
    }

    private static MultipartFormDataContent CreateReplayContent(string csv)
    {
        return new MultipartFormDataContent
        {
            { new StringContent("Replay"), "handler" },
            { new StringContent(csv), "UploadedCsv" }
        };
    }

    private static async Task<string> GetRequestVerificationTokenAsync(HttpClient client)
    {
        var html = await client.GetStringAsync("/");
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "Expected an antiforgery token on the page.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static StreamContent CreateFileContent(string csv)
    {
        var streamContent = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv)));
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");
        return streamContent;
    }
}
