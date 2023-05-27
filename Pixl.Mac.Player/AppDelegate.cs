using System.Threading;
using AppKit;
using Foundation;
using Pixl.Demo;

namespace Pixl.Mac.Player
{
    [Register ("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		private MacWindow? _window;
        private MacGamePlayer? _player;
		private Resources? _resources;
		private Graphics? _graphics;
		private Game? _game;
        private Thread? _gameThread;

		public override void DidFinishLaunching (NSNotification notification)
		{
			// Insert code here to initialize your application
			_window = new MacWindow("Pixl Game", new Int2(1000, 800));
			_player = new MacGamePlayer(_window);
			_resources = new();
			_graphics = new();

			_graphics.Start(_resources, _window, GraphicsApi.Metal);

            _game = new Game(_resources, _graphics, _player, new Entry());
            _gameThread = new Thread(() => RunGame(_window, _game, _graphics));
            _gameThread.Priority = ThreadPriority.Highest;
            _gameThread.IsBackground = false;
            _gameThread.Start();
        }

		public override void WillTerminate (NSNotification notification)
		{
            // Insert code here to tear down your application
            _window?.Stop();
            _window = null;

            _gameThread?.Join();

            if (_graphics != null && _resources != null)
            {
                _graphics.Stop(_resources);
            }
        }

        private static void RunGame(MacWindow window, Game game, Graphics graphics)
        {
            game.Start();
            while (game.Run())
            {
                graphics.SwapBuffers();
                game.WaitForNextUpdate();
            }
            game.Stop();
        }
    }
}

