using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReplayLab.Web.Hosting;
using Xunit;

namespace ReplayLab.Desktop.Tests;

public class DesktopWebHostTests
{
    [Fact]
    public async Task Desktop_WebHost_Configuration_Responds()
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
}
