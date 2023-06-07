using Pixl;
using Pixl.Demo;
using Pixl.Editor;
using Pixl.Win;
using Pixl.Win.Editor;
using System;
using System.IO;
using System.Threading;

try
{
    var resources = new Resources();
    var graphics = new Graphics();

    var editorWindow = new WinEditorWindow(new Int2(1000, 800));
    var editorPlayer = new WinEditorPlayer();

    graphics.Start(resources, editorWindow, GraphicsApi.DirectX);

#if DEBUG
    var logger = new BroadcastLogger(editorPlayer.MemoryLogger, new DiagnosticsLogger());
#else
    var logger = editorPlayer.MemoryLogger;
#endif

    var gameWindow = new EditorGameWindow(editorWindow, "Pixl Game", new Int2(500, 500));
    var gamePlayer = new EditorGamePlayer(gameWindow, editorPlayer, logger);

    var game = new Game(resources, graphics, gamePlayer, new Entry());
    var editor = new Editor(resources, graphics, editorWindow, game, gameWindow);

    var gameThread = new Thread(() => runEditor(editorWindow, editor, game, graphics));
    gameThread.Start();

    editorWindow.Run();

    gameThread.Join();
    graphics.Stop(resources);

    editorPlayer.Logger.Flush();
    return editorPlayer.ExitCode;
}
catch (Exception e)
{
    File.WriteAllText("crash.log", e.ToString());
    return 1;
}

static void runEditor(WinWindow window, Editor editor, Game game, Graphics graphics)
{
    game.Start();
    editor.Start();
    while (editor.Run())
    {
        graphics.SwapBuffers();
        editor.WaitForNextUpdate();
    }
    window.Stop();
    editor.Stop();
    game.Stop();
}