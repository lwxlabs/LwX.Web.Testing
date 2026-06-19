using System.Text.Json;

namespace LwX.Web.Testing.Reports;

public sealed class LwXReportWriter
{
    private readonly JsonSerializerOptions _json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task WriteAsync(LwXTestRunReport report, string path, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
        var json = JsonSerializer.Serialize(report, _json);
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }

    public static LwXArtifactReference CreateArtifact(string rootDirectory, string fullPath, string kind, string? contentType = null)
    {
        var info = new FileInfo(fullPath);
        return new LwXArtifactReference
        {
            Kind = kind,
            Name = Path.GetFileName(fullPath),
            RelativePath = Path.GetRelativePath(rootDirectory, fullPath).Replace('\\', '/'),
            ContentType = contentType,
            SizeBytes = info.Exists ? info.Length : null,
            CreatedAtUtc = info.Exists ? info.CreationTimeUtc : DateTimeOffset.UtcNow
        };
    }
}
