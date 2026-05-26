using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using ReplayLab.Core;
using System.Globalization;
using System.Text.Json;

namespace ReplayLab.Parsers.Csv;

public sealed class CsvReplayMessageParser : IMessageParser
{
    private readonly ILogger<CsvReplayMessageParser>? _logger;

    public CsvReplayMessageParser(ILogger<CsvReplayMessageParser>? logger = null)
    {
        _logger = logger;
    }

    public async Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        _logger?.LogInformation("Starting CSV parse");

        using var reader = new StreamReader(input, leaveOpen: true);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            AllowComments = true,
            Comment = '#',
            IgnoreBlankLines = true,
        };

        using var csv = new CsvReader(reader, config);
        string[]? headers = null;
        var messages = new List<ReplayMessage>();
        var recordNumber = 0;
        var totalRows = 0;

        while (await csv.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            totalRows++;

            if (headers is null)
            {
                csv.ReadHeader();
                headers = csv.HeaderRecord;
                if (headers is null || headers.Length == 0)
                {
                    throw new CsvParseException("CSV input does not contain a header row.");
                }

                _logger?.LogDebug("Detected CSV header with {HeaderCount} columns: {Headers}",
                    headers.Length, string.Join(", ", headers));

                continue;
            }

            var rawRow = csv.Context?.Parser?.RawRow ?? 0;

            if (csv.Parser.Count != headers.Length)
            {
                _logger?.LogWarning("CSV row {RawRow} has {ActualFieldCount} fields but header has {HeaderFieldCount} fields",
                    rawRow, csv.Parser.Count, headers.Length);

                throw new CsvParseException($"CSV row {rawRow} has {csv.Parser.Count} fields but header row has {headers.Length} fields.");
            }

            recordNumber++;

            _logger?.LogDebug("Parsed CSV row {SourceRowNumber} as record {DataRecordNumber}",
                rawRow, recordNumber);

            var payloadValues = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < headers.Length; i++)
            {
                payloadValues[headers[i]] = csv.GetField(i) ?? string.Empty;
            }

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceFormat"] = "csv",
                ["sourceRowNumber"] = rawRow.ToString(CultureInfo.InvariantCulture),
                ["dataRecordNumber"] = recordNumber.ToString(CultureInfo.InvariantCulture),
            };

            messages.Add(new ReplayMessage(
                $"record-{recordNumber}",
                JsonSerializer.Serialize(payloadValues),
                new Dictionary<string, string>(StringComparer.Ordinal),
                metadata));
        }

        if (headers is null)
        {
            throw new CsvParseException("CSV input does not contain a header row.");
        }

        _logger?.LogInformation("CSV parse complete: {RecordCount} messages from {TotalRows} rows",
            recordNumber, totalRows);

        return new ReplayBatch(messages);
    }
}
