using System;
using System.IO;

namespace Pixl.Win.Editor;

internal sealed class WinEditorPlayer : IPlayer
{
    public int ExitCode { get; set; }

    public string AssetsPath => "Assets";
    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pixl Editor", "Company Name", "Product Name");
    public string InternalAssetsPath => "InternalAssets";

    public AppWindow Window => throw new NotImplementedException();

    public void Log(object @object)
    {
        throw new NotImplementedException();
    }
}
