using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Adapters.Example;
using ReplayLab.Core;

namespace ReplayLab.Adapters.Example.Tests;

public class FileReplaySenderTests
{
    [Fact]
    public async Task SendAsync_writes_message_to_file()
    {
        var filePath = Path.GetTempFileName();
        try
        {
            var sender = new FileReplaySender(filePath);
            var message = new ReplayMessage(
                "test-message-1",
                """{"name":"John","value":42}""",
                new Dictionary<string, string>(),
                new Dictionary<string, string>());

            var result = await sender.SendAsync(message);

            Assert.True(result.Success);
            Assert.Equal(message.Id, result.MessageId);

            var content = await File.ReadAllTextAsync(filePath);
            Assert.Contains("test-message-1", content);
            Assert.Contains("{\"name\":\"John\",\"value\":42}", content);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public async Task AddExampleReplaySender_registers_FileReplaySender()
    {
        var services = new ServiceCollection();
        var filePath = Path.GetTempFileName();

        try
        {
            services.AddExampleReplaySender(filePath);

            var provider = services.BuildServiceProvider();
            var sender = provider.GetRequiredService<IReplaySender>();

            Assert.IsType<FileReplaySender>(sender);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
