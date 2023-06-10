using ImGuiNET;

namespace Pixl.Editor;

internal sealed class TestWindow : IEditorWindow
{
    private bool _open = false;

    public string Name => "Test";
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

        float framerate = ImGui.GetIO().Framerate;
        ImGui.Text("Hello, world!");
        ImGui.Text($"Mouse position: {ImGui.GetMousePos()}");
        ImGui.SameLine(0, -1);
        ImGui.Text($"Application average {1000.0f / framerate:0.##} ms/frame ({framerate:0.#} FPS)");
        ImGui.End();
    }
}
