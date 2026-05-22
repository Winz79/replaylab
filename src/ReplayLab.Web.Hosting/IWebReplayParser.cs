using ReplayLab.Core;

namespace ReplayLab.Web.Hosting;

public interface IWebReplayParser
{
    Task<WebReplayParseResult> ParseAsync(string input, CancellationToken cancellationToken = default);
}

public sealed record WebReplayParseResult(IReadOnlyList<ReplayMessage>? Messages, string? ErrorMessage)
{
    public bool Succeeded => ErrorMessage is null;

    public static WebReplayParseResult Success(IReadOnlyList<ReplayMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        return new WebReplayParseResult(messages, null);
    }

    public static WebReplayParseResult Failure(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new WebReplayParseResult(null, errorMessage);
    }
}
