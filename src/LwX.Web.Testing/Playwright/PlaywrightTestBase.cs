using Microsoft.Playwright;
using LwX.Web.Testing.Diagnostics;
using LwX.Web.Testing.Options;

namespace LwX.Web.Testing.Playwright;

public abstract class PlaywrightTestBase
{
    private PlaywrightSession? _session;

    protected LwXWebTestingOptions Options { get; } = new();
    protected PlaywrightSession Session => _session ?? throw new InvalidOperationException("Session is not initialized. Call InitializeAsync or use LwX.Web.Testing.Xunit.");
    protected IPage Page => Session.Page;
    protected IBrowserContext Context => Session.Context;
    protected PlaywrightDiagnostics Diagnostics => Session.Diagnostics;

    protected virtual ITestOutputSink? OutputSink => null;
    protected virtual void Configure(LwXWebTestingOptions options) { }

    public virtual async Task InitializeAsync()
    {
        Configure(Options);
        _session = new PlaywrightSession(Options, OutputSink);
        await _session.InitializeAsync();
    }

    public virtual async Task DisposeAsync()
    {
        if (_session is not null) await _session.DisposeAsync();
    }

    protected Task<IResponse?> GoToAsync(string path = "/") => Session.GoToAsync(path);
    protected Task StepAsync(string name, Func<Task> action) => Session.StepAsync(name, action);
    protected Task<T> StepAsync<T>(string name, Func<Task<T>> action) => Session.StepAsync(name, action);

    protected async Task RunSafelyAsync(Func<Task> test, string? artifactName = null)
    {
        Session.BeginTest(artifactName ?? GetType().Name, GetType().FullName);
        try
        {
            await test();
            await Session.EndTestAsync("Passed");
        }
        catch (Exception ex)
        {
            await Session.OnFailureAsync(ex, artifactName);
            await Session.EndTestAsync("Failed", ex);
            throw;
        }
    }
}
