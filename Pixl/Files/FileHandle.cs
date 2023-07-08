using System.Reflection;

namespace Pixl;

internal readonly struct FileHandle
{
    public FileHandle(string key, FileLocation storeLocation)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Location = storeLocation;
    }

    public string Key { get; }
    public FileLocation Location { get; }
    public Assembly? Assembly { get; }

    public static FileHandle CreateExternal(string filePath) => new(filePath, FileLocation.External);
    public static FileHandle CreateInternal(string key) => new(key, FileLocation.Internal);
}

