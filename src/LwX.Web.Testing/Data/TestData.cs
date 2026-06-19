namespace LwX.Web.Testing.Data;

public sealed class TestClock
{
    public DateTimeOffset Now { get; private set; } = DateTimeOffset.UtcNow;
    public void Set(DateTimeOffset value) => Now = value;
    public void Reset() => Now = DateTimeOffset.UtcNow;
}

public sealed class TestGuidGenerator
{
    private readonly Queue<Guid> _guids = new();
    public void Enqueue(Guid guid) => _guids.Enqueue(guid);
    public Guid NewGuid() => _guids.Count > 0 ? _guids.Dequeue() : Guid.NewGuid();
}

public sealed class TestDataStore
{
    private readonly Dictionary<string, object?> _items = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, object?> Items => _items;
    public void Set(string key, object? value) => _items[key] = value;
    public T? Get<T>(string key) => _items.TryGetValue(key, out var value) && value is T typed ? typed : default;
    public void Reset() => _items.Clear();
}
