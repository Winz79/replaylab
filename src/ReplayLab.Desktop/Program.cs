using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Photino.NET;
using ReplayLab.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:0");

builder.Services.AddReplayLabWeb();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapStaticAssets();
app.MapReplayLabWeb();

// Start the web host without blocking so we can read the bound address.
await app.StartAsync();

var server = app.Services.GetRequiredService<IServer>();
var addressFeature = server.Features.Get<IServerAddressesFeature>();
var localUrl = addressFeature?.Addresses.FirstOrDefault()
    ?? throw new InvalidOperationException("Unable to determine local server URL.");

var window = new PhotinoWindow()
    .SetTitle("ReplayLab")
    .SetUseOsDefaultSize(true)
    .SetUseOsDefaultLocation(true)
    .Load(localUrl);

// When the window closes, stop the web host.
window.RegisterWindowClosingHandler((sender, args) =>
{
    _ = app.StopAsync();
    return false;
});

window.WaitForClose();

await app.StopAsync();
