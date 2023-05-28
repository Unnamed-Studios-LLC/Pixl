using ImGuiNET;
using System.Numerics;
using Veldrid;

namespace Pixl.Editor
{
    internal class EditorGameWindow : AppWindow
    {
        private readonly AppWindow _editorWindow;

        public EditorGameWindow(AppWindow mainWindow, string title, Int2 size)
        {
            _editorWindow = mainWindow;
            Title = title;
            Size = size;
        }

        public override Int2 Size { get; set; }
        public override Int2 MousePosition => _editorWindow.MousePosition;
        public override WindowStyle Style { get; set; }
        public override SwapchainSource SwapchainSource => _editorWindow.SwapchainSource;
        public override string Title { get; set; }
        public RenderTexture? RenderTexture { get; set; }

        public void SubmitUI()
        {
            ImGui.Begin("Game");
            if (RenderTexture != null)
            {
                ImGui.Image((nint)RenderTexture.Id, new Vector2(Size.X, Size.Y));
            }
            ImGui.End();
        }
    }
}
