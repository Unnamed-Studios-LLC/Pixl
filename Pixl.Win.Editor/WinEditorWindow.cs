namespace Pixl.Win.Editor;

internal sealed class WinEditorWindow : WinWindow
{
    public WinEditorWindow(Int2 windowSize) : base("Pixl Editor", windowSize)
    {
    }

    private static bool IsRenderableEvent(WindowEventType type)
    {
        return type switch
        {
            WindowEventType.Render or
            WindowEventType.MouseMove or
            WindowEventType.KeyDown or
            WindowEventType.KeyUp or
            WindowEventType.Scroll => true,
            _ => false
        };
    }
}
