using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace CustomReplayTool.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTicketReplayServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IMessageParser, TicketReplayMessageParser>();
        services.AddSingleton<IReplaySender, TicketReplaySender>();

        return services;
    }
}
