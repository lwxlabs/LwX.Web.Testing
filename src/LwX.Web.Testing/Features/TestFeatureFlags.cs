namespace LwX.Web.Testing.Features;

public sealed class TestFeatureFlags
{
    private readonly Dictionary<string, object?> _values = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, object?> Values => _values;
    public void Set(string key, object? value) => _values[key] = value;
    public bool IsEnabled(string key) => _values.TryGetValue(key, out var value) && value is bool enabled && enabled;
    public T? Get<T>(string key) => _values.TryGetValue(key, out var value) && value is T typed ? typed : default;
    public void Clear() => _values.Clear();
}
