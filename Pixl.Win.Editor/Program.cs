using Pixl;
using Pixl.Demo;
using Pixl.Editor;
using Pixl.Win.Editor;
using System;
using System.IO;

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

    //var gameWindow = new EditorGameWindow(editorWindow.SwapchainSource, "Pixl Game");
    var gamePlayer = new EditorGamePlayer(editorWindow, logger);

    var game = new Game(resources, graphics, gamePlayer, new Entry());
    var editor = new Editor(resources, graphics, editorWindow, game);
    editorWindow.OnRender += editor.Update;

    game.Start();
    editor.Start();
    editorWindow.Run();
    editor.Stop();
    game.Stop();

    graphics.Stop(resources);

    editorPlayer.Logger.Flush();
    return editorPlayer.ExitCode;
}
catch (Exception e)
{
    File.WriteAllText("crash.log", e.ToString());
    return 1;
}