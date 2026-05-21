using ReplayLab.Core;

namespace ReplayLab.Cli;

public static class CliApplication
{
    public static Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        return ReplayLab.Cli.Hosting.CliApplication.RunAsync(
            args,
            output,
            error,
            services,
            cancellationToken);
    }

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        IMessageParser? parser = null,
        IReplaySender? sender = null,
        IReplaySenderFactory? senderFactory = null,
        CancellationToken cancellationToken = default)
    {
        var services = new Dictionary<Type, object>();
        if (parser is not null)
        {
            services[typeof(IMessageParser)] = parser;
        }

        if (sender is not null)
        {
            services[typeof(ReplayLab.Cli.Hosting.IReplaySenderFactory)] = new SingleSenderFactory(sender);
        }
        else if (senderFactory is not null)
        {
            services[typeof(ReplayLab.Cli.Hosting.IReplaySenderFactory)] = senderFactory;
        }

        return await RunAsync(
            args,
            output,
            error,
            new DictionaryServiceProvider(services),
            cancellationToken);
    }

    private sealed class DictionaryServiceProvider(IReadOnlyDictionary<Type, object> services) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return serviceType == typeof(IServiceProvider)
                ? this
                : services.TryGetValue(serviceType, out var service)
                    ? service
                    : null;
        }
    }

    private sealed class SingleSenderFactory(IReplaySender sender) : ReplayLab.Cli.Hosting.IReplaySenderFactory
    {
        public IReplaySender CreateMockSender() => sender;

        public IReplaySender CreateHttpSender(Uri endpointUrl) => sender;
    }
}
