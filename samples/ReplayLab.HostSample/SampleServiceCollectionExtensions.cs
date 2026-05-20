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
        return services;
    }
}
