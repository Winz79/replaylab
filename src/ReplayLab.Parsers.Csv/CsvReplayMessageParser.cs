using CsvHelper;
using CsvHelper.Configuration;
using ReplayLab.Core;
using System.Globalization;
using System.Text.Json;

namespace ReplayLab.Parsers.Csv;

public sealed class CsvReplayMessageParser : IMessageParser
{
    public async Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

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

        while (await csv.ReadAsync().ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (headers is null)
            {
                csv.ReadHeader();
                headers = csv.HeaderRecord;
                if (headers is null || headers.Length == 0)
                {
                    throw new CsvParseException("CSV input does not contain a header row.");
                }

                continue;
            }

            if (csv.Parser.Count != headers.Length)
            {
                var rawRow = csv.Context?.Parser?.RawRow ?? 0;
                throw new CsvParseException($"CSV row {rawRow} has {csv.Parser.Count} fields but header row has {headers.Length} fields.");
            }

            recordNumber++;
            var payloadValues = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < headers.Length; i++)
            {
                payloadValues[headers[i]] = csv.GetField(i) ?? string.Empty;
            }

            var rawRowNum = csv.Context?.Parser?.RawRow ?? 0;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceFormat"] = "csv",
                ["sourceRowNumber"] = rawRowNum.ToString(CultureInfo.InvariantCulture),
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

        return new ReplayBatch(messages);
    }
}
