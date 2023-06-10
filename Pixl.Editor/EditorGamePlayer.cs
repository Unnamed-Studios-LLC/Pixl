using System.IO;

namespace Pixl.Editor;

internal sealed class EditorGamePlayer : IPlayer
{
    private readonly IPlayer _editorPlayer;

    public EditorGamePlayer(AppWindow window, IPlayer editorPlayer, ILogger logger)
    {
        Window = window;
        Logger = logger;
        _editorPlayer = editorPlayer;
    }

    public int ExitCode { get; set; }
    public string AssetsPath => _editorPlayer.AssetsPath;
    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pixl Editor", "Company Name", "Product Name");
    public AppWindow Window { get; }

    public ILogger Logger { get; }
}
