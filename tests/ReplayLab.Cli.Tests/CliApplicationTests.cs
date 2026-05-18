using ReplayLab.Adapters.Mock;
using ReplayLab.Cli;
using ReplayLab.Core;

namespace ReplayLab.Cli.Tests;

public class CliApplicationTests
{
    [Fact]
    public async Task RunAsync_replays_valid_csv_and_prints_inspection_and_result_summaries()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            Updated,beta
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync([csvPath], output, error);

        Assert.Equal(0, exitCode);
        Assert.Contains("Loaded 2 message(s).", output.ToString());
        Assert.Contains("Inspected 2 message(s).", output.ToString());
        Assert.Contains("Sent 2 message(s): 2 succeeded, 0 failed.", output.ToString());
        Assert.Contains("record-1", output.ToString());
        Assert.Contains("record-2", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task RunAsync_replays_valid_csv_when_format_csv_is_explicit()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            Updated,beta
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["--format", "csv", csvPath], output, error);

        Assert.Equal(0, exitCode);
        Assert.Contains("Loaded 2 message(s).", output.ToString());
        Assert.Contains("Inspected 2 message(s).", output.ToString());
        Assert.Contains("Sent 2 message(s): 2 succeeded, 0 failed.", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task RunAsync_accepts_single_file_argument_that_starts_with_dash()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """, fileNamePrefix: "-sample-");
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync([csvPath], output, error);

        Assert.Equal(0, exitCode);
        Assert.Contains("Loaded 1 message(s).", output.ToString());
        Assert.Contains("Sent 1 message(s): 1 succeeded, 0 failed.", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task RunAsync_accepts_explicit_format_with_file_argument_that_starts_with_dash()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """, fileNamePrefix: "-sample-");
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["--format", "csv", csvPath], output, error);

        Assert.Equal(0, exitCode);
        Assert.Contains("Loaded 1 message(s).", output.ToString());
        Assert.Contains("Sent 1 message(s): 1 succeeded, 0 failed.", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_file_argument_is_missing()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync([], output, error);

        Assert.NotEqual(0, exitCode);
        Assert.Contains("Usage: replaylab <file>", error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_format_option_is_missing_a_value()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["--format"], output, error);

        Assert.NotEqual(0, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Missing value for --format.", error.ToString());
        Assert.Contains("Supported formats: csv", error.ToString());
        Assert.Contains("Usage: replaylab <file>", error.ToString());
        Assert.Contains("Usage: replaylab --format csv <file>", error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_format_is_unsupported_before_parsing_or_replay()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            ["--format", "json", csvPath],
            output,
            error,
            parser: new ThrowingParser(),
            sender: new ThrowingSender());

        Assert.NotEqual(0, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Unsupported input format: json", error.ToString());
        Assert.Contains("Supported formats: csv", error.ToString());
        Assert.Contains("Usage: replaylab --format csv <file>", error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_file_does_not_exist()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        var exitCode = await CliApplication.RunAsync([missingPath], output, error);

        Assert.NotEqual(0, exitCode);
        Assert.Contains("Input file was not found", error.ToString());
        Assert.Contains(missingPath, error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_csv_parse_fails()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha,extra
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync([csvPath], output, error);

        Assert.NotEqual(0, exitCode);
        Assert.Contains("CSV parse failed:", error.ToString());
        Assert.Contains("header row has 2 fields", error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_any_replay_result_fails()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            Updated,beta
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(
            [csvPath],
            output,
            error,
            sender: new FailingReplaySender());

        Assert.NotEqual(0, exitCode);
        Assert.Contains("Sent 2 message(s): 1 succeeded, 1 failed.", output.ToString());
        Assert.Contains("record-2: failed - Synthetic replay failure", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_sender_name_is_unsupported()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["--sender", "ftp", csvPath], output, error);

        Assert.Equal(2, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Unsupported sender: ftp", error.ToString());
        Assert.Contains("--sender", error.ToString());
    }

    [Fact]
    public async Task RunAsync_returns_non_zero_when_http_sender_is_missing_endpoint_url()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await CliApplication.RunAsync(["--sender", "http", csvPath], output, error);

        Assert.Equal(2, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("The --endpoint-url option is required when --sender http is selected.", error.ToString());
    }

    [Fact]
    public async Task RunAsync_uses_http_sender_when_selected_and_endpoint_url_is_provided()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var factory = new RecordingSenderFactory();

        var exitCode = await CliApplication.RunAsync(
            ["--sender", "http", "--endpoint-url", "https://example.test/replay", csvPath],
            output,
            error,
            senderFactory: factory);

        Assert.Equal(0, exitCode);
        Assert.Equal("http", factory.SelectedSender);
        Assert.Equal(new Uri("https://example.test/replay"), factory.EndpointUrl);
        Assert.Contains("Sent 1 message(s): 1 succeeded, 0 failed.", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    private static string CreateTempCsv(string contents, string fileNamePrefix = "")
    {
        var path = Path.Combine(Path.GetTempPath(), $"{fileNamePrefix}{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, contents);
        return path;
    }

    private sealed class TempFileContext(string path) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingReplaySender : IReplaySender
    {
        public Task<ReplayResult> SendAsync(
            ReplayMessage message,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(message.Id == "record-2"
                ? new ReplayResult(false, message.Id, "Synthetic replay failure")
                : new ReplayResult(true, message.Id));
        }
    }

    private sealed class ThrowingParser : IMessageParser
    {
        public Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Parser should not be called for invalid arguments.");
        }
    }

    private sealed class ThrowingSender : IReplaySender
    {
        public Task<ReplayResult> SendAsync(
            ReplayMessage message,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Sender should not be called for invalid arguments.");
        }
    }

    private sealed class RecordingSenderFactory : IReplaySenderFactory
    {
        public string? SelectedSender { get; private set; }

        public Uri? EndpointUrl { get; private set; }

        public IReplaySender CreateMockSender()
        {
            SelectedSender = "mock";
            return new MockReplaySender();
        }

        public IReplaySender CreateHttpSender(Uri endpointUrl)
        {
            SelectedSender = "http";
            EndpointUrl = endpointUrl;
            return new MockReplaySender();
        }
    }
}
