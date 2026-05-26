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
    public async Task ParseAsync_logs_start_and_complete_at_information_level()
    {
        const string csv = """
            name,status
            alpha,new
            """;
        var logger = new SpyLogger<CsvReplayMessageParser>();
        var parser = new CsvReplayMessageParser(logger);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        await parser.ParseAsync(stream);

        var infoEntries = logger.Entries.Where(e => e.Level == LogLevel.Information).ToList();
        Assert.Contains(infoEntries, e => e.Message.Contains("Starting CSV parse"));
        Assert.Contains(infoEntries, e => e.Message.Contains("CSV parse complete"));
    }

    [Fact]
    public async Task ParseAsync_logs_header_at_debug_level()
    {
        const string csv = """
            name,status
            alpha,new
            """;
        var logger = new SpyLogger<CsvReplayMessageParser>();
        var parser = new CsvReplayMessageParser(logger);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        await parser.ParseAsync(stream);

        var debugEntries = logger.Entries.Where(e => e.Level == LogLevel.Debug).ToList();
        Assert.Contains(debugEntries, e => e.Message.Contains("Detected CSV header"));
    }

    [Fact]
    public async Task ParseAsync_logs_per_record_at_debug_level()
    {
        const string csv = """
            name,status
            alpha,new
            beta,done
            """;
        var logger = new SpyLogger<CsvReplayMessageParser>();
        var parser = new CsvReplayMessageParser(logger);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        await parser.ParseAsync(stream);

        var debugEntries = logger.Entries.Where(e => e.Level == LogLevel.Debug).ToList();
        Assert.Contains(debugEntries, e => e.Message.Contains("Parsed CSV row"));
    }

    [Fact]
    public async Task ParseAsync_logs_field_count_mismatch_at_warning_level()
    {
        const string csv = """
            name,status
            alpha,new,extra
            """;
        var logger = new SpyLogger<CsvReplayMessageParser>();
        var parser = new CsvReplayMessageParser(logger);

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        await Assert.ThrowsAsync<CsvParseException>(() => parser.ParseAsync(stream));

        var warningEntries = logger.Entries.Where(e => e.Level == LogLevel.Warning).ToList();
        Assert.Contains(warningEntries, e => e.Message.Contains("has ") && e.Message.Contains("fields but header has"));
    }

    private static async Task<Core.ReplayBatch> Parse(string csv)
    {
        var parser = new CsvReplayMessageParser();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await parser.ParseAsync(stream);
    }

    private sealed class SpyLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
