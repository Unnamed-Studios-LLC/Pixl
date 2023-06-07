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

    public override bool MouseDownCanMoveWindow => false;

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

    public override void MouseDragged(NSEvent theEvent)
    {
        _window.OnMouseMove(theEvent);
    }

    public override void MouseDown(NSEvent theEvent)
    {
        _window.PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)KeyCode.Mouse0));
    }

    public override void MouseUp(NSEvent theEvent)
    {
        _window.PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)KeyCode.Mouse0));
    }

    public override void MouseMoved(NSEvent theEvent)
    {
        _window.OnMouseMove(theEvent);
    }

    public override void RightMouseDragged(NSEvent theEvent)
    {
        _window.OnMouseMove(theEvent);
    }

    public override void RightMouseDown(NSEvent theEvent)
    {
        _window.PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)KeyCode.Mouse1));
    }

    public override void RightMouseUp(NSEvent theEvent)
    {
        _window.PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)KeyCode.Mouse1));
    }

    public override void OtherMouseDragged(NSEvent theEvent)
    {
        _window.OnMouseMove(theEvent);
    }

    public override void OtherMouseDown(NSEvent theEvent)
    {
        // TODO correct mouse codes
        _window.PushEvent(new WindowEvent(WindowEventType.KeyDown, (int)KeyCode.Mouse1));
    }

    public override void OtherMouseUp(NSEvent theEvent)
    {
        _window.PushEvent(new WindowEvent(WindowEventType.KeyUp, (int)KeyCode.Mouse1));
    }
}

