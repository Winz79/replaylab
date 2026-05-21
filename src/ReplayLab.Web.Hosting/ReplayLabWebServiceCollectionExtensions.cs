using Microsoft.Extensions.DependencyInjection.Extensions;
using ReplayLab.Cli.Hosting;
using ReplayLab.Parsers.Csv;
using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace ReplayLab.Web.Hosting;

public static class ReplayLabWebServiceCollectionExtensions
{
    // Hosts must enable ASP.NET Core static web assets and static files middleware
    // when serving the library-owned CSS and JS under /_content/ReplayLab.Web.Hosting.
    public static IServiceCollection AddReplayLabWeb(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IMessageParser, CsvReplayMessageParser>();
        services.TryAddSingleton<IReplaySenderFactory, DefaultReplaySenderFactory>();

        services
            .AddRazorPages()
            .AddApplicationPart(typeof(ReplayLabWebServiceCollectionExtensions).Assembly);

        return services;
    }
}
