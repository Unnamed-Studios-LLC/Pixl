namespace Pixl;

public static class MetaData
{
    internal static bool RecordMetaData = false;

    private static readonly object s_lock = new();
    private static readonly Dictionary<Type, TypeMetaData> s_metaData = new();
    private static readonly Dictionary<string, TypeMetaData> s_metaDataNameMap = new();
    private static readonly HashSet<Type> s_recordedMetaData = new();

    public static IEnumerable<TypeMetaData> All => s_metaData.Values;

    public static TypeMetaData<T> Get<T>() => MetaDataStore<T>.MetaData;
    public static TypeMetaData Get(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        lock (s_lock)
        {
            if (!s_metaData.TryGetValue(type, out var metaData))
            {
                throw new Exception($"MetaData not generated for type: {type}");
            }
            return metaData;
        }
    }
    public static TypeMetaData? Get(string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        lock (s_lock)
        {
            return s_metaDataNameMap.TryGetValue(name, out var metaData) ? metaData : null;
        }
    }

    public static bool Register(Type type, TypeMetaData metaData)
    {
        if (metaData is null)
        {
            throw new ArgumentNullException(nameof(metaData));
        }

        lock (s_lock)
        {
            if (!s_metaDataNameMap.TryAdd(metaData.Name, metaData))
            {
                // TODO notify of name conflict
                return false;
            }
            if (!s_metaData.TryAdd(type, metaData))
            {
                s_metaDataNameMap.Remove(metaData.Name);
                return false;
            }
            if (RecordMetaData) s_recordedMetaData.Add(type);
            return true;
        }
    }

    public static bool TryGet(Type type, out TypeMetaData? metaData)
    {
        return s_metaData.TryGetValue(type, out metaData);
    }

    internal static void ClearRecordedMetaData()
    {
        lock (s_lock)
        {
            foreach (var type in s_recordedMetaData)
            {
                if (!s_metaData.Remove(type, out var metaData)) continue;
                s_metaDataNameMap.Remove(metaData.Name);
            }
            s_recordedMetaData.Clear();
        }
    }

    internal static TypeMetaData<T> InternalGet<T>()
    {
        var type = typeof(T);
        lock (s_lock)
        {
            if (!s_metaData.TryGetValue(type, out var metaData) ||
                metaData is not TypeMetaData<T> typed)
            {
                throw new Exception($"MetaData not generated for type: {type}");
            }
            return typed;
        }
    }
}