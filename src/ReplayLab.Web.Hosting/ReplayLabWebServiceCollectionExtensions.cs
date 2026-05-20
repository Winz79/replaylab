using Microsoft.Extensions.DependencyInjection;

namespace ReplayLab.Web.Hosting;

public static class ReplayLabWebServiceCollectionExtensions
{
    // Hosts must enable ASP.NET Core static web assets and static files middleware
    // when serving the library-owned CSS and JS under /_content/ReplayLab.Web.Hosting.
    public static IServiceCollection AddReplayLabWeb(this IServiceCollection services)
    {
        services
            .AddRazorPages()
            .AddApplicationPart(typeof(ReplayLabWebServiceCollectionExtensions).Assembly);

        return services;
    }
}
