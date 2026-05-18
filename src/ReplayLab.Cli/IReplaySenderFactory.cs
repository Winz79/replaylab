using ReplayLab.Core;

namespace ReplayLab.Cli;

public interface IReplaySenderFactory
{
    IReplaySender CreateMockSender();

    IReplaySender CreateHttpSender(Uri endpointUrl);
}
