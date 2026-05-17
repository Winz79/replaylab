namespace ReplayLab.Core;

public interface IMessageParser
{
    Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default);
}
