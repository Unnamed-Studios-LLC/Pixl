using System.IO;

namespace Pixl.Editor;

internal sealed class EditorGamePlayer : IPlayer
{
    public EditorGamePlayer(AppWindow window, ILogger logger)
    {
        Window = window;
        Logger = logger;
    }

    public int ExitCode { get; set; }
    public string AssetsPath => "Assets";
    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pixl Editor", "Company Name", "Product Name");
    public AppWindow Window { get; }

    public ILogger Logger { get; }
}
