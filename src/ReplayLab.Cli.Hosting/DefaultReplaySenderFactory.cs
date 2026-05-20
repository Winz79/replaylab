using ReplayLab.Adapters.Http;
using ReplayLab.Adapters.Mock;
using ReplayLab.Core;

namespace ReplayLab.Cli.Hosting;

public sealed class DefaultReplaySenderFactory : IReplaySenderFactory
{
    public IReplaySender CreateMockSender() => new MockReplaySender();

    public IReplaySender CreateHttpSender(Uri endpointUrl)
    {
        return new HttpReplaySender(
            new HttpClient(),
            new HttpReplaySenderOptions(endpointUrl));
    }
}
