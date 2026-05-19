using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Adapters.Mock;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Mock.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMockReplaySender_registers_IReplaySender()
    {
        var services = new ServiceCollection();

        services.AddMockReplaySender();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IReplaySender>();

        Assert.IsType<MockReplaySender>(sender);
    }
}
