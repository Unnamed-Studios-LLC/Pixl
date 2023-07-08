using ImGuiNET;

namespace Pixl.Editor;

internal sealed class SystemsWindow : EditorWindow
{
    private readonly Scene _scene;
    private readonly PropertiesWindow _properties;

    public SystemsWindow(Scene scene, PropertiesWindow properties)
    {
        _scene = scene;
        _properties = properties;
    }

    public override string Name => "Systems";

    protected override void OnUI()
    {
        var systems = _scene.GetSystems();
        foreach (var system in systems)
        {
            if (system == null) continue;
            if (ImGui.Selectable(system.GetType().Name, _properties.SelectedObject == system))
            {
                _properties.SelectedObject = system;
            }
        }
    }
}
