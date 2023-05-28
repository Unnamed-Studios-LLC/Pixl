using System;
using System.IO;

namespace Pixl.Win.Player;

internal sealed class WinGamePlayer : IPlayer
{
    public int ExitCode { get; set; }

    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Company Name", "Product Name");
    public string AssetsPath => "Assets";

    public AppWindow Window { get; }
    public ILogger Logger { get; }

    public WinGamePlayer(AppWindow window)
    {
        Window = window;

        Logger = new FileLogger(
            DataPath,
            "output",
#if DEBUG
            true
#else
            false
#endif
        );
    }
}
