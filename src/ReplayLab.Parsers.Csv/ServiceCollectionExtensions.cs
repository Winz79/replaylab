using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace ReplayLab.Parsers.Csv;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCsvMessageParser(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IMessageParser, CsvReplayMessageParser>();

        return services;
    }
}
