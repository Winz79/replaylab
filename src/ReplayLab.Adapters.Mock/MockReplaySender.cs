using ReplayLab.Core;

namespace ReplayLab.Adapters.Mock;

public sealed class MockReplaySender : IReplaySender
{
    public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new ReplayResult
        {
            Success = true,
            MessageId = message.Id
        });
    }
}
