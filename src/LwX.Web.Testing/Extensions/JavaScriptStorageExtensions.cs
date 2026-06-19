using System.Text.Json;
using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class JavaScriptStorageExtensions
{
    public static Task<T?> InvokeJsAsync<T>(this IPage page, string expression, object? arg = null) => page.EvaluateAsync<T>(expression, arg);
    public static Task WaitForDomStableAsync(this IPage page) => page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    public static Task ScrollToBottomAsync(this IPage page) => page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
    public static Task ScrollIntoViewAsync(this IPage page, string selector) => page.Locator(selector).ScrollIntoViewIfNeededAsync();
    public static Task DispatchCustomEventAsync(this IPage page, string eventName, object? detail = null)
        => page.EvaluateAsync("([name, detail]) => window.dispatchEvent(new CustomEvent(name, { detail }))", new object?[] { eventName, detail });

    public static Task<string?> GetLocalStorageAsync(this IPage page, string key) => page.EvaluateAsync<string?>("key => localStorage.getItem(key)", key);
    public static Task SetLocalStorageAsync(this IPage page, string key, string value) => page.EvaluateAsync("([key, value]) => localStorage.setItem(key, value)", new[] { key, value });
    public static Task<string?> GetSessionStorageAsync(this IPage page, string key) => page.EvaluateAsync<string?>("key => sessionStorage.getItem(key)", key);
    public static Task SetSessionStorageAsync(this IPage page, string key, string value) => page.EvaluateAsync("([key, value]) => sessionStorage.setItem(key, value)", new[] { key, value });
    public static Task ClearStorageAsync(this IPage page) => page.EvaluateAsync("() => { localStorage.clear(); sessionStorage.clear(); }");

    public static async Task<T?> GetLocalStorageJsonAsync<T>(this IPage page, string key)
    {
        var json = await page.GetLocalStorageAsync(key);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public static Task SetLocalStorageJsonAsync<T>(this IPage page, string key, T value)
        => page.SetLocalStorageAsync(key, JsonSerializer.Serialize(value));

    public static async Task ClearBrowserStateAsync(this IPage page)
    {
        await page.ClearStorageAsync();
        await page.Context.ClearCookiesAsync();
    }
}
