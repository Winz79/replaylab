using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using ReplayLab.HostSample;
using ReplayLab.Web.Hosting;

namespace ReplayLab.HostSample.Tests;

public sealed class WebSampleHostingTests
{
    [Fact]
    public async Task Sample_host_can_mount_replaylab_web_hosting_surface()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.WebHost.UseStaticWebAssets();
        builder.Services.AddReplayLabHostSample();
        builder.Services.AddReplayLabWeb();

        await using var app = builder.Build();
        app.UseRouting();
        app.MapStaticAssets(Path.Combine(AppContext.BaseDirectory, "ReplayLab.HostSample.Tests.staticwebassets.endpoints.json"));
        app.MapReplayLabWeb();
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var home = await client.GetAsync("/");
        using var css = await client.GetAsync("/_content/ReplayLab.Web.Hosting/css/site.css");

        Assert.Equal(System.Net.HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, css.StatusCode);
    }
}
