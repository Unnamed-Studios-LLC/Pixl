using ImGuiNET;
using System.Numerics;
using Veldrid;

namespace Pixl.Editor;

internal sealed class GameWindow : Window, IEditorUI
{
    private Vector2 _windowPosition;
    private Int2 _size;
    private bool _open = true;

    public override Int2 Size
    {
        get => _size;
        set { } // game cannot alter window size
    }
    public override WindowStyle Style { get; set; }
    public override string Title
    {
        get => "Game";
        set { } // game cannot alter window name
    }
    public RenderTexture? RenderTexture { get; set; }
    public bool Focused { get; private set; }
    public string Name => Title;
    public bool Open
    {
        get => _open;
        set => _open = value;
    }

    public override Int2 MousePosition => GetMousePosition();

    public override CursorState CursorState { get; set; }
    public override SwapchainSource SwapchainSource => throw new NotImplementedException();

    public void SubmitUI()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        SubmitGameWindow();
        ImGui.PopStyleVar(3);
    }

    public override void Start() { }
    public override void Stop() { }

    private Int2 GetMousePosition()
    {
        if (!_open) return Int2.Zero;

        var io = ImGui.GetIO();
        var relative = io.MousePos - _windowPosition;
        relative.Y = _size.Y - relative.Y - 1;
        return (Int2)relative.ToVec2();
    }

    private void SubmitGameWindow()
    {
        if (!ImGui.Begin(Name, ref _open))
        {
            return;
        }

        _windowPosition = ImGui.GetWindowPos();
        var contentMin = ImGui.GetWindowContentRegionMin();
        var contentMax = ImGui.GetWindowContentRegionMax();
        _size = (Int2)(contentMax - contentMin).ToVec2();
        Focused = ImGui.IsWindowFocused();
        if (RenderTexture != null)
        {
            ImGui.Image((nint)RenderTexture.Id, new Vector2(_size.X, _size.Y));
        }
        ImGui.End();
    }
}
