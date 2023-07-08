using System.Text;

namespace Pixl;

public abstract class Shader
{
    private readonly Files _files;
    private readonly FileHandle _fileHandle;

    internal Shader(Files files, FileHandle fileHandle)
    {
        _files = files;
        _fileHandle = fileHandle;
    }

    internal byte[] GetBytes()
    {
        var assetStream = _files.GetAssetStream(_fileHandle);
        string text;
        using (var streamReader = new StreamReader(assetStream))
            text = streamReader.ReadToEnd();
        return Encoding.UTF8.GetBytes(text);
    }
}
