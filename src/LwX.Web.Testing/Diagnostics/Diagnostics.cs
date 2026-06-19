using Microsoft.Playwright;

namespace LwX.Web.Testing.Diagnostics;

public interface ITestOutputSink { void WriteLine(string message); }
public sealed class ConsoleOutputSink : ITestOutputSink { public void WriteLine(string message) => Console.WriteLine(message); }
public sealed record BrowserConsoleEntry(DateTimeOffset Timestamp, string Type, string Text, string? Location = null);
public sealed record FailedRequestEntry(DateTimeOffset Timestamp, string Url, string Method, string? Failure, int? StatusCode = null);
public sealed record PageErrorEntry(DateTimeOffset Timestamp, string Message);

public sealed class PlaywrightDiagnostics
{
    private readonly List<BrowserConsoleEntry> _console = [];
    private readonly List<FailedRequestEntry> _failedRequests = [];
    private readonly List<PageErrorEntry> _pageErrors = [];
    private readonly ITestOutputSink? _sink;

    public PlaywrightDiagnostics(ITestOutputSink? sink = null) => _sink = sink;
    public IReadOnlyList<BrowserConsoleEntry> ConsoleEntries => _console;
    public IReadOnlyList<FailedRequestEntry> FailedRequests => _failedRequests;
    public IReadOnlyList<PageErrorEntry> PageErrors => _pageErrors;
    public int ConsoleCount => _console.Count;
    public int FailedRequestCount => _failedRequests.Count;
    public int PageErrorCount => _pageErrors.Count;
    public IReadOnlyList<BrowserConsoleEntry> ConsoleSince(int index) => _console.Skip(index).ToArray();
    public IReadOnlyList<FailedRequestEntry> FailedRequestsSince(int index) => _failedRequests.Skip(index).ToArray();
    public IReadOnlyList<PageErrorEntry> PageErrorsSince(int index) => _pageErrors.Skip(index).ToArray();

    public void Attach(IPage page)
    {
        page.Console += (_, message) =>
        {
            var entry = new BrowserConsoleEntry(DateTimeOffset.UtcNow, message.Type, message.Text);
            _console.Add(entry); _sink?.WriteLine($"[Browser Console][{entry.Type}] {entry.Text}");
        };
        page.PageError += (_, error) =>
        {
            var entry = new PageErrorEntry(DateTimeOffset.UtcNow, error);
            _pageErrors.Add(entry); _sink?.WriteLine($"[Page Error] {entry.Message}");
        };
        page.RequestFailed += (_, request) =>
        {
            var entry = new FailedRequestEntry(DateTimeOffset.UtcNow, request.Url, request.Method, request.Failure);
            _failedRequests.Add(entry); _sink?.WriteLine($"[Request Failed] {entry.Method} {entry.Url} {entry.Failure}");
        };
        page.Response += (_, response) =>
        {
            if (response.Status >= 400)
            {
                var entry = new FailedRequestEntry(DateTimeOffset.UtcNow, response.Url, response.Request.Method, null, response.Status);
                _failedRequests.Add(entry); _sink?.WriteLine($"[Response {response.Status}] {response.Request.Method} {response.Url}");
            }
        };
    }

    public async Task WriteToFileAsync(string directory, string name = "diagnostics", CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{Sanitize(name)}.log");
        await using var writer = File.CreateText(path);
        foreach (var entry in _console) await writer.WriteLineAsync($"[Console][{entry.Type}] {entry.Text}");
        foreach (var entry in _pageErrors) await writer.WriteLineAsync($"[PageError] {entry.Message}");
        foreach (var entry in _failedRequests) await writer.WriteLineAsync($"[Request] {entry.StatusCode?.ToString() ?? "failed"} {entry.Method} {entry.Url} {entry.Failure}");
    }

    private static string Sanitize(string value) { foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_'); return value; }
}

public static class TestingEnvironment
{
    public static bool IsCi => string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"))
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD"));
}
