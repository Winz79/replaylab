using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ReplayLab.Core;

/// <summary>
/// Coordinates the sequential replay of a batch of messages through a configured
/// <see cref="IReplaySender"/>, collecting a <see cref="ReplayResult"/> for each message.
/// </summary>
public sealed class SequentialReplayEngine
{
    private readonly IReplaySender _sender;
    private readonly ILogger<SequentialReplayEngine>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequentialReplayEngine"/> class.
    /// </summary>
    /// <param name="sender">The sender responsible for dispatching each message.</param>
    /// <param name="logger">An optional logger for structured diagnostic output.
    /// When <c>null</c>, logging is disabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is <c>null</c>.</exception>
    public SequentialReplayEngine(IReplaySender sender, ILogger<SequentialReplayEngine>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(sender);
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Sends each message in the batch sequentially through the configured sender,
    /// recording the result for every message.
    /// </summary>
    /// <param name="batch">The batch of messages to replay.</param>
    /// <param name="cancellationToken">A cancellation token that stops processing
    /// before the next message is sent. An empty batch returns an empty list.</param>
    /// <returns>An ordered list of <see cref="ReplayResult"/> entries, one per message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="batch"/>
    /// or <paramref name="batch.Messages"/> is <c>null</c>.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is
    /// canceled through the <paramref name="cancellationToken"/>.</exception>
    public async Task<IReadOnlyList<ReplayResult>> ReplayAsync(
        ReplayBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(batch.Messages);

        var batchStopwatch = Stopwatch.StartNew();
        var results = new List<ReplayResult>(batch.Messages.Count);
        var messageIndex = 0;
        var totalMessages = batch.Messages.Count;

        _logger?.LogInformation("Starting sequential replay of {MessageCount} messages", totalMessages);

        foreach (var message in batch.Messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            messageIndex++;
            var stopwatch = Stopwatch.StartNew();

            _logger?.LogDebug("Sending message {MessageId} ({MessageIndex}/{TotalMessages})",
                message.Id, messageIndex, totalMessages);

            try
            {
                var result = await _sender.SendAsync(message, cancellationToken);
                stopwatch.Stop();

                _logger?.LogDebug("Message {MessageId} sent successfully in {ElapsedMs}ms",
                    message.Id, (long)stopwatch.Elapsed.TotalMilliseconds);

                results.Add(result with
                {
                    MessageId = message.Id ?? "(unknown)",
                    Elapsed = stopwatch.Elapsed
                });
            }
            catch (OperationCanceledException exception)
            {
                stopwatch.Stop();

                if (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                _logger?.LogWarning("Message {MessageId} was canceled", message.Id);

                results.Add(new ReplayResult
                {
                    Success = false,
                    MessageId = message.Id ?? "(unknown)",
                    ErrorMessage = "The send operation was canceled.",
                    Elapsed = stopwatch.Elapsed,
                    ExceptionType = exception.GetType().FullName,
                    ExceptionMessage = exception.Message,
                    ExceptionDetails = exception.ToString()
                });
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

                _logger?.LogError(exception, "Message {MessageId} failed: {ErrorMessage}",
                    message.Id, exception.Message);

                results.Add(new ReplayResult
                {
                    Success = false,
                    MessageId = message.Id ?? "(unknown)",
                    ErrorMessage = "An unexpected error occurred while sending the message.",
                    Elapsed = stopwatch.Elapsed,
                    ExceptionType = exception.GetType().FullName,
                    ExceptionMessage = exception.Message,
                    ExceptionDetails = exception.ToString()
                });
            }
        }

        batchStopwatch.Stop();

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);

        _logger?.LogInformation(
            "Replay complete: {SuccessCount} succeeded, {FailureCount} failed out of {TotalMessages} in {TotalElapsedMs}ms",
            successCount, failureCount, totalMessages, (long)batchStopwatch.Elapsed.TotalMilliseconds);

        return results;
    }
}
