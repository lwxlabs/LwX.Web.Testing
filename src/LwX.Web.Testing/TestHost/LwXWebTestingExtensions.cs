using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LwX.Web.Testing.Auth;
using LwX.Web.Testing.Data;
using LwX.Web.Testing.Features;
using LwX.Web.Testing.Options;
using LwX.Web.Testing.Services;

namespace LwX.Web.Testing.TestHost;

public static class LwXWebTestingExtensions
{
    public static IServiceCollection AddLwXWebTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
    {
        var options = new LwXWebTestingOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<TestFeatureFlags>();
        services.AddSingleton<TestDataStore>();
        services.AddSingleton<TestClock>();
        services.AddSingleton<TestGuidGenerator>();
        services.AddSingleton<FakeAuthStore>();
        services.AddSingleton<ILwXTestNavigation, FakeTestNavigation>();
        services.AddSingleton<ILwXTestDialog, FakeTestDialog>();
        services.AddSingleton<ILwXTestStorage, InMemoryTestStorage>();
        services.AddSingleton<ILwXTestFilePicker, FakeTestFilePicker>();
        services.AddSingleton<ILwXTestFileSystem, InMemoryFileSystem>();
        services.AddSingleton<ILwXClipboard, FakeClipboard>();
        services.AddSingleton<ILwXConnectivity, FakeConnectivity>();
        services.AddSingleton<ILwXDeviceInfo, FakeDeviceInfo>();
        services.AddSingleton<ILwXGeolocation, FakeGeolocation>();
        services.AddSingleton<ILwXPermissions, FakePermissions>();
        services.AddSingleton<ILwXShare, FakeShare>();
        services.AddSingleton<ILwXLauncher, FakeLauncher>();
        return services;
    }

    public static IServiceCollection AddLwXBlazorTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
        => services.AddLwXWebTesting(o => { o.UseBlazor(); configure?.Invoke(o); });

    public static IServiceCollection AddLwXBlazorHybridTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
        => services.AddLwXWebTesting(o => { o.UseBlazorHybrid(); configure?.Invoke(o); });

    public static IServiceCollection AddLwXAngularTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
        => services.AddLwXWebTesting(o => { o.UseAngular(); configure?.Invoke(o); });

    public static IServiceCollection AddLwXReactTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
        => services.AddLwXWebTesting(o => { o.UseReact(); configure?.Invoke(o); });

    public static IServiceCollection AddLwXVueTesting(this IServiceCollection services, Action<LwXWebTestingOptions>? configure = null)
        => services.AddLwXWebTesting(o => { o.UseVue(); configure?.Invoke(o); });

    public static IServiceCollection ReplaceForWebTesting<TService, TImplementation>(this IServiceCollection services)
        where TService : class where TImplementation : class, TService
    {
        var existing = services.Where(d => d.ServiceType == typeof(TService)).ToArray();
        foreach (var descriptor in existing) services.Remove(descriptor);
        services.AddSingleton<TService, TImplementation>();
        return services;
    }

    public static WebApplication MapLwXWebTestRoutes(this WebApplication app)
    {
        var env = app.Environment.EnvironmentName;
        var options = app.Services.GetService<LwXWebTestingOptions>() ?? new LwXWebTestingOptions();
        if (options.Security.BlockInProduction && !string.Equals(env, options.Security.RequiredEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"LwX test routes can only run in '{options.Security.RequiredEnvironmentName}' environment. Current environment is '{env}'.");
        }

        var group = app.MapGroup("/__test");
        if (options.Security.RequireTestHeader)
        {
            group.AddEndpointFilter(async (context, next) =>
            {
                var http = context.HttpContext;
                if (!http.Request.Headers.TryGetValue(options.Security.TestHeaderName, out var value) ||
                    !string.Equals(value.ToString(), options.Security.TestHeaderValue, StringComparison.Ordinal))
                {
                    return Results.StatusCode(StatusCodes.Status404NotFound);
                }
                return await next(context);
            });
        }
        group.MapGet("/state", (TestFeatureFlags features, TestDataStore data, FakeAuthStore auth) => Results.Json(new { features = features.Values, data = data.Items.Keys, user = auth.CurrentUser.UserName }));
        group.MapPost("/reset", (TestFeatureFlags features, TestDataStore data, TestClock clock, FakeAuthStore auth) => { features.Clear(); data.Reset(); clock.Reset(); auth.Clear(); return Results.Ok(new { reset = true }); });
        group.MapPost("/features/{key}", (string key, HttpContext ctx, TestFeatureFlags features) => { features.Set(key, true); return Results.Ok(new { key, enabled = true }); });
        group.MapDelete("/features/{key}", (string key, TestFeatureFlags features) => { features.Set(key, false); return Results.Ok(new { key, enabled = false }); });
        group.MapPost("/auth/login/{userName}", (string userName, FakeAuthStore auth) => Results.Json(auth.LoginAs(userName)));
        group.MapGet("/logs", () => Results.Ok(new { message = "Wire this endpoint to your app log provider if needed." }));
        return app;
    }
}
