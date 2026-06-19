using LwX.Web.Testing.Diagnostics;
using LwX.Web.Testing.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace LwX.Web.Testing.Xunit;

public abstract class XunitPlaywrightTestBase : PlaywrightTestBase, IAsyncLifetime
{
    private readonly ITestOutputHelper? _output;

    protected XunitPlaywrightTestBase() { }
    protected XunitPlaywrightTestBase(ITestOutputHelper output) => _output = output;

    protected override ITestOutputSink? OutputSink => _output is null ? null : new XunitOutputSink(_output);

    async Task IAsyncLifetime.InitializeAsync() => await InitializeAsync();
    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();
}

public sealed class XunitOutputSink : ITestOutputSink
{
    private readonly ITestOutputHelper _output;
    public XunitOutputSink(ITestOutputHelper output) => _output = output;
    public void WriteLine(string message)
    {
        try { _output.WriteLine(message); } catch { }
    }
}
