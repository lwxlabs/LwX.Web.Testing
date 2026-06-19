using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class ArtifactAssertionExtensions
{
    public static async Task<string> TakeNamedScreenshotAsync(this IPage page, string name, string directory = "test-results/screenshots", bool fullPage = true)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{Sanitize(name)}.png");
        await page.ScreenshotAsync(new() { Path = path, FullPage = fullPage });
        return path;
    }

    public static Task StartTracingAsync(this IBrowserContext context, string? title = null)
        => context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true, Title = title });

    public static Task StopTracingAsync(this IBrowserContext context, string path = "test-results/traces/trace.zip")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return context.Tracing.StopAsync(new() { Path = path });
    }

    public static Task ShouldHaveTitleAsync(this IPage page, string title) => Assertions.Expect(page).ToHaveTitleAsync(title);
    public static Task ShouldContainTextAsync(this IPage page, string text) => Assertions.Expect(page.Locator("body")).ToContainTextAsync(text);
    public static Task ShouldHaveUrlContainingAsync(this IPage page, string urlPart) => Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(urlPart)));

    public static async Task ShouldHaveNoConsoleErrorsAsync(this IPage page)
    {
        var errors = new List<string>();
        page.Console += (_, msg) => { if (msg.Type == "error") errors.Add(msg.Text); };
        await page.WaitForTimeoutAsync(50);
        if (errors.Count > 0) throw new InvalidOperationException("Console errors found: " + string.Join(Environment.NewLine, errors));
    }

    public static async Task ShouldHaveNoFailedRequestsAsync(this IPage page)
    {
        var failed = new List<string>();
        page.RequestFailed += (_, r) => failed.Add(r.Url);
        await page.WaitForTimeoutAsync(50);
        if (failed.Count > 0) throw new InvalidOperationException("Failed requests found: " + string.Join(Environment.NewLine, failed));
    }

    public static async Task ExpectNoErrorBoundaryAsync(this IPage page)
    {
        var count = await page.Locator(".blazor-error-boundary, [data-testid='error-boundary'], .error-boundary").CountAsync();
        if (count > 0) throw new InvalidOperationException("Error boundary is visible.");
    }

    private static string Sanitize(string value) { foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_'); return value; }
}
