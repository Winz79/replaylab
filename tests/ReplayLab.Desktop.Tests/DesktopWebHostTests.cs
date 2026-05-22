using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReplayLab.Desktop;
using ReplayLab.Web.Hosting;
using Xunit;

namespace ReplayLab.Desktop.Tests;

public class DesktopWebHostTests
{
    [Fact]
    public async Task ReplayLab_Web_Hosting_Composition_Responds()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();

        builder.Services.AddReplayLabWeb();

        var app = builder.Build();

        app.UseRouting();
        app.MapReplayLabWeb();

        await app.StartAsync();

        var client = app.GetTestClient();
        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("ReplayLab Web", html);

        await app.StopAsync();
    }

    [Fact]
    public async Task Desktop_Bootstrap_Starts_On_Loopback_And_Serves_Web_Ui()
    {
        await using var app = DesktopBootstrap.BuildApp(Array.Empty<string>());

        await app.StartAsync();

        var localUrl = DesktopBootstrap.GetLocalUrl(app);

        Assert.StartsWith("http://127.0.0.1:", localUrl, StringComparison.Ordinal);

        using var client = new HttpClient();
        var response = await client.GetAsync(localUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("ReplayLab Web", html);

        await app.StopAsync();
    }
}
