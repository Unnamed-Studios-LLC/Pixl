using ImGuiNET;
using System.Numerics;

namespace Pixl.Editor;

internal abstract class EditorWindow : IEditorUI
{
    private bool _open = false;

    public abstract string Name { get; }
    public bool Open
    {
        get => _open;
        set => _open = value;
    }
    public bool Borderless { get; set; }
    public bool UnsavedChanges { get; set; }

    public virtual void Save() { }

    public void SubmitUI()
    {
        var borderless = Borderless;
        if (borderless)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        }

        var flags = UnsavedChanges ? ImGuiWindowFlags.UnsavedDocument : ImGuiWindowFlags.None;
        if (ImGui.Begin(Name, ref _open, flags))
        {
            OnUI();
            ImGui.End();
        }

        if (borderless)
        {
            ImGui.PopStyleVar(3);
        }
    }

    protected abstract void OnUI();
}
