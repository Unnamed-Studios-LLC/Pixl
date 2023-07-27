using ImGuiNET;
using System.Numerics;

namespace Pixl.Editor;

internal abstract class EditorModal : IEditorUI
{
    private bool _popupOpen = false;

    public abstract string Name { get; }
    public bool Open { get; set; }

    protected virtual bool CanClose => true;
    protected virtual bool ShouldOpen => Open;

    public bool SubmitModal()
    {
        Open = ShouldOpen;
        _popupOpen = ImGui.IsPopupOpen(Name);

        if (Open && !_popupOpen)
        {
            ImGui.OpenPopup(Name);
            _popupOpen = true;

            // Always center this window when appearing
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }

        if (ImGui.BeginPopupModal(Name, ref _popupOpen, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize))
        {
            if (!Open)
            {
                ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
                return false;
            }

            SubmitUI();
            return true;
        }
        else if (CanClose)
        {
            Open = false;
        }

        return false;
    }

    public abstract void SubmitUI();
}