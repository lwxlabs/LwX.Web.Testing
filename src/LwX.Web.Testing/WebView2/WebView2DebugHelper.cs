namespace LwX.Web.Testing.WebView2;

public sealed class WebView2DebugOptions
{
    public int Port { get; set; } = 9222;
    public bool EnableDevTools { get; set; }
}

public static class WebView2DebugHelper
{
    public static void Enable(int port = 9222)
    {
        if (!OperatingSystem.IsWindows()) return;
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", $"--remote-debugging-port={port}");
    }

    public static string GetCdpUrl(int port = 9222) => $"http://localhost:{port}";
}
