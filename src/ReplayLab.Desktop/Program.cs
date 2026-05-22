using ReplayLab.Desktop.Hosting;

namespace ReplayLab.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ReplayLabDesktopHost.Run(args);
    }
}
