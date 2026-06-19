using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;

namespace LwX.Web.Testing.Processes;

public sealed class LwXWebAppRunnerOptions
{
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();
    public string ReadyUrl { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(60);
    public Dictionary<string, string?> EnvironmentVariables { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool KillProcessTreeOnDispose { get; set; } = true;
    public bool RedirectOutput { get; set; } = true;
    public List<string> OutputLines { get; } = [];

    public static LwXWebAppRunnerOptions FromSolutionRelative(string solutionRootRelativeDirectory, string command, string arguments, string readyUrl)
    {
        var root = FindSolutionRoot(AppContext.BaseDirectory) ?? Directory.GetCurrentDirectory();
        return new()
        {
            WorkingDirectory = Path.GetFullPath(solutionRootRelativeDirectory, root),
            Command = command,
            Arguments = arguments,
            ReadyUrl = readyUrl
        };
    }

    private static string? FindSolutionRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (dir.EnumerateFiles("*.sln*").Any() || dir.GetDirectories("src").Any() && dir.GetDirectories("samples").Any()) return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}

public sealed class LwXWebAppRunner : IAsyncDisposable
{
    private readonly Process _process;
    private readonly LwXWebAppRunnerOptions _options;

    private LwXWebAppRunner(Process process, LwXWebAppRunnerOptions options)
    {
        _process = process;
        _options = options;
    }

    public int? ProcessId => _process.HasExited ? null : _process.Id;
    public IReadOnlyList<string> OutputLines => _options.OutputLines;

    public static async Task<LwXWebAppRunner> StartAsync(LwXWebAppRunnerOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Command)) throw new ArgumentException("Command is required.");
        Directory.CreateDirectory(options.WorkingDirectory);
        var info = new ProcessStartInfo(options.Command, options.Arguments)
        {
            WorkingDirectory = options.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = options.RedirectOutput,
            RedirectStandardError = options.RedirectOutput,
            CreateNoWindow = true
        };
        foreach (var pair in options.EnvironmentVariables) info.Environment[pair.Key] = pair.Value;
        var process = new Process { StartInfo = info, EnableRaisingEvents = true };
        if (options.RedirectOutput)
        {
            process.OutputDataReceived += (_, e) => { if (e.Data is not null) lock (options.OutputLines) options.OutputLines.Add(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) lock (options.OutputLines) options.OutputLines.Add(e.Data); };
        }
        if (!process.Start()) throw new InvalidOperationException($"Could not start {options.Command}.");
        if (options.RedirectOutput)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        var runner = new LwXWebAppRunner(process, options);
        if (!string.IsNullOrWhiteSpace(options.ReadyUrl))
            await WaitForReadyUrlAsync(options.ReadyUrl, options.Timeout, cancellationToken, process, options);
        return runner;
    }

    private static async Task WaitForReadyUrlAsync(string url, TimeSpan timeout, CancellationToken cancellationToken, Process process, LwXWebAppRunnerOptions options)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        using var http = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
        while (!cts.IsCancellationRequested)
        {
            if (process.HasExited) throw new InvalidOperationException($"The app process exited before it became ready. Output: {string.Join(Environment.NewLine, options.OutputLines.TakeLast(50))}");
            try
            {
                using var response = await http.GetAsync(url, cts.Token);
                if ((int)response.StatusCode < 500) return;
            }
            catch when (!cts.IsCancellationRequested) { }
            await Task.Delay(500, cts.Token);
        }
        throw new TimeoutException($"The app did not become ready at {url} within {timeout}. Output: {string.Join(Environment.NewLine, options.OutputLines.TakeLast(50))}");
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                if (_options.KillProcessTreeOnDispose) _process.Kill(entireProcessTree: true);
                else _process.Kill();
            }
            _process.Dispose();
        }
        catch { }
        return ValueTask.CompletedTask;
    }
}
