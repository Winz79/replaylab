using System.Diagnostics;

namespace ReplayLab.Core;

public sealed class SequentialReplayEngine
{
    private readonly IReplaySender _sender;

    public SequentialReplayEngine(IReplaySender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);
        _sender = sender;
    }

    /// <summary>
    /// Sends each message in the batch sequentially through the configured sender,
    /// recording the result for every message.
    /// </summary>
    /// <param name="batch">The batch of messages to replay.</param>
    /// <param name="cancellationToken">A cancellation token that stops processing
    /// before the next message is sent. An empty batch returns an empty list.</param>
    /// <returns>An ordered list of <see cref="ReplayResult"/> entries, one per message.</returns>
    public async Task<IReadOnlyList<ReplayResult>> ReplayAsync(
        ReplayBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(batch.Messages);

        var results = new List<ReplayResult>(batch.Messages.Count);

        foreach (var message in batch.Messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await _sender.SendAsync(message, cancellationToken);
                stopwatch.Stop();

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

                results.Add(new ReplayResult
                {
                    Success = false,
                    MessageId = message.Id ?? "(unknown)",
                    ErrorMessage = exception.Message,
                    Elapsed = stopwatch.Elapsed,
                    ExceptionType = typeof(OperationCanceledException).FullName,
                    ExceptionMessage = exception.Message,
                    ExceptionDetails = exception.ToString()
                });
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

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

        return results;
    }
}
