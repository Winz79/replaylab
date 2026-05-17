namespace ReplayLab.Core;

public interface IReplaySender
{
    Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default);
}
