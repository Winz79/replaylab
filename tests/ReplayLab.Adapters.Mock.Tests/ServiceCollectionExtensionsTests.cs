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

    [Fact]
    public void AddMockReplaySender_does_not_override_existing_registration()
    {
        var customSender = new CustomSender();
        var services = new ServiceCollection();
        services.AddSingleton<IReplaySender>(customSender);

        services.AddMockReplaySender();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IReplaySender>();

        Assert.Same(customSender, sender);
    }

    private sealed class CustomSender : IReplaySender
    {
        public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReplayResult { Success = true, MessageId = message.Id });
        }
    }
}
