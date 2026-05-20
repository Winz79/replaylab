using Microsoft.Extensions.DependencyInjection;
using ReplayLab.Cli.Hosting;
using ReplayLab.Web.Hosting;

namespace ReplayLab.HostSample;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            await Console.Error.WriteLineAsync("Usage: hostsample <cli|web> [args]");
            return 2;
        }

        return args[0] switch
        {
            "cli" => await RunCliAsync(args[1..]),
            "web" => await RunWebAsync(args[1..]),
            _ => 2
        };
    }

    private static async Task<int> RunCliAsync(string[] args)
    {
        var services = new ServiceCollection();
        services.AddReplayLabHostSample();
        using var provider = services.BuildServiceProvider();
        return await CliApplication.RunAsync(args, Console.Out, Console.Error, provider);
    }

    private static async Task<int> RunWebAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddReplayLabHostSample();
        builder.Services.AddReplayLabWeb();

        var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets();
        app.MapReplayLabWeb();
        await app.RunAsync();
        return 0;
    }
}
