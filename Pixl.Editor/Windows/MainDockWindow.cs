using System;
using ImGuiNET;
using System.Numerics;

namespace Pixl.Editor;

internal sealed class MainDockWindow
{
    public void SubmitUI()
    {
        var io = ImGui.GetIO();

        var mainFlags = ImGuiWindowFlags.DockNodeHost | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoDecoration;
        ImGui.Begin("Main", mainFlags);
        ImGui.SetWindowSize(io.DisplaySize);
        ImGui.SetWindowPos(new Vector2(0, 0));
        var mainDock = ImGui.GetWindowDockID();
        ImGui.End();
    }
}

