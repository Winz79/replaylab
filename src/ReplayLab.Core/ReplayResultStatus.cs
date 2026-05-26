namespace ReplayLab.Core;

/// <summary>
/// Indicates the outcome of a replay operation for a single message.
/// </summary>
public enum ReplayResultStatus
{
    /// <summary>
    /// The message was sent successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The message could not be sent.
    /// </summary>
    Failed
}
