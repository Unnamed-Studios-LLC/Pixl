using System;

namespace Pixl.Win.Editor;

internal sealed class WinEditorWindow : WinWindow
{
    public event Action? OnRender;

    public WinEditorWindow(Int2 windowSize) : base("Pixl Editor", windowSize)
    {
    }

    public override void PushEvent(in WindowEvent @event)
    {
        switch (@event.Type)
        {
            case WindowEventType.Render:
                OnRender?.Invoke();
                return;
            case WindowEventType.MouseMove:
                OnRender?.Invoke();
                return;
        }

        base.PushEvent(@event);
    }
}
