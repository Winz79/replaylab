using System.Diagnostics;

namespace ReplayLab.Core;

public sealed class SequentialReplayEngine
{
    private readonly IReplaySender _sender;

    public SequentialReplayEngine(IReplaySender sender)
    {
        _sender = sender;
    }

    public async Task<IReadOnlyList<ReplayResult>> ReplayAsync(
        ReplayBatch batch,
        CancellationToken cancellationToken = default)
    {
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
                    MessageId = message.Id,
                    Elapsed = stopwatch.Elapsed
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

                results.Add(new ReplayResult
                {
                    Success = false,
                    MessageId = message.Id,
                    ErrorMessage = exception.Message,
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
