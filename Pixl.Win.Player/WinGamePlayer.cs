using System;
using System.IO;

namespace Pixl.Win.Player;

internal sealed class WinGamePlayer : IPlayer
{
    public int ExitCode { get; set; }

    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Company Name", "Product Name");
    public string AssetsPath => "Assets";
    public string InternalAssetsPath => "InternalAssets";

    public AppWindow Window { get; }

    public WinGamePlayer(AppWindow window)
    {
        Window = window;
    }

    public void Log(object @object)
    {
        System.Diagnostics.Debug.WriteLine(@object);
    }
}
