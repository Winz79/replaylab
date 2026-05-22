using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReplayLab.Core;

namespace ReplayLab.Parsers.Csv;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCsvMessageParser(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IMessageParser, CsvReplayMessageParser>();

        return services;
    }
}
