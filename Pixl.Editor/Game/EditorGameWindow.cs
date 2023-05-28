﻿using ImGuiNET;
using System.Numerics;
using Veldrid;

namespace Pixl.Editor
{
    internal class EditorGameWindow : AppWindow
    {
        private Int2 _size;
        private readonly AppWindow _editorWindow;

        public EditorGameWindow(AppWindow mainWindow, string title, Int2 size)
        {
            _editorWindow = mainWindow;
            Title = title;
            _size = size;
        }

        public override Int2 Size
        {
            get => _size;
            set
            {
                // game cannot alter window size
            }
        }
        public override Int2 MousePosition => _editorWindow.MousePosition;
        public override WindowStyle Style { get; set; }
        public override SwapchainSource SwapchainSource => _editorWindow.SwapchainSource;
        public override string Title { get; set; }
        public RenderTexture? RenderTexture { get; set; }
        public bool Focused { get; private set; }

        public void SubmitUi()
        {
            ImGui.Begin("Game");
            ImGui.SetWindowSize(new Vector2(_size.X, _size.Y), ImGuiCond.FirstUseEver);
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
}
