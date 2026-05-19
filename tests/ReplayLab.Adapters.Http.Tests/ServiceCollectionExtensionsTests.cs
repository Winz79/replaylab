using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Adapters.Http;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Http.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHttpReplaySender_registers_IReplaySender()
    {
        var services = new ServiceCollection();
        var httpClient = new HttpClient();
        var endpointUrl = new Uri("http://example.com/api");

        services.AddHttpReplaySender(httpClient, endpointUrl);

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IReplaySender>();

        Assert.IsType<HttpReplaySender>(sender);
    }
}
