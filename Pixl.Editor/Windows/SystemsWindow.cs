using ImGuiNET;

namespace Pixl.Editor;

internal sealed class SystemsWindow : IEditorWindow
{
    private readonly Scene _scene;
    private ComponentSystem? _selectedSystem;
    private bool _open = true;

    public SystemsWindow(Scene scene)
    {
        _scene = scene;
    }

    public string Name => "Systems";
    public bool Open
    {
        get => _open;
        set => _open = value;
    }

    public void SubmitUI()
    {
        if (!ImGui.Begin(Name, ref _open))
        {
            return;
        }

        var systems = _scene.GetSystems();
        foreach (var system in systems)
        {
            if (system == null) continue;
            if (ImGui.Selectable(system.GetType().Name, _selectedSystem == system))
            {
                _selectedSystem = system;
            }
        }

        ImGui.End();
    }
}
