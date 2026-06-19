using System.Net.Http.Headers;
using System.Text;
using Microsoft.Playwright;

namespace LwX.Web.Testing.Extensions;

public static class HeaderAuthExtensions
{
    public static Task WithHeadersAsync(this IPage page, IDictionary<string, string> headers) => page.Context.SetExtraHTTPHeadersAsync(headers);
    public static Task WithHeaderAsync(this IPage page, string name, string value) => page.Context.SetExtraHTTPHeadersAsync(new Dictionary<string, string> { [name] = value });
    public static Task WithBearerTokenAsync(this IPage page, string token) => page.WithHeaderAsync("Authorization", $"Bearer {token}");
    public static Task WithBasicAuthAsync(this IPage page, string username, string password)
    {
        var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return page.WithHeaderAsync("Authorization", $"Basic {value}");
    }

    public static Task SetCookieAsync(this IPage page, string name, string value, string? domain = null, string path = "/")
    {
        var resolvedDomain = domain;
        if (string.IsNullOrWhiteSpace(resolvedDomain))
        {
            if (Uri.TryCreate(page.Url, UriKind.Absolute, out var uri) && uri.Scheme != "about") resolvedDomain = uri.Host;
            else throw new InvalidOperationException("Cannot infer cookie domain before navigation. Pass the domain argument explicitly or navigate first.");
        }
        return page.Context.AddCookiesAsync(new[] { new Cookie { Name = name, Value = value, Domain = resolvedDomain, Path = path } });
    }

    public static Task UseCookieAuthAsync(this IPage page, string cookieName, string value) => page.SetCookieAsync(cookieName, value);
    public static Task ClearAuthAsync(this IPage page) => page.Context.ClearCookiesAsync();
    public static Task LoginAsAsync(this IPage page, string userName) => page.SetLocalStorageAsync("lwx.test.user", userName);
    public static Task LoginAsRoleAsync(this IPage page, string role) => page.SetLocalStorageAsync("lwx.test.role", role);
}
