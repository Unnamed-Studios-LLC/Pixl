using System;
using AppKit;
using MetalKit;

namespace Pixl.Mac;

internal sealed class MacView : NSView
{
    private readonly MacWindow _window;

    public MacView(MacWindow window)
    {
        _window = window;
    }

    public override bool AcceptsFirstResponder() => true;
    public override bool AcceptsFirstMouse(NSEvent theEvent) => true;

    public override void KeyDown(NSEvent theEvent)
    {
        var keyCode = MacKeyHelper.GetKeyCode(theEvent.KeyCode);
        if (keyCode == KeyCode.None) return;
        _window.PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)keyCode));
    }

    public override void KeyUp(NSEvent theEvent)
    {
        var keyCode = MacKeyHelper.GetKeyCode(theEvent.KeyCode);
        if (keyCode == KeyCode.None) return;
        _window.PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)keyCode));
    }
}

