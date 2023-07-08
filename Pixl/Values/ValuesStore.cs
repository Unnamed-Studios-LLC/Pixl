namespace Pixl;

internal abstract class ValuesStore
{
    public abstract void Read(Dictionary<string, StoredValue> all);
    public abstract void Write(Dictionary<string, StoredValue> all, Dictionary<string, StoredValue> edited);
}
