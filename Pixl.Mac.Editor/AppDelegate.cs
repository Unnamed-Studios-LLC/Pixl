using System.Threading;
using AppKit;
using Foundation;
using Pixl.Demo;
using Pixl.Editor;
using PixlEditor = Pixl.Editor.Editor;

namespace Pixl.Mac.Editor;

[Register ("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private MacEditorPlayer? _editorPlayer;
    private MacEditorWindow? _editorWindow;
    private EditorGamePlayer? _gamePlayer;
    private GameWindow? _gameWindow;
    private Resources? _resources;
	private Graphics? _graphics;
	private Game? _game;
    private PixlEditor? _editor;
    private Thread? _gameThread;

	public override void DidFinishLaunching (NSNotification notification)
	{
		// Insert code here to initialize your application
		_editorWindow = new MacEditorWindow(new Int2(1000, 800));
		_editorPlayer = new MacEditorPlayer();
		_resources = new();
		_graphics = new();

		_graphics.Start(_resources, _editorWindow, GraphicsApi.Metal);

#if DEBUG
        var logger = new BroadcastLogger(_editorPlayer.MemoryLogger, new DiagnosticsLogger());
#else
    var logger = editorPlayer.MemoryLogger;
#endif

        _gameWindow = new GameWindow(_editorWindow, "Pixl Game", new Int2(500, 500));
        _gamePlayer = new EditorGamePlayer(_gameWindow, _editorPlayer, logger);

        _game = new Game(_resources, _graphics, _gamePlayer, new Entry());
        _editor = new PixlEditor(_resources, _graphics, _editorWindow, _game, _gameWindow);
        _gameThread = new Thread(() => RunEditor(_editorWindow, _editor, _game, _graphics));
        _gameThread.Priority = ThreadPriority.Highest;
        _gameThread.IsBackground = false;
        _gameThread.Start();
    }

	public override void WillTerminate (NSNotification notification)
	{
        // Insert code here to tear down your application
        _editorWindow?.Stop();
        _editorWindow = null;

        _gameThread?.Join();

        if (_graphics != null && _resources != null)
        {
            _graphics.Stop(_resources);
        }

        _editorPlayer?.Logger.Flush();
    }

    private static void RunEditor(MacWindow window, PixlEditor editor, Game game, Graphics graphics)
    {
        game.Start();
        editor.Start();
        while (editor.Run())
        {
            graphics.SwapBuffers();
            editor.WaitForNextUpdate();
        }
        editor.Stop();
        game.Stop();
    }
}