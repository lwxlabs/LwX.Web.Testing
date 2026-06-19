using Microsoft.Playwright;
using LwX.Web.Testing.Processes;

namespace LwX.Web.Testing.Options;

public class LwXWebTestingOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5005";
    public LwXFrontendFramework Framework { get; set; } = LwXFrontendFramework.Generic;
    public LwXTestProfile Profile { get; set; } = LwXTestProfile.Local;
    public LwXPlaywrightOptions Playwright { get; } = new();
    public LwXLoggingOptions Logging { get; } = new();
    public LwXArtifactOptions Artifacts { get; } = new();
    public LwXReportOptions Report { get; } = new();
    public LwXStepOptions Steps { get; } = new();
    public LwXHeaderOptions Headers { get; } = new();
    public LwXJavaScriptOptions JavaScript { get; } = new();
    public LwXAuthOptions Auth { get; } = new();
    public LwXFeatureOptions Features { get; } = new();
    public LwXNetworkOptions Network { get; } = new();
    public LwXKeyboardOptions Keyboard { get; } = new();
    public LwXSecurityOptions Security { get; } = new();
    public LwXVisualOptions Visual { get; } = new();
    public LwXPermissionsOptions Permissions { get; } = new();
    public LwXWebView2Options WebView2 { get; } = new();
    public LwXWebAppRunnerOptions? AppRunner { get; set; }
    public LwXWebAppRunnerOptions? ApiRunner { get; set; }
    public Func<IPage, CancellationToken, Task>? BeforeEachPageAsync { get; set; }
    public Func<IPage, CancellationToken, Task>? AfterEachPageAsync { get; set; }
    public Func<IPage, Exception, CancellationToken, Task>? OnTestFailureAsync { get; set; }

    public void UseProfile(LwXTestProfile profile)
    {
        Profile = profile;
        switch (profile)
        {
            case LwXTestProfile.CI:
                Playwright.Headless = true;
                Artifacts.ScreenshotOnFailure = true;
                Artifacts.TraceOnFailure = true;
                Artifacts.HarOnFailure = true;
                Logging.WriteToFile = true;
                Report.Enabled = true;
                break;
            case LwXTestProfile.Debug:
                Playwright.Headless = false;
                Playwright.SlowMo = 120;
                Playwright.KeepBrowserOpenOnFailure = true;
                Report.Enabled = true;
                Steps.ScreenshotEachStep = true;
                break;
            case LwXTestProfile.Visual:
                Visual.Enabled = true;
                Artifacts.ScreenshotOnFailure = true;
                Report.Enabled = true;
                break;
            case LwXTestProfile.Angular: UseAngular(); break;
            case LwXTestProfile.React: UseReact(); break;
            case LwXTestProfile.Vue: UseVue(); break;
            case LwXTestProfile.NextJs: UseNextJs(); break;
            case LwXTestProfile.Nuxt: UseNuxt(); break;
            case LwXTestProfile.Svelte: UseSvelte(); break;
            case LwXTestProfile.SvelteKit: UseSvelteKit(); break;
            case LwXTestProfile.Solid: UseSolid(); break;
            case LwXTestProfile.Astro: UseAstro(); break;
            case LwXTestProfile.PlainHtml: UsePlainHtml(); break;
            case LwXTestProfile.StaticSite: UseStaticSite(); break;
            case LwXTestProfile.Hybrid:
            case LwXTestProfile.WebView2:
                UseBlazorHybrid();
                WebView2.Enabled = true;
                break;
        }
    }

    public LwXWebTestingOptions UseFramework(LwXFrontendFramework framework) { Framework = framework; return this; }
    public LwXWebTestingOptions UseGeneric() => UseFramework(LwXFrontendFramework.Generic);
    public LwXWebTestingOptions UsePlainHtml() => UseFramework(LwXFrontendFramework.PlainHtml);
    public LwXWebTestingOptions UseStaticSite() => UseFramework(LwXFrontendFramework.StaticSite);
    public LwXWebTestingOptions UseVanillaJs() => UseFramework(LwXFrontendFramework.VanillaJs);
    public LwXWebTestingOptions UseWebComponents() => UseFramework(LwXFrontendFramework.WebComponents);
    public LwXWebTestingOptions UseLit() => UseFramework(LwXFrontendFramework.Lit);
    public LwXWebTestingOptions UseJQuery() => UseFramework(LwXFrontendFramework.JQuery);
    public LwXWebTestingOptions UseHtmx() => UseFramework(LwXFrontendFramework.HTMX);
    public LwXWebTestingOptions UseAlpineJs() => UseFramework(LwXFrontendFramework.AlpineJs);
    public LwXWebTestingOptions UseBlazor() => UseFramework(LwXFrontendFramework.Blazor);
    public LwXWebTestingOptions UseBlazorServer() => UseFramework(LwXFrontendFramework.BlazorServer);
    public LwXWebTestingOptions UseBlazorWebAssembly() => UseFramework(LwXFrontendFramework.BlazorWebAssembly);
    public LwXWebTestingOptions UseBlazorWebApp() => UseFramework(LwXFrontendFramework.BlazorWebApp);
    public LwXWebTestingOptions UseBlazorHybrid() => UseFramework(LwXFrontendFramework.BlazorHybrid);
    public LwXWebTestingOptions UseAngular() => UseFramework(LwXFrontendFramework.Angular);
    public LwXWebTestingOptions UseReact() => UseFramework(LwXFrontendFramework.React);
    public LwXWebTestingOptions UseNextJs() => UseFramework(LwXFrontendFramework.NextJs);
    public LwXWebTestingOptions UseRemix() => UseFramework(LwXFrontendFramework.Remix);
    public LwXWebTestingOptions UseVue() => UseFramework(LwXFrontendFramework.Vue);
    public LwXWebTestingOptions UseNuxt() => UseFramework(LwXFrontendFramework.Nuxt);
    public LwXWebTestingOptions UseSvelte() => UseFramework(LwXFrontendFramework.Svelte);
    public LwXWebTestingOptions UseSvelteKit() => UseFramework(LwXFrontendFramework.SvelteKit);
    public LwXWebTestingOptions UseSolid() => UseFramework(LwXFrontendFramework.Solid);
    public LwXWebTestingOptions UseQwik() => UseFramework(LwXFrontendFramework.Qwik);
    public LwXWebTestingOptions UseAstro() => UseFramework(LwXFrontendFramework.Astro);
    public LwXWebTestingOptions UseEmber() => UseFramework(LwXFrontendFramework.Ember);
}

public sealed class LwXPlaywrightOptions
{
    public LwXBrowserKind BrowserKind { get; set; } = LwXBrowserKind.Chromium;
    public bool Headless { get; set; } = true;
    public float? SlowMo { get; set; }
    public string? Channel { get; set; }
    public int DefaultTimeout { get; set; } = 30_000;
    public int NavigationTimeout { get; set; } = 30_000;
    public int ViewportWidth { get; set; } = 1366;
    public int ViewportHeight { get; set; } = 900;
    public string? Locale { get; set; }
    public string? TimezoneId { get; set; }
    public LwXColorScheme ColorScheme { get; set; } = LwXColorScheme.NoPreference;
    public float? DeviceScaleFactor { get; set; }
    public bool IsMobile { get; set; }
    public bool HasTouch { get; set; }
    public bool IgnoreHTTPSErrors { get; set; } = true;
    public bool ReuseBrowserBetweenTests { get; set; }
    public bool KeepBrowserOpenOnFailure { get; set; }
    public Action<BrowserTypeLaunchOptions>? ConfigureLaunchOptions { get; set; }
    public Action<BrowserNewContextOptions>? ConfigureContextOptions { get; set; }
    public Func<IPlaywright, LwXWebTestingOptions, Task<IBrowser>>? BrowserFactory { get; set; }
    public Func<IBrowser, LwXWebTestingOptions, Task<IBrowserContext>>? ContextFactory { get; set; }
    public Func<IBrowserContext, LwXWebTestingOptions, Task<IPage>>? PageFactory { get; set; }
}

public sealed class LwXLoggingOptions
{
    public bool CaptureConsole { get; set; } = true;
    public bool CapturePageErrors { get; set; } = true;
    public bool CaptureNetwork { get; set; } = true;
    public bool CaptureFailedRequests { get; set; } = true;
    public bool WriteToConsole { get; set; }
    public bool WriteToFile { get; set; }
    public string LogDirectory { get; set; } = "test-results/logs";
}

public sealed class LwXArtifactOptions
{
    public bool ScreenshotOnFailure { get; set; } = true;
    public bool VideoOnFailure { get; set; }
    public bool TraceOnFailure { get; set; } = true;
    public bool HarOnFailure { get; set; }
    public string OutputDirectory { get; set; } = "test-results";
}

public sealed class LwXReportOptions
{
    public bool Enabled { get; set; }
    public string OutputDirectory { get; set; } = "test-results";
    public string FileName { get; set; } = "run-summary.json";
    public Dictionary<string, string> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool IncludeConsoleLogs { get; set; } = true;
    public bool IncludeNetworkLogs { get; set; } = true;
    public bool IncludeFrameworkErrors { get; set; } = true;
    public string FullPath => Path.Combine(OutputDirectory, FileName);
}

public sealed class LwXStepOptions
{
    public bool ScreenshotEachStep { get; set; }
    public bool CaptureConsoleEachStep { get; set; } = true;
    public bool CaptureNetworkEachStep { get; set; } = true;
}

public sealed class LwXHeaderOptions { public Dictionary<string, string> DefaultHeaders { get; } = new(StringComparer.OrdinalIgnoreCase); }
public sealed class LwXJavaScriptOptions { public List<string> InitScripts { get; } = new(); public bool DisableAnimations { get; set; } = true; public bool EnableTestModeFlag { get; set; } = true; }
public sealed class LwXAuthOptions
{
    public string DefaultUser { get; set; } = "test-user";
    public bool UseFakeAuthentication { get; set; }
    public string LoginPath { get; set; } = "/login";
    public string MeEndpoint { get; set; } = "/api/account/me";
    public Dictionary<string, LwXFakeUserDefinition> Users { get; } = new(StringComparer.OrdinalIgnoreCase);
    public void AddUser(string name, string[]? roles = null, Dictionary<string, string>? claims = null) => Users[name] = new(name, roles ?? [], claims ?? new());
}
public sealed record LwXFakeUserDefinition(string Name, string[] Roles, Dictionary<string, string> Claims);
public sealed class LwXFeatureOptions { public Dictionary<string, object?> Values { get; } = new(StringComparer.OrdinalIgnoreCase); public void Set(string key, object? value) => Values[key] = value; }
public sealed class LwXNetworkOptions { public bool BlockImages { get; set; } public bool BlockFonts { get; set; } public bool FailOnFailedRequests { get; set; } }
public sealed class LwXKeyboardOptions { public TestKeyboardPlatform Platform { get; set; } = TestKeyboardPlatform.Auto; }
public sealed class LwXSecurityOptions { public string RequiredEnvironmentName { get; set; } = "Testing"; public bool BlockInProduction { get; set; } = true; public bool RequireTestHeader { get; set; } public string TestHeaderName { get; set; } = "X-LwX-Test"; public string TestHeaderValue { get; set; } = "true"; }
public sealed class LwXVisualOptions { public bool Enabled { get; set; } public int MaxDiffPixels { get; set; } = 100; public bool UpdateSnapshots { get; set; } public List<string> MaskSelectors { get; } = new(); public string SnapshotDirectory { get; set; } = "snapshots"; }
public sealed class LwXPermissionsOptions { public bool Clipboard { get; set; } public bool Geolocation { get; set; } public bool Camera { get; set; } public bool Microphone { get; set; } }
public sealed class LwXWebView2Options { public bool Enabled { get; set; } public int DebugPort { get; set; } = 9222; public bool ConnectToExistingWebView2 { get; set; } public string DebugUrl => $"http://localhost:{DebugPort}"; }
