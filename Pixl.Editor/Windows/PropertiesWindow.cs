using ImGuiNET;

namespace Pixl.Editor;

internal sealed class PropertiesWindow : IEditorWindow
{
    private bool _open = true;

    public string Name => "Properties";
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

        ImGui.End();
    }
}
