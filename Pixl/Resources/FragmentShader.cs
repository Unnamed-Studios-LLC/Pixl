namespace Pixl;

public sealed class FragmentShader : Shader
{
    internal FragmentShader(Files files, FileHandle assetHandle) : base(files, assetHandle) { }

    public static FragmentShader Create(string filePath)
    {
        Game.Shared.RequireMainThread();
        return new FragmentShader(Game.Shared.Files, FileHandle.CreateExternal(filePath));
    }
}
