namespace Pixl;

public sealed class FragmentShader : Shader
{
    public FragmentShader(string filePath) : this(AssetHandle.CreateAbsolutePath(filePath)) { }

    internal FragmentShader(AssetHandle assetHandle) : base(assetHandle) { }
}
