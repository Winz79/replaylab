using ReplayLab.Cli.Hosting;
using ReplayLab.Core;

namespace ReplayLab.Cli.Tests;

public class CliHostingApplicationTests
{
    [Fact]
    public async Task RunAsync_uses_services_from_the_composition_root()
    {
        var csvPath = CreateTempCsv("""
            kind,name
            Created,alpha
            """);
        await using var context = new TempFileContext(csvPath);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var parser = new RecordingParser();
        var senderFactory = new RecordingSenderFactory();
        var services = new DictionaryServiceProvider(new Dictionary<Type, object>
        {
            [typeof(IMessageParser)] = parser,
            [typeof(ReplayLab.Cli.Hosting.IReplaySenderFactory)] = senderFactory,
        });

        var exitCode = await ReplayLab.Cli.Hosting.CliApplication.RunAsync(
            ["--sender", "http", "--endpoint-url", "https://example.test/replay", csvPath],
            output,
            error,
            services);

        Assert.Equal(0, exitCode);
        Assert.True(parser.WasUsed);
        Assert.Equal("http", senderFactory.SelectedSender);
        Assert.Equal(new Uri("https://example.test/replay"), senderFactory.EndpointUrl);
        Assert.Contains("Loaded 1 message(s).", output.ToString());
        Assert.Contains("Sent 1 message(s): 1 succeeded, 0 failed.", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    private static string CreateTempCsv(string contents, string fileNamePrefix = "")
    {
        var path = Path.Combine(Path.GetTempPath(), $"{fileNamePrefix}{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, contents);
        return path;
    }

    private sealed class TempFileContext(string path) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class DictionaryServiceProvider : IServiceProvider
    {
        private readonly IReadOnlyDictionary<Type, object> _services;

        public DictionaryServiceProvider(IReadOnlyDictionary<Type, object> services)
        {
            _services = services;
        }

        public object? GetService(Type serviceType)
        {
            return serviceType == typeof(IServiceProvider)
                ? this
                : _services.TryGetValue(serviceType, out var service)
                    ? service
                    : null;
        }
    }

    private sealed class RecordingParser : IMessageParser
    {
        public bool WasUsed { get; private set; }

        public Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
        {
            WasUsed = true;
            return Task.FromResult(new ReplayBatch([
                new ReplayMessage(
                    "custom-1",
                    "payload",
                    new Dictionary<string, string>(),
                    new Dictionary<string, string>())]));
        }
    }

    private sealed class RecordingSenderFactory : ReplayLab.Cli.Hosting.IReplaySenderFactory
    {
        private readonly RecordingSender _sender = new();

        public string? SelectedSender { get; private set; }

        public Uri? EndpointUrl { get; private set; }

        public IReplaySender CreateMockSender()
        {
            SelectedSender = "mock";
            return _sender;
        }

        public IReplaySender CreateHttpSender(Uri endpointUrl)
        {
            SelectedSender = "http";
            EndpointUrl = endpointUrl;
            return _sender;
        }
    }

    private sealed class RecordingSender : IReplaySender
    {
        public Task<ReplayResult> SendAsync(ReplayMessage message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReplayResult
            {
                Success = true,
                MessageId = message.Id,
            });
        }
    }
}
