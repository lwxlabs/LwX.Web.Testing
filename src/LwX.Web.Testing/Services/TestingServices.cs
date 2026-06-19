namespace LwX.Web.Testing.Services;

public interface ILwXTestNavigation { string? LastUri { get; } void NavigateTo(string uri); }
public sealed class FakeTestNavigation : ILwXTestNavigation { public string? LastUri { get; private set; } public void NavigateTo(string uri) => LastUri = uri; }

public interface ILwXTestDialog
{
    string? LastMessage { get; }
    bool AutoConfirm { get; set; }
    Task<bool> ConfirmAsync(string message);
    Task AlertAsync(string message);
}
public sealed class FakeTestDialog : ILwXTestDialog
{
    public string? LastMessage { get; private set; }
    public bool AutoConfirm { get; set; } = true;
    public Task<bool> ConfirmAsync(string message) { LastMessage = message; return Task.FromResult(AutoConfirm); }
    public Task AlertAsync(string message) { LastMessage = message; return Task.CompletedTask; }
}

public interface ILwXTestStorage
{
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAsync();
}
public sealed class InMemoryTestStorage : ILwXTestStorage
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    public Task SetAsync(string key, string value) { _values[key] = value; return Task.CompletedTask; }
    public Task<string?> GetAsync(string key) => Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);
    public Task RemoveAsync(string key) { _values.Remove(key); return Task.CompletedTask; }
    public Task ClearAsync() { _values.Clear(); return Task.CompletedTask; }
}

public interface ILwXTestFilePicker { Task<IReadOnlyList<string>> PickFilesAsync(); }
public sealed class FakeTestFilePicker : ILwXTestFilePicker
{
    public List<string> Files { get; } = [];
    public Task<IReadOnlyList<string>> PickFilesAsync() => Task.FromResult<IReadOnlyList<string>>(Files);
}

public interface ILwXTestFileSystem
{
    Task WriteTextAsync(string path, string content);
    Task<string?> ReadTextAsync(string path);
    Task DeleteAsync(string path);
    Task<IReadOnlyList<string>> ListAsync(string prefix = "");
}
public sealed class InMemoryFileSystem : ILwXTestFileSystem
{
    private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
    public Task WriteTextAsync(string path, string content) { _files[path] = content; return Task.CompletedTask; }
    public Task<string?> ReadTextAsync(string path) => Task.FromResult(_files.TryGetValue(path, out var content) ? content : null);
    public Task DeleteAsync(string path) { _files.Remove(path); return Task.CompletedTask; }
    public Task<IReadOnlyList<string>> ListAsync(string prefix = "") => Task.FromResult<IReadOnlyList<string>>(_files.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray());
}

public sealed class TempFolderFileSystem : ILwXTestFileSystem, IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "lwx-web-testing-" + Guid.NewGuid().ToString("N"));
    public TempFolderFileSystem() => Directory.CreateDirectory(Root);
    public async Task WriteTextAsync(string path, string content) { var full = Full(path); Directory.CreateDirectory(Path.GetDirectoryName(full)!); await File.WriteAllTextAsync(full, content); }
    public async Task<string?> ReadTextAsync(string path) { var full = Full(path); return File.Exists(full) ? await File.ReadAllTextAsync(full) : null; }
    public Task DeleteAsync(string path) { var full = Full(path); if (File.Exists(full)) File.Delete(full); return Task.CompletedTask; }
    public Task<IReadOnlyList<string>> ListAsync(string prefix = "") => Task.FromResult<IReadOnlyList<string>>(Directory.Exists(Root) ? Directory.GetFiles(Root, "*", SearchOption.AllDirectories).Select(f => Path.GetRelativePath(Root, f)).ToArray() : []);
    private string Full(string path) => Path.Combine(Root, path.TrimStart('/', '\\'));
    public void Dispose() { try { if (Directory.Exists(Root)) Directory.Delete(Root, true); } catch { } }
}

public interface ILwXClipboard { Task SetTextAsync(string value); Task<string?> GetTextAsync(); }
public sealed class FakeClipboard : ILwXClipboard { private string? _text; public Task SetTextAsync(string value) { _text = value; return Task.CompletedTask; } public Task<string?> GetTextAsync() => Task.FromResult(_text); }
public interface ILwXConnectivity { bool IsOnline { get; set; } }
public sealed class FakeConnectivity : ILwXConnectivity { public bool IsOnline { get; set; } = true; }
public interface ILwXDeviceInfo { string Platform { get; } string Model { get; } }
public sealed class FakeDeviceInfo : ILwXDeviceInfo { public string Platform { get; set; } = "Test"; public string Model { get; set; } = "TestDevice"; }
public interface ILwXGeolocation { double Latitude { get; set; } double Longitude { get; set; } }
public sealed class FakeGeolocation : ILwXGeolocation { public double Latitude { get; set; } = 50.8503; public double Longitude { get; set; } = 4.3517; }
public interface ILwXPermissions { Task<bool> RequestAsync(string name); }
public sealed class FakePermissions : ILwXPermissions { public HashSet<string> Granted { get; } = new(StringComparer.OrdinalIgnoreCase); public Task<bool> RequestAsync(string name) => Task.FromResult(Granted.Contains(name)); }
public interface ILwXShare { Task ShareTextAsync(string text); }
public sealed class FakeShare : ILwXShare { public string? LastText { get; private set; } public Task ShareTextAsync(string text) { LastText = text; return Task.CompletedTask; } }
public interface ILwXLauncher { Task OpenAsync(string uri); }
public sealed class FakeLauncher : ILwXLauncher { public string? LastUri { get; private set; } public Task OpenAsync(string uri) { LastUri = uri; return Task.CompletedTask; } }
