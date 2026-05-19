using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Mock;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMockReplaySender(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IReplaySender, MockReplaySender>();

        return services;
    }
}
