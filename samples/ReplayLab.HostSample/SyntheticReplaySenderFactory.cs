using ReplayLab.Cli.Hosting;
using ReplayLab.Core;

namespace ReplayLab.HostSample;

public sealed class SyntheticReplaySenderFactory : IReplaySenderFactory
{
    private readonly SyntheticServiceLog _log;

    public SyntheticReplaySenderFactory(SyntheticServiceLog log)
    {
        _log = log;
    }

    public IReplaySender CreateMockSender()
    {
        _log.Record("sender-factory:create-mock");
        return new SyntheticReplaySender(_log);
    }

    public IReplaySender CreateHttpSender(Uri endpointUrl)
    {
        _log.Record($"sender-factory:create-http:{endpointUrl}");
        return new SyntheticReplaySender(_log);
    }
}
