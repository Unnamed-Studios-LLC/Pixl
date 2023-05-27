using System;
using System.Runtime.CompilerServices;
using CoreAnimation;
using Foundation;
using UIKit;
using Veldrid;

[assembly: InternalsVisibleTo("Pixl.iOS.Player")]

namespace Pixl.iOS;

internal sealed class iOSWindow : AppWindow
{
    private readonly Int2 _screenSize;
    private readonly UIWindow _window;
    private CADisplayLink? _timer;
    private Game? _game;

    public iOSWindow()
    {
        var screen = UIScreen.MainScreen;
        _screenSize = screen.Bounds.Size.ToInt2();

        _window = new UIWindow(screen.Bounds);
        _window.RootViewController = new GameViewController();
        _window.MakeKeyAndVisible();

        var view = _window.RootViewController.View ?? throw new Exception("RootViewController view is null!");
        SwapchainSource = SwapchainSource.CreateUIView(view.Handle);
    }

    public override Int2 Size
    {
        get => _screenSize;
        set { } // cannot set app screen size
    }

    public override Int2 MousePosition { get; }

    public override WindowStyle Style { get; set; }

    public override SwapchainSource SwapchainSource { get; }

    public override string Title { get; set; } = string.Empty;

    public void Start(Game game)
    {
        _timer = CADisplayLink.Create(Update);
        _timer.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
    }

    public void Stop()
    {
        _window.Dispose();
        PushEvent(new WindowEvent(WindowEventType.Quit));
    }

    private void Update()
    {
        _game?.Run();
    }
}

