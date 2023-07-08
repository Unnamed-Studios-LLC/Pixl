using System.Reflection;

namespace Pixl;

internal sealed class Files
{
    private readonly Dictionary<string, (Assembly Assembly, string ResourceName)> _internalFiles = new();

    public Files(string dataPath, FileBrowser fileBrowser, params Assembly[] internalAssemblies)
    {
        DataPath = dataPath ?? throw new ArgumentNullException(nameof(dataPath));
        FileBrowser = fileBrowser ?? throw new ArgumentNullException(nameof(fileBrowser));

        foreach (var assembly in internalAssemblies)
        {
            var fullName = assembly.FullName ?? throw new Exception($"Unable to resolve internal assembly name of: {assembly}");
            var commaIndex = fullName.IndexOf(',');
            if (commaIndex == -1) throw new Exception("Unable to resolve internal assembly name.");
            var assemblyName = fullName[..commaIndex];
            var internalPrefix = $"{assemblyName}.InternalAssets.";
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                var key = resourceName.Substring(internalPrefix.Length);
                _internalFiles[key] = (assembly, resourceName);
            }
        }
    }

    /// <summary>
    /// The path pointing to a directory where application data may be read and written.
    /// </summary>
    public string DataPath { get; }
    /// <summary>
    /// Platform file browser
    /// </summary>
    public FileBrowser FileBrowser { get; }

    public Stream GetAssetStream(FileHandle handle)
    {
        switch (handle.Location)
        {
            case FileLocation.Internal:
                if (!_internalFiles.TryGetValue(handle.Key, out var resourceInfo)) throw new Exception($"Internal file stream not found for {handle.Key}");
                var internalStream = resourceInfo.Assembly.GetManifestResourceStream(resourceInfo.ResourceName) ?? throw new Exception($"Internal file stream not found for {handle.Key} in {resourceInfo.Assembly}");
                return internalStream;
            default:
                // TODO check asset library for packed or project relative paths
                return File.OpenRead(handle.Key);
        }
    }

    public byte[] GetBytes(FileHandle handle)
    {
        using var fileStream = GetAssetStream(handle);
        var buffer = new byte[fileStream.Length];
        using var memoryStream = new MemoryStream(buffer);
        fileStream.CopyTo(memoryStream);
        return buffer;
    }
}
