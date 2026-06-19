# LwX.Web.Testing

Universal Playwright-based testing toolkit for normal web apps and component libraries.

It is designed to replace duplicated testing packages by putting shared browser/testing behavior in one package and keeping framework-specific helpers as adapters.

## Supports

- Blazor Server
- Blazor WebAssembly
- Blazor Web App
- MAUI Blazor Hybrid WebView2 smoke tests
- Angular
- React / Next.js / Vite React
- Vue / Nuxt / Vite Vue
- Svelte, Solid, Astro, plain HTML/JS

## Install

```powershell
dotnet add package LwX.Web.Testing
```

Install Playwright browsers:

```powershell
pwsh bin/Debug/net8.0/playwright.ps1 install --with-deps
```

## Basic usage

```csharp
using LwX.Web.Testing.Options;
using LwX.Web.Testing.Playwright;
using LwX.Web.Testing.Extensions;

public sealed class DashboardTests : PlaywrightTestBase
{
    protected override void Configure(LwXWebTestingOptions options)
    {
        options.BaseUrl = "http://localhost:5173";
        options.UseReact();
        options.UseProfile(LwXTestProfile.CI);

        options.Playwright.ConfigureLaunchOptions = launch =>
        {
            launch.Channel = "chrome";
        };

        options.Headers.DefaultHeaders["X-Test-Mode"] = "true";
        options.Artifacts.TraceOnFailure = true;
        options.Artifacts.ScreenshotOnFailure = true;
    }

    [Fact]
    public async Task Dashboard_Should_Load()
    {
        await RunSafelyAsync(async () =>
        {
            await GoToAsync("/dashboard");
            await Page.WaitForReactReadyAsync();
            await Page.ClickByTestIdAsync("refresh-button");
            await Page.ShouldContainTextAsync("Updated");
        });
    }
}
```

## Auto-start Angular, React, Vue, Blazor, or API projects

```csharp
options.AppRunner = new()
{
    Command = "npm",
    Arguments = "run dev -- --mode test --port 5173",
    WorkingDirectory = "../my-react-app",
    ReadyUrl = "http://localhost:5173",
    EnvironmentVariables =
    {
        ["VITE_API_URL"] = "http://localhost:5005",
        ["VITE_TEST_MODE"] = "true"
    }
};

options.ApiRunner = new()
{
    Command = "dotnet",
    Arguments = "run --project ../MyApi/MyApi.csproj",
    ReadyUrl = "http://localhost:5005/health"
};
```

## Framework adapters

```csharp
options.UseBlazor();
options.UseBlazorHybrid();
options.UseAngular();
options.UseReact();
options.UseVue();
```

Then use readable helpers:

```csharp
await Page.WaitForBlazorReadyAsync();
await Page.WaitForAngularReadyAsync();
await Page.WaitForReactReadyAsync();
await Page.WaitForVueReadyAsync();
```

## Test host routes

In a test host web app:

```csharp
builder.Services.AddLwXWebTesting(options =>
{
    options.Security.RequiredEnvironmentName = "Testing";
});

var app = builder.Build();
app.MapLwXWebTestRoutes();
app.Run();
```

Exposes test-only endpoints:

- `/__test/state`
- `/__test/reset`
- `/__test/features/{key}`
- `/__test/auth/login/{userName}`
- `/__test/logs`

These are blocked outside the configured testing environment by default.

## Included helper groups

- Playwright configuration and factories
- console/page/network diagnostics
- screenshots, traces, video, HAR
- headers, cookies, bearer/basic auth
- localStorage/sessionStorage helpers
- API mocks, delays, status failures, offline mode
- click, fill, hover, right-click, keyboard shortcuts
- drag/drop and split-pane resize
- Blazor, Angular, React, Vue readiness helpers
- Material/Vuetify/Element Plus helper selectors
- SignalR/reconnect helpers
- file upload/download helpers
- editor/Monaco/markdown/mermaid helpers
- feature flags, fake auth, test data, fake clock
- fake services for hybrid-like abstractions
- WebView2 debugging helper
- app/API process runner
- CI and template examples

## Production-safety

`MapLwXWebTestRoutes()` throws unless the environment matches `Testing` by default. Keep test endpoints in a dedicated test host or guarded test environment only.

## Multi-targeting

```xml
<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
```


## Production fixes included

This ZIP now contains the core package and the optional xUnit integration package:

```txt
src/LwX.Web.Testing
src/LwX.Web.Testing.Xunit
```

The core package includes structured report generation compatible with `LwX.Web.Testing.Viewer`:

```csharp
options.Report.Enabled = true;
options.Report.OutputDirectory = "test-results";
options.Report.FileName = "run-summary.json";
options.Steps.ScreenshotEachStep = true;

await StepAsync("Open editor", () => GoToAsync("/editor"));
```

The xUnit package provides `XunitPlaywrightTestBase`, which correctly implements `Xunit.IAsyncLifetime`.

App runners now pump stdout/stderr in the background to avoid deadlocks with Vite, Angular, Next.js, Nuxt, or `dotnet run`.

`MapLwXWebTestRoutes()` now enforces `RequireTestHeader` when enabled.
