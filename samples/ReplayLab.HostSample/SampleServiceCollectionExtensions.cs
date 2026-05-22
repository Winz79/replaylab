using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Cli.Hosting;
using ReplayLab.Core;

namespace ReplayLab.HostSample;

public static class SampleServiceCollectionExtensions
{
    public static IServiceCollection AddReplayLabHostSample(this IServiceCollection services)
    {
        services.AddSingleton<SyntheticServiceLog>();
        services.AddSingleton<IMessageParser, SyntheticMessageParser>();
        services.AddSingleton<IReplaySenderFactory, SyntheticReplaySenderFactory>();
        services.AddSingleton<IReplaySender>(serviceProvider =>
            serviceProvider.GetRequiredService<IReplaySenderFactory>().CreateMockSender());
        return services;
    }
}
