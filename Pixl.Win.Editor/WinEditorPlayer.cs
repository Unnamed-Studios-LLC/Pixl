using System;
using System.IO;

namespace Pixl.Win.Editor;

internal sealed class WinEditorPlayer : IPlayer
{
    public WinEditorPlayer()
    {
        MemoryLogger = new(8388608, 4096); // 8 MB
    }

    public int ExitCode { get; set; }

    public string AssetsPath => "Assets";
    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pixl Editor", "Company Name", "Product Name");

    public AppWindow Window => throw new NotImplementedException();
    public ILogger Logger => MemoryLogger;

    public MemoryLogger MemoryLogger { get; }
}
