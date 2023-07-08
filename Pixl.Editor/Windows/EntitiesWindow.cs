namespace Pixl.Editor;

internal sealed class EntitiesWindow : EditorWindow
{
    private readonly Scene _scene;

    public EntitiesWindow(Scene scene)
    {
        _scene = scene;
    }

    public override string Name => "Entities";

    protected override void OnUI()
    {

    }
}
