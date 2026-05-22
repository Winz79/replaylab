using System.Text.Json;
using ReplayLab.Core;

namespace CustomReplayTool.Domain;

public sealed class TicketReplayMessageParser : IMessageParser
{
    public async Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        using var reader = new StreamReader(input, leaveOpen: true);
        var messages = new List<ReplayMessage>();
        var lineNumber = 0;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var fields = line.Split('|');
            if (fields.Length != 4)
            {
                // Skip malformed lines rather than failing the whole batch
                continue;
            }

            var ticketId = fields[0].Trim();
            var operation = fields[1].Trim();
            var target = fields[2].Trim();
            var description = fields[3].Trim();

            var payload = JsonSerializer.Serialize(new
            {
                ticketId,
                operation,
                target,
                description
            });

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceFormat"] = "tickets",
                ["sourceLineNumber"] = lineNumber.ToString()
            };

            messages.Add(new ReplayMessage(
                ticketId,
                payload,
                new Dictionary<string, string>(StringComparer.Ordinal),
                metadata));
        }

        return new ReplayBatch(messages);
    }
}
