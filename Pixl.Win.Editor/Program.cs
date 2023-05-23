using Pixl;
using Pixl.Editor;
using Pixl.Win.Editor;
using System;
using System.IO;

try
{
    var window = new WinEditorWindow(new Int2(1000, 800));
    var player = new WinEditorPlayer();
    var resources = new Resources();
    var graphics = new Graphics();

    graphics.Start(resources, window, GraphicsApi.DirectX);

    var editor = new Editor(resources, graphics, window);
    window.OnRender += editor.Update;

    window.Run();

    graphics.Stop(resources);

    return player.ExitCode;
}
catch (Exception e)
{
    File.WriteAllText("crash.log", e.ToString());
    return 1;
}