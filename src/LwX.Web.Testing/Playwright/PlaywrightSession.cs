using Microsoft.Playwright;
using LwX.Web.Testing.Adapters;
using LwX.Web.Testing.Diagnostics;
using LwX.Web.Testing.Options;
using LwX.Web.Testing.Processes;
using LwX.Web.Testing.Reports;

namespace LwX.Web.Testing.Playwright;

public sealed class PlaywrightSession : IAsyncDisposable
{
    private readonly LwXWebTestingOptions _options;
    private readonly ITestOutputSink? _sink;
    private readonly LwXReportWriter _reportWriter = new();
    private LwXWebAppRunner? _apiRunner;
    private LwXWebAppRunner? _appRunner;
    private bool _tracingStarted;

    public IPlaywright Playwright { get; private set; } = default!;
    public IBrowser Browser { get; private set; } = default!;
    public IBrowserContext Context { get; private set; } = default!;
    public IPage Page { get; private set; } = default!;
    public PlaywrightDiagnostics Diagnostics { get; }
    public IFrontendFrameworkAdapter FrameworkAdapter { get; }
    public LwXTestRunReport? Report { get; private set; }
    public LwXTestCaseReport? CurrentTest { get; private set; }

    public PlaywrightSession(LwXWebTestingOptions options, ITestOutputSink? sink = null)
    {
        _options = options;
        _sink = sink;
        Diagnostics = new PlaywrightDiagnostics(sink);
        FrameworkAdapter = FrontendFrameworkAdapterFactory.Create(options.Framework);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_options.Artifacts.OutputDirectory);
        if (_options.Report.Enabled) Directory.CreateDirectory(_options.Report.OutputDirectory);
        Directory.CreateDirectory(Path.Combine(_options.Artifacts.OutputDirectory, "screenshots"));
        Directory.CreateDirectory(Path.Combine(_options.Artifacts.OutputDirectory, "traces"));
        Directory.CreateDirectory(Path.Combine(_options.Artifacts.OutputDirectory, "videos"));
        Directory.CreateDirectory(Path.Combine(_options.Artifacts.OutputDirectory, "har"));
        Directory.CreateDirectory(_options.Logging.LogDirectory);

        if (_options.Report.Enabled)
        {
            Report = new LwXTestRunReport
            {
                Framework = _options.Framework.ToString(),
                Browser = _options.Playwright.BrowserKind.ToString(),
                Profile = _options.Profile.ToString(),
                BaseUrl = _options.BaseUrl,
                Metadata = new Dictionary<string, string>(_options.Report.Metadata, StringComparer.OrdinalIgnoreCase)
            };
        }

        if (_options.ApiRunner is not null) _apiRunner = await LwXWebAppRunner.StartAsync(_options.ApiRunner, cancellationToken);
        if (_options.AppRunner is not null) _appRunner = await LwXWebAppRunner.StartAsync(_options.AppRunner, cancellationToken);

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = _options.Playwright.BrowserFactory is not null ? await _options.Playwright.BrowserFactory(Playwright, _options) : await LaunchBrowserAsync();
        Context = _options.Playwright.ContextFactory is not null ? await _options.Playwright.ContextFactory(Browser, _options) : await Browser.NewContextAsync(CreateContextOptions());

        if (_options.Artifacts.TraceOnFailure)
        {
            await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
            _tracingStarted = true;
        }

        await ApplyInitScriptsAsync();
        await ApplyPermissionsAsync();
        if (_options.Headers.DefaultHeaders.Count > 0) await Context.SetExtraHTTPHeadersAsync(_options.Headers.DefaultHeaders);

        Page = _options.Playwright.PageFactory is not null ? await _options.Playwright.PageFactory(Context, _options) : await Context.NewPageAsync();
        Page.SetDefaultTimeout(_options.Playwright.DefaultTimeout);
        Page.SetDefaultNavigationTimeout(_options.Playwright.NavigationTimeout);
        Diagnostics.Attach(Page);
        if (_options.BeforeEachPageAsync is not null) await _options.BeforeEachPageAsync(Page, cancellationToken);
    }

    public LwXTestCaseReport BeginTest(string name, string? className = null)
    {
        CurrentTest = new LwXTestCaseReport { Name = name, ClassName = className ?? string.Empty, Status = "Running" };
        Report?.Tests.Add(CurrentTest);
        return CurrentTest;
    }

    public async Task EndTestAsync(string status = "Passed", Exception? exception = null, CancellationToken cancellationToken = default)
    {
        if (CurrentTest is not null)
        {
            CurrentTest.FinishedAtUtc = DateTimeOffset.UtcNow;
            CurrentTest.DurationMs = (long)(CurrentTest.FinishedAtUtc.Value - CurrentTest.StartedAtUtc).TotalMilliseconds;
            CurrentTest.Status = exception is null ? status : "Failed";
            if (exception is not null)
            {
                CurrentTest.ErrorType = exception.GetType().FullName;
                CurrentTest.ErrorMessage = exception.Message;
                CurrentTest.StackTrace = exception.ToString();
            }
            AddDiagnosticsToTest(CurrentTest);
        }
        await FlushReportAsync(cancellationToken);
    }

    public async Task<IResponse?> GoToAsync(string path = "/", PageGotoOptions? options = null)
    {
        var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : Combine(_options.BaseUrl, path);
        var response = await Page.GotoAsync(url, options);
        await FrameworkAdapter.WaitForReadyAsync(Page);
        return response;
    }

    public async Task StepAsync(string name, Func<Task> action, CancellationToken cancellationToken = default)
    {
        var step = new LwXTestStepReport { Name = name, Status = "Running" };
        CurrentTest?.Steps.Add(step);
        var consoleStart = Diagnostics.ConsoleCount;
        var networkStart = Diagnostics.FailedRequestCount;
        try
        {
            await action();
            step.Status = "Passed";
        }
        catch (Exception ex)
        {
            step.Status = "Failed";
            step.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            step.FinishedAtUtc = DateTimeOffset.UtcNow;
            step.DurationMs = (long)(step.FinishedAtUtc.Value - step.StartedAtUtc).TotalMilliseconds;
            if (_options.Steps.ScreenshotEachStep && Page is not null)
            {
                var path = Path.Combine(_options.Artifacts.OutputDirectory, "screenshots", $"{Sanitize(CurrentTest?.Name ?? "test")}-{Sanitize(name)}.png");
                await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
                step.Artifacts.Add(LwXReportWriter.CreateArtifact(_options.Report.OutputDirectory, path, "screenshot", "image/png"));
            }
            if (_options.Steps.CaptureConsoleEachStep && CurrentTest is not null)
                CurrentTest.Console.AddRange(Diagnostics.ConsoleSince(consoleStart).Select(x => new LwXConsoleReportEntry { TimestampUtc = x.Timestamp, Type = x.Type, Text = x.Text, Location = x.Location }));
            if (_options.Steps.CaptureNetworkEachStep && CurrentTest is not null)
                CurrentTest.Network.AddRange(Diagnostics.FailedRequestsSince(networkStart).Select(x => new LwXNetworkReportEntry { TimestampUtc = x.Timestamp, Method = x.Method, Url = x.Url, Failure = x.Failure, StatusCode = x.StatusCode }));
            await FlushReportAsync(cancellationToken);
        }
    }

    public Task<T> StepAsync<T>(string name, Func<Task<T>> action, CancellationToken cancellationToken = default)
        => StepWithResultAsync(name, action, cancellationToken);

    private async Task<T> StepWithResultAsync<T>(string name, Func<Task<T>> action, CancellationToken cancellationToken)
    {
        T? result = default;
        await StepAsync(name, async () => { result = await action(); }, cancellationToken);
        return result!;
    }

    public async Task<IBrowser> ConnectToWebView2Async(string? cdpUrl = null)
    {
        cdpUrl ??= _options.WebView2.DebugUrl;
        Browser = await Playwright.Chromium.ConnectOverCDPAsync(cdpUrl);
        Context = Browser.Contexts.FirstOrDefault() ?? await Browser.NewContextAsync();
        Page = Context.Pages.FirstOrDefault() ?? await Context.NewPageAsync();
        Diagnostics.Attach(Page);
        return Browser;
    }

    public async Task OnFailureAsync(Exception exception, string? name = null)
    {
        name ??= CurrentTest?.Name ?? DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var artifactsRoot = _options.Artifacts.OutputDirectory;
        Directory.CreateDirectory(artifactsRoot);

        if (_options.Artifacts.ScreenshotOnFailure && Page is not null)
        {
            var path = Path.Combine(artifactsRoot, "screenshots", $"{Sanitize(name)}.failure.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
            CurrentTest?.Artifacts.Add(LwXReportWriter.CreateArtifact(_options.Report.OutputDirectory, path, "screenshot", "image/png"));
        }

        if (_options.Artifacts.TraceOnFailure && _tracingStarted)
        {
            var path = Path.Combine(artifactsRoot, "traces", $"{Sanitize(name)}.trace.zip");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await Context.Tracing.StopAsync(new() { Path = path });
            _tracingStarted = false;
            CurrentTest?.Artifacts.Add(LwXReportWriter.CreateArtifact(_options.Report.OutputDirectory, path, "trace", "application/zip"));
        }

        if (_options.Logging.WriteToFile)
        {
            await Diagnostics.WriteToFileAsync(_options.Logging.LogDirectory, name);
            var log = Path.Combine(_options.Logging.LogDirectory, $"{Sanitize(name)}.log");
            if (File.Exists(log)) CurrentTest?.Artifacts.Add(LwXReportWriter.CreateArtifact(_options.Report.OutputDirectory, log, "log", "text/plain"));
        }

        if (_options.Report.Enabled && CurrentTest is not null)
        {
            CurrentTest.Status = "Failed";
            CurrentTest.ErrorType = exception.GetType().FullName;
            CurrentTest.ErrorMessage = exception.Message;
            CurrentTest.StackTrace = exception.ToString();
            AddDiagnosticsToTest(CurrentTest);
            await FlushReportAsync();
        }

        if (_options.OnTestFailureAsync is not null) await _options.OnTestFailureAsync(Page, exception, CancellationToken.None);
    }

    private async Task FlushReportAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Report.Enabled || Report is null) return;
        Report.FinishedAtUtc = DateTimeOffset.UtcNow;
        Report.Console = Diagnostics.ConsoleEntries.Select(x => new LwXConsoleReportEntry { TimestampUtc = x.Timestamp, Type = x.Type, Text = x.Text, Location = x.Location }).ToList();
        Report.Network = Diagnostics.FailedRequests.Select(x => new LwXNetworkReportEntry { TimestampUtc = x.Timestamp, Method = x.Method, Url = x.Url, Failure = x.Failure, StatusCode = x.StatusCode }).ToList();
        var errors = await FrameworkAdapter.GetFrameworkErrorsAsync(Page);
        Report.FrameworkErrors = errors.Select(x => new LwXFrameworkReportEntry { Framework = FrameworkAdapter.Name, Message = x }).ToList();
        await _reportWriter.WriteAsync(Report, _options.Report.FullPath, cancellationToken);
    }

    private void AddDiagnosticsToTest(LwXTestCaseReport test)
    {
        test.Console = Diagnostics.ConsoleEntries.Select(x => new LwXConsoleReportEntry { TimestampUtc = x.Timestamp, Type = x.Type, Text = x.Text, Location = x.Location }).ToList();
        test.Network = Diagnostics.FailedRequests.Select(x => new LwXNetworkReportEntry { TimestampUtc = x.Timestamp, Method = x.Method, Url = x.Url, Failure = x.Failure, StatusCode = x.StatusCode }).ToList();
    }

    private async Task<IBrowser> LaunchBrowserAsync()
    {
        var launch = new BrowserTypeLaunchOptions { Headless = _options.Playwright.Headless, SlowMo = _options.Playwright.SlowMo, Channel = _options.Playwright.Channel };
        _options.Playwright.ConfigureLaunchOptions?.Invoke(launch);
        return _options.Playwright.BrowserKind switch
        {
            LwXBrowserKind.Firefox => await Playwright.Firefox.LaunchAsync(launch),
            LwXBrowserKind.WebKit => await Playwright.Webkit.LaunchAsync(launch),
            _ => await Playwright.Chromium.LaunchAsync(launch)
        };
    }

    private BrowserNewContextOptions CreateContextOptions()
    {
        Directory.CreateDirectory(Path.Combine(_options.Artifacts.OutputDirectory, "videos"));
        Directory.CreateDirectory(Path.Combine(_options.Artifacts.OutputDirectory, "har"));
        var context = new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = _options.Playwright.IgnoreHTTPSErrors,
            ViewportSize = new() { Width = _options.Playwright.ViewportWidth, Height = _options.Playwright.ViewportHeight },
            Locale = _options.Playwright.Locale,
            TimezoneId = _options.Playwright.TimezoneId,
            DeviceScaleFactor = _options.Playwright.DeviceScaleFactor,
            IsMobile = _options.Playwright.IsMobile,
            HasTouch = _options.Playwright.HasTouch,
            RecordVideoDir = _options.Artifacts.VideoOnFailure ? Path.Combine(_options.Artifacts.OutputDirectory, "videos") : null,
            RecordHarPath = _options.Artifacts.HarOnFailure ? Path.Combine(_options.Artifacts.OutputDirectory, "har", "session.har") : null
        };
        if (_options.Playwright.ColorScheme != LwXColorScheme.NoPreference)
            context.ColorScheme = _options.Playwright.ColorScheme == LwXColorScheme.Dark ? ColorScheme.Dark : ColorScheme.Light;
        _options.Playwright.ConfigureContextOptions?.Invoke(context);
        return context;
    }

    private async Task ApplyInitScriptsAsync()
    {
        if (_options.JavaScript.EnableTestModeFlag) await Context.AddInitScriptAsync("window.__LW_TEST_MODE__ = true;");
        if (_options.JavaScript.DisableAnimations) await Context.AddInitScriptAsync("""
            (() => { const style = document.createElement('style'); style.innerHTML='*,*::before,*::after{transition:none!important;animation:none!important;scroll-behavior:auto!important}'; document.addEventListener('DOMContentLoaded',()=>document.head.appendChild(style)); })();
        """);
        foreach (var script in _options.JavaScript.InitScripts) await Context.AddInitScriptAsync(script);
    }

    private async Task ApplyPermissionsAsync()
    {
        var permissions = new List<string>();
        if (_options.Permissions.Clipboard) permissions.Add("clipboard-read");
        if (_options.Permissions.Geolocation) permissions.Add("geolocation");
        if (_options.Permissions.Camera) permissions.Add("camera");
        if (_options.Permissions.Microphone) permissions.Add("microphone");
        if (permissions.Count > 0) await Context.GrantPermissionsAsync(permissions);
    }

    private static string Combine(string baseUrl, string path) => $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    private static string Sanitize(string value) { foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_'); return value; }

    public async ValueTask DisposeAsync()
    {
        try { if (_options.AfterEachPageAsync is not null && Page is not null) await _options.AfterEachPageAsync(Page, CancellationToken.None); } catch { }
        try { if (_tracingStarted && Context is not null) await Context.Tracing.StopAsync(); } catch { }
        try { await FlushReportAsync(); } catch { }
        try { if (Context is not null) await Context.CloseAsync(); } catch { }
        try { if (Browser is not null && !_options.Playwright.KeepBrowserOpenOnFailure) await Browser.CloseAsync(); } catch { }
        Playwright?.Dispose();
        if (_appRunner is not null) await _appRunner.DisposeAsync();
        if (_apiRunner is not null) await _apiRunner.DisposeAsync();
    }
}
