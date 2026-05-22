using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using ReplayLab.Web;

namespace ReplayLab.Web.Tests;

public sealed class WebUiFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebUiFlowTests(WebApplicationFactory<Program> factory)
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
        Assert.Contains("Choose file", html);
        Assert.Contains("No file selected", html);
        Assert.DoesNotContain("Preview parsed messages", html);
        Assert.Contains("tabulator.min.css", html);
        Assert.Contains("tabulator.min.js", html);
        Assert.Contains("replay-grid.js", html);
    }

    [Fact]
    public async Task Post_upload_parses_csv_and_emits_tabulator_grid_data()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/", await CreateUploadContentAsync(client, """
            kind,name
            Created,alpha
            Updated,beta
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"replay-grid\"", html);
        Assert.Contains("id=\"replay-grid-data\"", html);
        Assert.Contains("Send selected", html);
        Assert.Contains("messages.csv", html);
        Assert.Contains("2 row(s)", html);
        Assert.DoesNotContain("Parsed message preview", html);
        Assert.DoesNotContain("Preview parsed messages", html);

        var gridState = ReadGridState(html);
        Assert.Equal("record-1", gridState.Rows[0]["_msgId"]?.GetValue<string>());
        Assert.Equal("pending", gridState.Rows[0]["_status"]?.GetValue<string>());
        Assert.Equal("Created", gridState.Rows[0]["kind"]?.GetValue<string>());
        Assert.Equal("alpha", gridState.Rows[0]["name"]?.GetValue<string>());
        Assert.Equal(["kind", "name"], gridState.CsvColumns);
        Assert.Equal([], gridState.SelectedIds);
    }

    [Fact]
    public async Task Post_upload_disables_replay_until_rows_are_selected()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/", await CreateUploadContentAsync(client, """
            kind,name
            Created,alpha
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("0 selected", html);
        Assert.Contains("id=\"replay-selected\"", html);
        Assert.Matches("<button[^>]+id=\"replay-selected\"[^>]+disabled", html);
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
        Assert.Contains("Parse failed:", html);
        Assert.Contains("header row has 2 fields", html);
    }

    [Fact]
    public async Task Post_replay_runs_mock_sender_for_selected_rows_and_shows_in_row_results()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
            kind,name
            Created,alpha
            Updated,beta
            Deleted,gamma
            """, "record-1", "record-3"));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sent 2 selected row(s): 2 succeeded, 0 failed.", html);
        Assert.DoesNotContain("Replay results", html);

        var gridState = ReadGridState(html);
        Assert.Equal("succeeded", FindRow(gridState, "record-1")["_status"]?.GetValue<string>());
        Assert.Equal("pending", FindRow(gridState, "record-2")["_status"]?.GetValue<string>());
        Assert.Equal("succeeded", FindRow(gridState, "record-3")["_status"]?.GetValue<string>());
        Assert.Equal([], gridState.SelectedIds);
        Assert.Contains("0 selected / 3 row(s)", html);
        Assert.Matches("<button[^>]+id=\"replay-selected\"[^>]+disabled", html);
    }

    [Fact]
    public async Task Post_replay_without_selected_rows_shows_error_and_does_not_send()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
            kind,name
            Created,alpha
            """));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Select at least one row to replay.", html);
        Assert.DoesNotContain("Sent 1 selected row(s):", html);
        Assert.Equal("pending", FindRow(ReadGridState(html), "record-1")["_status"]?.GetValue<string>());
    }

    [Fact]
    public async Task Post_replay_warns_before_resending_previously_succeeded_rows()
    {
        using var client = _factory.CreateClient();
        const string csv = """
            kind,name
            Created,alpha
            Updated,beta
            """;

        using var firstResponse = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, csv, "record-1"));
        var firstHtml = await firstResponse.Content.ReadAsStringAsync();
        var firstGridState = ReadGridState(firstHtml);

        using var warningResponse = await client.PostAsync(
            "/?handler=Replay",
            await CreateReplayContentWithStateAsync(client, csv, firstGridState.RawJson, confirmResendSucceeded: false, "record-1"));
        var warningHtml = await warningResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, warningResponse.StatusCode);
        Assert.Contains("already succeeded", warningHtml);
        Assert.DoesNotContain("Sent 1 selected row(s):", warningHtml);

        var warningGridState = ReadGridState(warningHtml);
        Assert.Equal(["record-1"], warningGridState.SelectedIds);
        Assert.Equal("succeeded", FindRow(warningGridState, "record-1")["_status"]?.GetValue<string>());
        Assert.Equal("pending", FindRow(warningGridState, "record-2")["_status"]?.GetValue<string>());

        using var confirmedResponse = await client.PostAsync(
            "/?handler=Replay",
            await CreateReplayContentWithStateAsync(client, csv, warningGridState.RawJson, confirmResendSucceeded: true, "record-1"));
        var confirmedHtml = await confirmedResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, confirmedResponse.StatusCode);
        Assert.Contains("Sent 1 selected row(s): 1 succeeded, 0 failed.", confirmedHtml);

        var confirmedGridState = ReadGridState(confirmedHtml);
        Assert.Equal([], confirmedGridState.SelectedIds);
        Assert.Equal("succeeded", FindRow(confirmedGridState, "record-1")["_status"]?.GetValue<string>());
        Assert.Equal("pending", FindRow(confirmedGridState, "record-2")["_status"]?.GetValue<string>());
    }

    [Fact]
    public async Task Post_replay_preserves_prior_row_results_after_sending_another_selection()
    {
        using var client = _factory.CreateClient();
        const string csv = """
            kind,name
            Created,alpha
            Updated,beta
            """;

        using var firstResponse = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, csv, "record-1"));
        var firstGridState = ReadGridState(await firstResponse.Content.ReadAsStringAsync());

        using var secondResponse = await client.PostAsync(
            "/?handler=Replay",
            await CreateReplayContentWithStateAsync(client, csv, firstGridState.RawJson, confirmResendSucceeded: false, "record-2"));
        var secondHtml = await secondResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var secondGridState = ReadGridState(secondHtml);
        Assert.Equal([], secondGridState.SelectedIds);
        Assert.Equal("succeeded", FindRow(secondGridState, "record-1")["_status"]?.GetValue<string>());
        Assert.Equal("succeeded", FindRow(secondGridState, "record-2")["_status"]?.GetValue<string>());
    }

    [Fact]
    public async Task Post_replay_response_keeps_upload_form_targeted_to_upload_handler()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/?handler=Replay", await CreateReplayContentAsync(client, """
            kind,name
            Created,alpha
            """, "record-1"));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Sent 1 selected row(s): 1 succeeded, 0 failed.", html);
        Assert.Matches("<form[^>]+class=\"upload-form\"[^>]+action=\"/\"", html);
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
        Assert.Contains("Parse failed:", html);
        Assert.DoesNotContain("Sent 0 selected row(s): 0 succeeded, 0 failed.", html);
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
        Assert.Contains("id=\"replay-grid\"", html);
        Assert.Contains("record-1", html);
        Assert.DoesNotContain("record-2", html);
        Assert.DoesNotContain("Sent 2 selected row(s): 2 succeeded, 0 failed.", html);
        Assert.Equal("pending", FindRow(ReadGridState(html), "record-1")["_status"]?.GetValue<string>());
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

    private static async Task<MultipartFormDataContent> CreateReplayContentAsync(
        HttpClient client,
        string csv,
        params string[] selectedRows)
    {
        var content = CreateReplayContent(csv, selectedRows);
        content.Add(new StringContent(await GetRequestVerificationTokenAsync(client)), "__RequestVerificationToken");
        return content;
    }

    private static async Task<MultipartFormDataContent> CreateReplayContentWithStateAsync(
        HttpClient client,
        string csv,
        string replayStateJson,
        bool confirmResendSucceeded,
        params string[] selectedRows)
    {
        var content = CreateReplayContent(csv, selectedRows);
        content.Add(new StringContent(replayStateJson), "ReplayStateJson");
        content.Add(new StringContent(confirmResendSucceeded.ToString()), "ConfirmResendSucceeded");
        content.Add(new StringContent(await GetRequestVerificationTokenAsync(client)), "__RequestVerificationToken");
        return content;
    }

    private static MultipartFormDataContent CreateUploadContent(string csv)
    {
        var content = new MultipartFormDataContent();
        content.Add(CreateFileContent(csv), "Upload", "messages.csv");
        return content;
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

    private sealed record GridState(JsonObject[] Rows, string[] CsvColumns, string[] SelectedIds, string RawJson);
}
