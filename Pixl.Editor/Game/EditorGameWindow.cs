using Veldrid;

namespace Pixl.Editor
{
    internal class EditorGameWindow : AppWindow
    {
        private Int2 _mousePosition;

        public EditorGameWindow(SwapchainSource swapchainSource, string title)
        {
            SwapchainSource = swapchainSource;
            Title = title;
        }

        public override Int2 Size { get; set; }
        public override Int2 MousePosition => _mousePosition;
        public override WindowStyle Style { get; set; }
        public override SwapchainSource SwapchainSource { get; }
        public override string Title { get; set; }

        public void SetMousePosition(Int2 mousePosition)
        {
            _mousePosition = mousePosition;
        }
    }
}
