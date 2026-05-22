using Photino.NET;
using ReplayLab.Desktop;

namespace ReplayLab.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        RunAsync(args).GetAwaiter().GetResult();
    }

    private static async Task RunAsync(string[] args)
    {
        await using var app = DesktopBootstrap.BuildApp(args);

        // Start the web host without blocking so we can read the bound address.
        await app.StartAsync();

        var localUrl = DesktopBootstrap.GetLocalUrl(app);
        
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
    }
}
