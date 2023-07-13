namespace Pixl;

internal static class MetaDataStore<T>
{
    private static TypeMetaData<T>? s_metaData;

    public static TypeMetaData<T> MetaData => GetMetaData();

    private static TypeMetaData<T> GetMetaData()
    {
        s_metaData ??= Pixl.MetaData.InternalGet<T>();
        return s_metaData;
    }
}
