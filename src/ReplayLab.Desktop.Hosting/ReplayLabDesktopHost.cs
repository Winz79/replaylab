using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Photino.NET;
using ReplayLab.Web.Hosting;

namespace ReplayLab.Desktop.Hosting;

public static class ReplayLabDesktopHost
{
    public static void Run(string[] args, Action<IServiceCollection>? configureServices = null)
    {
        var app = BuildApp(args, configureServices);
        RunWithPhotino(app);
    }

    public static WebApplication BuildApp(string[] args, Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        var seqUrl = Environment.GetEnvironmentVariable("SEQ_SERVER_URL");
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            builder.Logging.AddSeq(seqUrl);
        }

        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseStaticWebAssets();

        configureServices?.Invoke(builder.Services);
        builder.Services.AddReplayLabWeb();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseRouting();

        var staticAssetsManifestPath = Path.Combine(AppContext.BaseDirectory, "ReplayLab.Web.Hosting.staticwebassets.endpoints.json");
        if (File.Exists(staticAssetsManifestPath))
        {
            app.MapStaticAssets(staticAssetsManifestPath);
        }
        else
        {
            app.MapStaticAssets();
        }

        app.MapReplayLabWeb();

        return app;
    }

    public static string GetLocalUrl(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var server = app.Services.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        return addressFeature?.Addresses.FirstOrDefault()
            ?? throw new InvalidOperationException("Unable to determine local server URL.");
    }

    private static void RunWithPhotino(WebApplication app)
    {
        try
        {
            app.StartAsync().GetAwaiter().GetResult();

            var localUrl = GetLocalUrl(app);

            var window = new PhotinoWindow()
                .SetTitle("ReplayLab")
                .SetUseOsDefaultSize(true)
                .SetUseOsDefaultLocation(true)
                .Load(localUrl);

            window.RegisterWindowClosingHandler((sender, args) =>
            {
                _ = app.StopAsync();
                return false;
            });

            window.WaitForClose();

            app.StopAsync().GetAwaiter().GetResult();
        }
        finally
        {
            app.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
