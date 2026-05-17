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
        string[]? headers = null;
        var messages = new List<ReplayMessage>();
        var rowNumber = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();

            rowNumber++;

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var fields = ParseLine(line, rowNumber);
            if (headers is null)
            {
                headers = fields;
                continue;
            }

            if (fields.Length != headers.Length)
            {
                throw new CsvParseException($"CSV row {rowNumber} has {fields.Length} fields but header row has {headers.Length} fields.");
            }

            var recordNumber = messages.Count + 1;
            var payloadValues = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 0; i < headers.Length; i++)
            {
                payloadValues[headers[i]] = fields[i];
            }

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceFormat"] = "csv",
                ["sourceRowNumber"] = rowNumber.ToString(CultureInfo.InvariantCulture),
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

    private static string[] ParseLine(string line, int rowNumber)
    {
        if (line.Contains('"'))
        {
            throw new CsvParseException($"CSV row {rowNumber} contains quoted fields, which are not supported in the first parser slice.");
        }

        return line.Split(',');
    }
}
