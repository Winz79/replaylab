using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReplayLab.Cli;

var seqUrl = Environment.GetEnvironmentVariable("SEQ_SERVER_URL");

var services = new ServiceCollection();
if (!string.IsNullOrWhiteSpace(seqUrl))
{
    services.AddLogging(logging => logging.AddSeq(seqUrl));
}

var serviceProvider = services.BuildServiceProvider();
Environment.ExitCode = await CliApplication.RunAsync(args, Console.Out, Console.Error, serviceProvider);
