using ImGuiNET;

namespace Pixl.Editor;

internal sealed class TestWindow : EditorWindow
{
    public override string Name => "Test";

    protected override void OnUI()
    {
        float framerate = ImGui.GetIO().Framerate;
        ImGui.Text("Hello, world!");
        ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");
        ImGui.SameLine(0, -1);
        ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
    }
}
