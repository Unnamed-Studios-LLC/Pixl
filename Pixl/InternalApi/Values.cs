namespace Pixl;

internal sealed class Values
{
    private readonly ValuesStore _store;
    private readonly Dictionary<string, StoredValue> _all = new();
    private readonly Dictionary<string, StoredValue> _edited = new();

    internal Values(ValuesStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public float GetFloat(string key, float @default = default)
    {
        if (!TryGetValue(key, out StoredValue value) ||
            !value.TryGetFloat(out var @float))
        {
            return @default;
        }

        return @float;
    }

    public int GetInt(string key, int @default = default)
    {
        if (!TryGetValue(key, out StoredValue value) ||
            !value.TryGetInt(out var @int))
        {
            return @default;
        }

        return @int;
    }

    public string? GetString(string key, string? @default = default)
    {
        if (!TryGetValue(key, out StoredValue value) ||
            !value.TryGetString(out var @string))
        {
            return @default;
        }

        return @string;
    }

    public bool HasKey(string key) => TryGetValue(key, out _);

    public void Save()
    {
        if (_edited.Count == 0) return;
        foreach (var (key, value) in _edited) _all[key] = value;
        _store.Write(_all, _edited);
        _edited.Clear();
    }

    public void SetFloat(string key, float value) => _edited[key] = new StoredValue(ValueType.Float, value);
    public void SetInt(string key, int value) => _edited[key] = new StoredValue(ValueType.Int, value);
    public void SetString(string key, string value) => _edited[key] = new StoredValue(ValueType.String, value);

    internal void Start()
    {
        _all.Clear();
        _edited.Clear();
        _store.Read(_all);
    }

    internal void Stop()
    {
        Save();
    }

    private bool TryGetValue(string key, out StoredValue value)
    {
        if (!_edited.TryGetValue(key, out value) &&
            !_all.TryGetValue(key, out value))
        {
            return false;
        }
        return true;
    }
}
