namespace Pixl;

internal sealed class MemoryValuesStore : ValuesStore
{
    private readonly Dictionary<string, StoredValue> _values = new();

    public override void Read(Dictionary<string, StoredValue> all)
    {
        foreach (var (key, value) in _values) all[key] = value;
    }

    public override void Write(Dictionary<string, StoredValue> all, Dictionary<string, StoredValue> edited)
    {
        foreach (var (key, value) in edited) _values[key] = value;
    }
}
