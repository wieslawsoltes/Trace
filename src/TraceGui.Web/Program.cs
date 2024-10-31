using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using TraceGui;

[assembly:SupportedOSPlatform("browser")]

internal class Program
{
    private static void Main(string[] args) 
        => BuildAvaloniaApp().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
