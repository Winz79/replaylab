using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Example;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExampleReplaySender(this IServiceCollection services, string filePath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));
        }

        services.AddTransient<IReplaySender>(provider => new FileReplaySender(filePath));

        return services;
    }
}
