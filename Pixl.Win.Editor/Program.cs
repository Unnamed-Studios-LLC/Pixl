using Pixl;
using Pixl.Editor;
using Pixl.Win.Editor;

try
{
    var window = new WinEditorWindow(new Int2(1000, 800));
    var player = new WinEditorPlayer();

    var worldToClipMatrix = SharedProperty.Create(PropertyDescriptor.CreateStandard(PropertyType.Mat4));
    var defaultMaterial = Material.CreateDefault(player.InternalAssetsPath, worldToClipMatrix);
    var errorMaterial = Material.CreateDefault(player.InternalAssetsPath, worldToClipMatrix);

    Resources resources = new(
        worldToClipMatrix,
        defaultMaterial,
        errorMaterial
    );

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