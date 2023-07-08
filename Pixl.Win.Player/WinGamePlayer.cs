using System;
using System.IO;

namespace Pixl.Win.Player;

internal sealed class WinGamePlayer : IPlayer
{
    public int ExitCode { get; set; }

    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Company Name", "Product Name");
    public string AssetsPath => "Assets";

    public Window Window { get; }
    public Logger Logger { get; }

    public WinGamePlayer(Window window)
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
