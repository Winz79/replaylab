namespace ReplayLab.Core;

public sealed record ReplayResult
{
    public bool Success { get; init; }

    public string MessageId { get; init; } = string.Empty;

    public string? ErrorMessage { get; init; }

    public ReplayResultStatus Status => Success ? ReplayResultStatus.Succeeded : ReplayResultStatus.Failed;

    public TimeSpan Elapsed { get; init; }

    public string? ExceptionType { get; init; }

    public string? ExceptionMessage { get; init; }

    public string? ExceptionDetails { get; init; }
}
