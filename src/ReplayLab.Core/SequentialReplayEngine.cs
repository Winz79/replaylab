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
                    Status = result.Success ? ReplayResultStatus.Succeeded : ReplayResultStatus.Failed,
                    Elapsed = stopwatch.Elapsed
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                stopwatch.Stop();

                results.Add(new ReplayResult(
                    Success: false,
                    MessageId: message.Id,
                    ErrorMessage: exception.Message,
                    status: ReplayResultStatus.Failed,
                    elapsed: stopwatch.Elapsed,
                    exceptionType: exception.GetType().FullName,
                    exceptionMessage: exception.Message,
                    exceptionDetails: exception.ToString()));
            }
        }

        return results;
    }
}
