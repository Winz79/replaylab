using ReplayLab.Core;

namespace ReplayLab.HostSample;

public sealed class SyntheticReplaySender : IReplaySender
{
    private readonly SyntheticServiceLog _log;

    public SyntheticReplaySender(SyntheticServiceLog log)
    {
        _log = log;
    }

    public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
    {
        _log.Record($"sender:send:{message.Id}");
        return Task.FromResult(new ReplayResult
        {
            Success = true,
            MessageId = message.Id,
            Elapsed = TimeSpan.Zero
        });
    }
}
