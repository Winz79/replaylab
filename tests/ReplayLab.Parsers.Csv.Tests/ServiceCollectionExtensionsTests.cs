using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Core;
using ReplayLab.Parsers.Csv;

namespace ReplayLab.Parsers.Csv.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCsvMessageParser_registers_IMessageParser()
    {
        var services = new ServiceCollection();

        services.AddCsvMessageParser();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<IMessageParser>();

        Assert.IsType<CsvReplayMessageParser>(parser);
    }

    [Fact]
    public void AddCsvMessageParser_does_not_override_existing_registration()
    {
        var customParser = new CustomParser();
        var services = new ServiceCollection();
        services.AddSingleton<IMessageParser>(customParser);

        services.AddCsvMessageParser();

        var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<IMessageParser>();

        Assert.Same(customParser, parser);
    }

    private sealed class CustomParser : IMessageParser
    {
        public Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ReplayBatch([]));
        }
    }
}
