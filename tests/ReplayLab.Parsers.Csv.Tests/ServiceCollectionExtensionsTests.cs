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
}
