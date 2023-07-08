using ImGuiNET;
using System.Numerics;

namespace Pixl.Editor;

internal abstract class EditorModal : IEditorUI
{
    private bool _open = false;

    public abstract string Name { get; }
    public bool Open
    {
        get => _open;
        set => _open = value;
    }

    protected virtual bool ShouldOpen => Open;

    public void SubmitUI() => SubmitUI(null);
    public bool SubmitUI(Action? next)
    {
        if (ShouldOpen)
        {
            ImGui.OpenPopup(Name);
            Open = ShouldOpen;
        }

        // Always center this window when appearing
        var center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));

        var open = Open;
        if (ImGui.BeginPopupModal(Name, ref open, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize))
        {
            OnUI();

            next?.Invoke();
            ImGui.EndPopup();
            return true;
        }

        return false;
    }

    protected abstract void OnUI();
}