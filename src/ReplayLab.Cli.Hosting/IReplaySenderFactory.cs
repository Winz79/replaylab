using ReplayLab.Core;

namespace ReplayLab.Cli.Hosting;

public interface IReplaySenderFactory
{
    IReplaySender CreateMockSender();

    IReplaySender CreateHttpSender(Uri endpointUrl);
}
