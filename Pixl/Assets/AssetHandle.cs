using System;
using System.Reflection;

namespace Pixl;

internal readonly struct AssetHandle
{
    public AssetHandle(string key, AssetStoreLocation storeLocation)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        StoreLocation = storeLocation;
    }

    public string Key { get; }
    public AssetStoreLocation StoreLocation { get; }

    public static AssetHandle CreateAbsolutePath(string filePath) => new AssetHandle(filePath, AssetStoreLocation.AbsolutePath);
    public static AssetHandle CreateInternal(string key) => new AssetHandle(key, AssetStoreLocation.Internal);

    public Stream GetStream()
    {
        switch (StoreLocation)
        {
            case AssetStoreLocation.Internal:
                var assembly = Assembly.GetExecutingAssembly();
                var resources = assembly.GetManifestResourceNames();
                var internalStream = assembly.GetManifestResourceStream($"Pixl.InternalAssets.{Key}");
                if (internalStream is null) throw new Exception($"Internal file stream not found for {Key}");
                return internalStream;
            default:
                return File.OpenRead(Key);
        }
    }
}

