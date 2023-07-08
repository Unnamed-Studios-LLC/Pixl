using Pixl;
using Pixl.Editor;
using Pixl.Win;
using System;
using System.IO;

try
{
    var window = new WinWindow("Pixl Editor", new Int2(1000, 800));
    var files = new Files(string.Empty, new WinFileBrowser(window), typeof(Game).Assembly, typeof(Editor).Assembly);
    var graphics = new Graphics();
    var resources = new Resources(graphics, files);

    var valueStore = new WinValueStore("Unnamed Studios", "Pixl Editor");
    var values = new Values(valueStore);
    var logger = new FileLogger(string.Empty, "editor.log", false);

    var editor = new Editor(window, graphics, resources, values, logger, files);
    editor.Start();
    graphics.Start(resources, window, GraphicsApi.DirectX);
    while (editor.Run())
    {
        graphics.SwapBuffers();
        editor.WaitForNextUpdate();
    }
    graphics.Stop(resources);
    editor.Stop();

    return editor.ExitCode;
}
catch (Exception e)
{
    File.WriteAllText("crash.log", e.ToString());
    return 1;
}