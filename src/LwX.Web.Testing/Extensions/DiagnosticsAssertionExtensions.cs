using LwX.Web.Testing.Diagnostics;

namespace LwX.Web.Testing.Extensions;

public static class DiagnosticsAssertionExtensions
{
    public static Task ShouldHaveNoConsoleErrorsAsync(this PlaywrightDiagnostics diagnostics)
    {
        var errors = diagnostics.ConsoleEntries
            .Where(x => string.Equals(x.Type, "error", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Text)
            .ToArray();
        if (errors.Length > 0) throw new InvalidOperationException("Console errors found: " + string.Join(Environment.NewLine, errors));
        return Task.CompletedTask;
    }

    public static Task ShouldHaveNoFailedRequestsAsync(this PlaywrightDiagnostics diagnostics)
    {
        if (diagnostics.FailedRequests.Count > 0)
        {
            var lines = diagnostics.FailedRequests.Select(x => $"{x.StatusCode?.ToString() ?? "failed"} {x.Method} {x.Url} {x.Failure}");
            throw new InvalidOperationException("Failed requests found: " + string.Join(Environment.NewLine, lines));
        }
        return Task.CompletedTask;
    }

    public static Task ShouldHaveNoPageErrorsAsync(this PlaywrightDiagnostics diagnostics)
    {
        if (diagnostics.PageErrors.Count > 0)
            throw new InvalidOperationException("Page errors found: " + string.Join(Environment.NewLine, diagnostics.PageErrors.Select(x => x.Message)));
        return Task.CompletedTask;
    }
}
