using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AppKit;
using CoreFoundation;
using CoreGraphics;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using Veldrid;

[assembly: InternalsVisibleTo("Pixl.Mac.Editor")]
[assembly: InternalsVisibleTo("Pixl.Mac.Player")]

namespace Pixl.Mac;

internal class MacWindow : Window, INSWindowDelegate
{
    private readonly NSWindow _window;
    private readonly MacView _view;

    private Int2 _clientSize;
    private WindowStyle _windowStyle;
    private string _windowTitle;
    private Int2 _mousePosition;

    public MacWindow(string title, Int2 size)
	{
        _clientSize = size;
        _windowStyle = WindowStyle.Windowed;
        _windowTitle = title;

        _view = new MacView(this);

        _window = new NSWindow(new CGRect(0, 0, size.X, size.Y), _windowStyle.GetWindowStyles(), NSBackingStore.Buffered, false);
        _window.Title = title;
        _window.IsOpaque = false;
        _window.Center();
        _window.IsMovable = true;
        _window.ContentView = _view;
        _window.MovableByWindowBackground = true;
        _window.BackgroundColor = NSColor.Black;
        _window.Delegate = new WindowDelegate(this);
        _window.AcceptsMouseMovedEvents = true;
        _window.MakeKeyAndOrderFront(null);

        SwapchainSource = SwapchainSource.CreateNSWindow(_window.Handle);
        _mousePosition = _window.MouseLocationOutsideOfEventStream.ToInt2();
    }

    public int ExitCode { get; set; }

    public override Int2 MousePosition => _mousePosition;

    public override Int2 Size
    {
        get => _clientSize;
        set => throw new NotImplementedException();
    }
    public override WindowStyle Style
    {
        get => _windowStyle;
        set => throw new NotImplementedException();
    }
    public override SwapchainSource SwapchainSource { get; }
    public override string Title
    {
        get => _windowTitle;
        set => SetWindowTitle(value);
    }

    public NativeHandle Handle => NativeHandle.Zero;

    public void Dispose()
    {

    }

    public void OnMouseMove(NSEvent theEvent)
    {
        _mousePosition = theEvent.LocationInWindow.ToInt2();
        Debug.Log(_mousePosition);
    }

    public void Stop()
    {
        _window.Close();
        _window.Dispose();
        PushEvent(new WindowEvent(WindowEventType.Quit));
    }

    internal void WillResize(CGSize toFrameSize)
    {
        var clientSize = _window.ContentRectFor(new CGRect(0, 0, toFrameSize.Width, toFrameSize.Height));
        _clientSize = clientSize.Size.ToInt2();
    }

    private void SetWindowTitle(string value)
    {
        _windowTitle = value;
        DispatchQueue.MainQueue.DispatchAsync(() =>
        {
            _window.Title = value;
        });
    }
}

