using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReplayLab.Adapters.Mock;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Web.Hosting;

public static class ReplayLabWebServiceCollectionExtensions
{
    // Hosts must enable ASP.NET Core static web assets and static files middleware
    // when serving the library-owned CSS and JS under /_content/ReplayLab.Web.Hosting.
    public static IServiceCollection AddReplayLabWeb(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IMessageParser, CsvReplayMessageParser>();
        services.TryAddTransient<IWebReplayParser, MessageParserWebReplayParser>();
        services.TryAddSingleton<IReplaySender, MockReplaySender>();

        services
            .AddRazorPages()
            .AddApplicationPart(typeof(ReplayLabWebServiceCollectionExtensions).Assembly);

        return services;
    }
}
