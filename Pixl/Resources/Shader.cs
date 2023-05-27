using System.Text;

namespace Pixl;

public abstract class Shader
{
    private readonly AssetHandle _assetHandle;

    internal Shader(AssetHandle assetHandle)
    {
        _assetHandle = assetHandle;
    }

    internal byte[] GetBytes()
    {
        var assetStream = _assetHandle.GetStream();
        string text;
        using (var streamReader = new StreamReader(assetStream))
            text = streamReader.ReadToEnd();
        return Encoding.UTF8.GetBytes(text);
    }
}
