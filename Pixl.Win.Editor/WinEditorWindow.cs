namespace Pixl.Win.Editor;

internal sealed class WinEditorWindow : WinWindow
{
    public event Action? OnRender;

    public WinEditorWindow(Int2 windowSize) : base("Pixl Editor", windowSize)
    {

    }

    protected override void OnEvent(in WindowEvent @event)
    {
        switch (@event.Type)
        {
            case WindowEventType.Render:
                OnRender?.Invoke();
                return;
        }

        base.OnEvent(@event);
    }
}
