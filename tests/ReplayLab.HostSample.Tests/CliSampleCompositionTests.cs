using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Cli.Hosting;
using ReplayLab.HostSample;

namespace ReplayLab.HostSample.Tests;

public sealed class CliSampleCompositionTests
{
    [Fact]
    public async Task Cli_mode_uses_sample_owned_parser_and_sender_factory_from_di()
    {
        var services = new ServiceCollection();
        services.AddReplayLabHostSample();
        using var provider = services.BuildServiceProvider();

        var inputPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(inputPath, "sample input");

        var output = new StringWriter(new StringBuilder());
        var error = new StringWriter(new StringBuilder());

        try
        {
            var exitCode = await CliApplication.RunAsync([inputPath], output, error, provider);

            Assert.Equal(0, exitCode);

            var log = provider.GetRequiredService<SyntheticServiceLog>();
            Assert.Contains("parser:parse", log.Entries);
            Assert.Contains("sender-factory:create-mock", log.Entries);
            Assert.Contains("sender:send:sample-1", log.Entries);
            Assert.Contains("Loaded 1 message(s).", output.ToString());
        }
        finally
        {
            File.Delete(inputPath);
        }
    }
}
