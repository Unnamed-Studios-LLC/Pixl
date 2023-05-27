using Foundation;
using Pixl.Demo;
using Pixl.iOS.Player;
using UIKit;
using Veldrid.MetalBindings;

namespace Pixl.iOS.Player2;

[Register ("AppDelegate")]
public class AppDelegate : UIApplicationDelegate {

    private iOSWindow? _window;
    private iOSGamePlayer? _player;
    private Resources? _resources;
    private Graphics? _graphics;
    private Game? _game;

    public override UIWindow? Window {
		get;
		set;
	}

	public override bool FinishedLaunching (UIApplication application, Foundation.NSDictionary launchOptions)
	{
        // create a new window instance based on the screen size
        _window = new iOSWindow();
        _player = new iOSGamePlayer(_window);
        _resources = new();
        _graphics = new();

        _graphics.Start(_resources, _window, GraphicsApi.Metal);

        _game = new Game(_resources, _graphics, _player, new Entry());
        _window.Start(_game);

        return true;
    }

    public override void WillTerminate(UIApplication application)
    {
        // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        _window?.Stop();
        _window = null;

        if (_graphics != null && _resources != null)
        {
            _graphics.Stop(_resources);
        }
    }
}

