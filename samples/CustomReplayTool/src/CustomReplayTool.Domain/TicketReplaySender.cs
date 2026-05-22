using System.Text.Json;
using ReplayLab.Core;

namespace CustomReplayTool.Domain;

public sealed class TicketReplaySender : IReplaySender
{
    public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var operation = ExtractOperation(message.Payload);

        if (string.Equals(operation, "Fail", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ReplayResult
            {
                Success = false,
                MessageId = message.Id,
                ErrorMessage = $"Custom sender rejected {message.Id} because operation is 'Fail'."
            });
        }

        return Task.FromResult(new ReplayResult
        {
            Success = true,
            MessageId = message.Id
        });
    }

    private static string? ExtractOperation(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("operation", out var property))
            {
                return property.GetString();
            }
        }
        catch (JsonException)
        {
            // Ignore parse failures
        }

        return null;
    }
}
