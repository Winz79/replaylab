using ReplayLab.Core;

namespace ReplayLab.Core.Tests;

public class CoreSmokeTests
{
    [Fact]
    public void Replay_core_types_can_be_instantiated()
    {
        var message = new ReplayMessage("message-1", "{\"type\":\"sample\"}", new Dictionary<string, string>(), new Dictionary<string, string>());
        var batch = new ReplayBatch([message]);
        var result = new ReplayResult
        {
            Success = true,
            MessageId = message.Id
        };

        Assert.Equal("message-1", message.Id);
        Assert.Single(batch.Messages);
        Assert.True(result.Success);
    }
}
