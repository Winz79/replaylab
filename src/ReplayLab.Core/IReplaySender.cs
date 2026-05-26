namespace ReplayLab.Core;

/// <summary>
/// Sends replay messages to a target system through a configured adapter.
/// </summary>
public interface IReplaySender
{
    /// <summary>
    /// Sends a single replay message to the configured target.
    /// </summary>
    /// <param name="message">The replay message to send.</param>
    /// <param name="cancellationToken">A token that cancels the send operation.</param>
    /// <returns>A <see cref="ReplayResult"/> describing the outcome of the send operation.</returns>
    Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default);
}
