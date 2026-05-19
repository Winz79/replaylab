using ReplayLab.Adapters.Mock;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Mock.Tests;

public class MockReplaySenderSmokeTests
{
    [Fact]
    public async Task SendAsync_returns_successful_replay_result()
    {
        var sender = new MockReplaySender();
        var message = new ReplayMessage("message-1", "{}", new Dictionary<string, string>(), new Dictionary<string, string>());

        var result = await sender.SendAsync(message);

        Assert.True(result.Success);
        Assert.Equal(message.Id, result.MessageId);
        Assert.Null(result.ErrorMessage);
    }
}
