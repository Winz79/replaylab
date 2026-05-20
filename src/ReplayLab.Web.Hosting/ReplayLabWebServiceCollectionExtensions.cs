using Microsoft.Extensions.DependencyInjection;

namespace ReplayLab.Web.Hosting;

public static class ReplayLabWebServiceCollectionExtensions
{
    public static IServiceCollection AddReplayLabWeb(this IServiceCollection services)
    {
        services
            .AddRazorPages()
            .AddApplicationPart(typeof(ReplayLabWebServiceCollectionExtensions).Assembly);

        return services;
    }
}
