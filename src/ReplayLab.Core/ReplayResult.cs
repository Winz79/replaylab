namespace ReplayLab.Core;

/// <summary>
/// Contains the outcome of sending a single replay message, including success status,
/// timing, and any error information.
/// </summary>
public sealed record ReplayResult
{
    /// <summary>
    /// Gets a value indicating whether the message was sent successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the identifier of the message that was processed.
    /// </summary>
    public string MessageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the error message if the send operation failed; otherwise, <c>null</c>.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the replay result status derived from the <see cref="Success"/> value.
    /// </summary>
    public ReplayResultStatus Status => Success ? ReplayResultStatus.Succeeded : ReplayResultStatus.Failed;

    /// <summary>
    /// Gets the time elapsed while sending the message.
    /// </summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>
    /// Gets the fully qualified type name of the exception if the send operation
    /// failed; otherwise, <c>null</c>.
    /// </summary>
    public string? ExceptionType { get; init; }

    /// <summary>
    /// Gets the exception message if the send operation failed; otherwise, <c>null</c>.
    /// </summary>
    public string? ExceptionMessage { get; init; }

    /// <summary>
    /// Gets the full exception details including stack trace if the send operation
    /// failed; otherwise, <c>null</c>.
    /// </summary>
    public string? ExceptionDetails { get; init; }
}
