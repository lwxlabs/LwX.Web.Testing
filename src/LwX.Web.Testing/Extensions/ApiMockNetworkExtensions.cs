using System.Text.Json;
using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class ApiMockNetworkExtensions
{
    public static Task MockJsonAsync(this IPage page, string urlPattern, object value, int status = 200)
    {
        var json = JsonSerializer.Serialize(value);
        return page.RouteAsync(urlPattern, route => route.FulfillAsync(new()
        {
            Status = status,
            ContentType = "application/json",
            Body = json
        }));
    }

    public static Task MockStatusAsync(this IPage page, string urlPattern, int status)
        => page.RouteAsync(urlPattern, route => route.FulfillAsync(new() { Status = status, Body = string.Empty }));

    public static Task MockEmptyAsync(this IPage page, string urlPattern)
        => page.MockJsonAsync(urlPattern, Array.Empty<object>());

    public static Task MockUnauthorizedAsync(this IPage page, string urlPattern)
        => page.MockStatusAsync(urlPattern, 401);

    public static Task MockDelayAsync(this IPage page, string urlPattern, int delayMilliseconds, object? value = null)
        => page.RouteAsync(urlPattern, async route =>
        {
            await Task.Delay(delayMilliseconds);
            await route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/json",
                Body = JsonSerializer.Serialize(value ?? new { ok = true })
            });
        });

    public static Task BlockRequestsAsync(this IPage page, string urlPattern)
        => page.RouteAsync(urlPattern, route => route.AbortAsync());

    public static Task SimulateApiTimeoutAsync(this IPage page, string urlPattern)
        => page.RouteAsync(urlPattern, async route => { await Task.Delay(TimeSpan.FromMinutes(5)); await route.AbortAsync(); });

    public static Task SetOfflineAsync(this IPage page) => page.Context.SetOfflineAsync(true);
    public static Task SetOnlineAsync(this IPage page) => page.Context.SetOfflineAsync(false);
    public static Task SimulateSlowNetworkAsync(this IPage page, int delayMilliseconds = 1000)
        => page.RouteAsync("**/*", async route => { await Task.Delay(delayMilliseconds); await route.ContinueAsync(); });

    public static Task BlockImagesAsync(this IPage page)
        => page.RouteAsync("**/*.{png,jpg,jpeg,webp,gif,svg}", route => route.AbortAsync());

    public static Task BlockFontsAsync(this IPage page)
        => page.RouteAsync("**/*.{woff,woff2,ttf,otf}", route => route.AbortAsync());
}
