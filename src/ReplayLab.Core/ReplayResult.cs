namespace ReplayLab.Core;

public sealed record ReplayResult
{
    public ReplayResult(
        bool Success,
        string MessageId,
        string? ErrorMessage = null,
        ReplayResultStatus? status = null,
        TimeSpan elapsed = default,
        string? exceptionType = null,
        string? exceptionMessage = null,
        string? exceptionDetails = null)
    {
        this.Success = Success;
        this.MessageId = MessageId;
        this.ErrorMessage = ErrorMessage;
        Status = status ?? (Success ? ReplayResultStatus.Succeeded : ReplayResultStatus.Failed);
        Elapsed = elapsed;
        ExceptionType = exceptionType;
        ExceptionMessage = exceptionMessage;
        ExceptionDetails = exceptionDetails;
    }

    public bool Success { get; init; }

    public string MessageId { get; init; }

    public string? ErrorMessage { get; init; }

    public ReplayResultStatus Status { get; init; }

    public TimeSpan Elapsed { get; init; }

    public string? ExceptionType { get; init; }

    public string? ExceptionMessage { get; init; }

    public string? ExceptionDetails { get; init; }
}
