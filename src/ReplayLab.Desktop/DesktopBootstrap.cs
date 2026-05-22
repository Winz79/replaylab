using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReplayLab.Web.Hosting;

namespace ReplayLab.Desktop;

public static class DesktopBootstrap
{
    public static WebApplication BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.WebHost.UseStaticWebAssets();
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
}
