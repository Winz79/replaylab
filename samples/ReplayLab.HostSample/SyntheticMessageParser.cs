using System.Text;
using ReplayLab.Core;

namespace ReplayLab.HostSample;

public sealed class SyntheticMessageParser : IMessageParser
{
    private readonly SyntheticServiceLog _log;

    public SyntheticMessageParser(SyntheticServiceLog log)
    {
        _log = log;
    }

    public async Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);
        _ = await reader.ReadToEndAsync(cancellationToken);
        _log.Record("parser:parse");

        return new ReplayBatch([
            new ReplayMessage(
                "sample-1",
                "{\"source\":\"sample\",\"payload\":\"Synthetic sample payload\"}",
                new Dictionary<string, string>(),
                new Dictionary<string, string>())
        ]);
    }
}
