using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ReplayLab.Parsers.Csv.Tests;

public class CsvReplayMessageParserTests
{
    [Fact]
    public async Task ParseAsync_converts_valid_csv_rows_into_replay_messages()
    {
        const string csv = """
            messageType,name,quantity
            Created,alpha,2
            Updated,beta,5
            """;

        var batch = await Parse(csv);

        Assert.Equal(2, batch.Messages.Count);
        Assert.Equal("record-1", batch.Messages[0].Id);
        Assert.Equal("record-2", batch.Messages[1].Id);
        Assert.Empty(batch.Messages[0].Headers!);

        using var payload = JsonDocument.Parse(batch.Messages[0].Payload);
        Assert.Equal("Created", payload.RootElement.GetProperty("messageType").GetString());
        Assert.Equal("alpha", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("2", payload.RootElement.GetProperty("quantity").GetString());
    }

    [Fact]
    public async Task ParseAsync_ignores_empty_lines_and_comment_lines()
    {
        const string csv = """
            # synthetic comment before header
            name,status

            alpha,new
            # synthetic comment between records
            beta,done

            """;

        var batch = await Parse(csv);

        Assert.Equal(2, batch.Messages.Count);
        Assert.Equal("record-1", batch.Messages[0].Id);
        Assert.Equal("record-2", batch.Messages[1].Id);
    }

    [Fact]
    public async Task ParseAsync_records_csv_metadata_for_each_message()
    {
        const string csv = """
            # comment row
            name,status
            alpha,new

            beta,done
            """;

        var batch = await Parse(csv);

        Assert.Equal("csv", batch.Messages[0].Metadata!["sourceFormat"]);
        Assert.Equal("3", batch.Messages[0].Metadata!["sourceRowNumber"]);
        Assert.Equal("1", batch.Messages[0].Metadata!["dataRecordNumber"]);
        Assert.Equal("5", batch.Messages[1].Metadata!["sourceRowNumber"]);
        Assert.Equal("2", batch.Messages[1].Metadata!["dataRecordNumber"]);
    }

    [Fact]
    public async Task ParseAsync_throws_clear_exception_when_header_is_missing()
    {
        var exception = await Assert.ThrowsAsync<CsvParseException>(() => Parse("""

            # only comments

            """));

        Assert.Contains("header row", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_throws_clear_exception_when_field_count_does_not_match_header()
    {
        var exception = await Assert.ThrowsAsync<CsvParseException>(() => Parse("""
            name,status
            alpha,new,extra
            """));

        Assert.Contains("row 2", exception.Message);
        Assert.Contains("header row has 2 fields", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_parses_quoted_fields()
    {
        const string csv = """
            name,status
            "alpha",new
            """;

        var batch = await Parse(csv);

        Assert.Single(batch.Messages);
        using var payload = JsonDocument.Parse(batch.Messages[0].Payload);
        Assert.Equal("alpha", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("new", payload.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ParseAsync_parses_embedded_commas_in_quoted_fields()
    {
        const string csv = """
            name,status
            "alpha, beta",new
            """;

        var batch = await Parse(csv);

        Assert.Single(batch.Messages);
        using var payload = JsonDocument.Parse(batch.Messages[0].Payload);
        Assert.Equal("alpha, beta", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("new", payload.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ParseAsync_parses_escaped_quotes()
    {
        const string csv = """"
            name,status
            "alpha ""beta""",new
            """";

        var batch = await Parse(csv);

        Assert.Single(batch.Messages);
        using var payload = JsonDocument.Parse(batch.Messages[0].Payload);
        Assert.Equal("alpha \"beta\"", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("new", payload.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ParseAsync_parses_embedded_newlines_in_quoted_fields()
    {
        const string csv = """"
            name,status
            "alpha
            beta",new
            """";

        var batch = await Parse(csv);

        Assert.Single(batch.Messages);
        using var payload = JsonDocument.Parse(batch.Messages[0].Payload);
        Assert.Equal("alpha\nbeta", payload.RootElement.GetProperty("name").GetString());
        Assert.Equal("new", payload.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task ParseAsync_logs_parse_start_and_completion_information()
    {
        const string csv = """
            name,status
            alpha,new
            """;
        var logger = new FakeLogger<CsvReplayMessageParser>();

        await ParseWithLogger(csv, logger);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("Starting CSV parse"));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("CSV parse complete: 1 messages"));
    }

    [Fact]
    public async Task ParseAsync_logs_header_detection_at_debug_level()
    {
        const string csv = """
            name,status,category
            alpha,new,test
            """;
        var logger = new FakeLogger<CsvReplayMessageParser>();

        await ParseWithLogger(csv, logger);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Detected CSV header with 3 columns"));
    }

    [Fact]
    public async Task ParseAsync_logs_per_record_parse_at_debug_level()
    {
        const string csv = """
            name,status
            alpha,new
            beta,done
            """;
        var logger = new FakeLogger<CsvReplayMessageParser>();

        await ParseWithLogger(csv, logger);

        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Debug && e.Message.Contains("Parsed CSV row") && e.Message.Contains("as record"));
    }

    [Fact]
    public async Task ParseAsync_logs_field_count_mismatch_warning_before_throwing()
    {
        const string csv = """
            name,status
            alpha,new,extra
            """;
        var logger = new FakeLogger<CsvReplayMessageParser>();

        var exception = await Assert.ThrowsAsync<CsvParseException>(() =>
            ParseWithLogger(csv, logger));

        Assert.Contains("row 2", exception.Message);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Message.Contains("has") && e.Message.Contains("fields but header has"));
    }

    [Fact]
    public async Task ParseAsync_works_without_logger()
    {
        const string csv = """
            name,status
            alpha,new
            """;

        var batch = await Parse(csv);

        Assert.Single(batch.Messages);
    }

    private static async Task<Core.ReplayBatch> Parse(string csv)
    {
        var parser = new CsvReplayMessageParser();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await parser.ParseAsync(stream);
    }

    private static async Task<Core.ReplayBatch> ParseWithLogger(
        string csv,
        ILogger<CsvReplayMessageParser> logger)
    {
        var parser = new CsvReplayMessageParser(logger);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await parser.ParseAsync(stream);
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        bool ILogger.IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
