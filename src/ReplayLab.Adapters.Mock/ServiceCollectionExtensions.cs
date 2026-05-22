using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Mock;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMockReplaySender(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IReplaySender, MockReplaySender>();

        return services;
    }
}
