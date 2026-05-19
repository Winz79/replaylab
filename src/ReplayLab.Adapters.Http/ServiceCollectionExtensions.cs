using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Http;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpReplaySender(
        this IServiceCollection services,
        HttpClient httpClient,
        Uri endpointUrl)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(endpointUrl);

        if (!endpointUrl.IsAbsoluteUri)
        {
            throw new ArgumentException("Endpoint URL must be an absolute URI.", nameof(endpointUrl));
        }

        var options = new HttpReplaySenderOptions(endpointUrl);
        services.AddTransient<IReplaySender>(provider => new HttpReplaySender(httpClient, options));

        return services;
    }

    public static IServiceCollection AddHttpReplaySender(
        this IServiceCollection services,
        HttpClient httpClient,
        string endpointUrl)
    {
        ArgumentNullException.ThrowIfNull(endpointUrl);
        return AddHttpReplaySender(services, httpClient, new Uri(endpointUrl));
    }
}
