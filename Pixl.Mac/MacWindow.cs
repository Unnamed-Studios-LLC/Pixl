using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using AppKit;
using CoreFoundation;
using CoreGraphics;
using ObjCRuntime;
using Veldrid;
using Vulkan.Xlib;

[assembly: InternalsVisibleTo("Pixl.Mac.Editor")]
[assembly: InternalsVisibleTo("Pixl.Mac.Player")]

namespace Pixl.Mac;

internal class MacWindow : Window, INSWindowDelegate
{
    private readonly NSWindow _window;
    private readonly SwapchainSource _swapchainSource;

    private Int2 _clientSize;
    private WindowStyle _windowStyle;
    private string _windowTitle;
    private Int2 _mousePosition;
    private bool _stopped;

    public MacWindow(string title, Int2 size)
	{
        _clientSize = size;
        _windowStyle = WindowStyle.Windowed;
        _windowTitle = title;

        _window = new NSWindow(new CGRect(0, 0, _clientSize.X, _clientSize.Y), _windowStyle.GetWindowStyles(), NSBackingStore.Buffered, false)
        {
            Title = _windowTitle,
            IsOpaque = false,
            IsMovable = true,
            ContentView = new MacView(this),
            MovableByWindowBackground = true,
            BackgroundColor = NSColor.Black,
            Delegate = new WindowDelegate(this),
            AcceptsMouseMovedEvents = true
        };
        _window.Center();
        _window.MakeKeyAndOrderFront(null);

        _swapchainSource = SwapchainSource.CreateNSWindow(_window.Handle);
        _mousePosition = _window.MouseLocationOutsideOfEventStream.ToInt2();
    }

    public int ExitCode { get; set; }
    public override CursorState CursorState { get; set; }

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
    public override SwapchainSource SwapchainSource => _swapchainSource ?? throw new Exception("Window not created!");
    public override string Title
    {
        get => _windowTitle;
        set => SetWindowTitle(value);
    }

    public NativeHandle Handle => _window.Handle;
    public NSWindow? NSWindow => _window;

    public void Dispose()
    {

    }

    public void OnMouseMove(NSEvent theEvent)
    {
        _mousePosition = theEvent.LocationInWindow.ToInt2();
    }

    public override void Start()
    {

    }

    public override void Stop()
    {
        if (_stopped) return;
        _stopped = true;

        DispatchQueue.MainQueue.DispatchAsync(() =>
        {
            _window.Close();
            _window.Dispose();
        });
        PushEvent(new WindowEvent(WindowEventType.Quit));
    }

    internal void WillResize(CGSize toFrameSize)
    {
        if (_window == null) return;
        var clientSize = _window.ContentRectFor(new CGRect(0, 0, toFrameSize.Width, toFrameSize.Height));
        _clientSize = clientSize.Size.ToInt2();
    }

    private void SetWindowTitle(string value)
    {
        _windowTitle = value;
        if (_window == null) return;
        DispatchQueue.MainQueue.DispatchAsync(() =>
        {
            if (_window == null) return;
            _window.Title = value;
        });
    }
}

