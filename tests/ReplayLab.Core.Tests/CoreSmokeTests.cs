using ReplayLab.Core;

namespace ReplayLab.Core.Tests;

public class CoreSmokeTests
{
    [Fact]
    public void Replay_core_types_can_be_instantiated()
    {
        var message = new ReplayMessage("message-1", "{\"type\":\"sample\"}");
        var batch = new ReplayBatch([message]);
        var result = new ReplayResult(true, message.Id);

        Assert.Equal("message-1", message.Id);
        Assert.Single(batch.Messages);
        Assert.True(result.Success);
    }
}
