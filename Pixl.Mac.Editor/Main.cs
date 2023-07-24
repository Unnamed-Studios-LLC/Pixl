using AppKit;

namespace Pixl.Mac.Editor;

static class MainClass
{
	static void Main (string [] args)
	{
        NSApplication.Init();
        var application = NSApplication.SharedApplication;
        var appDelegate = new AppDelegate();
        application.Delegate = appDelegate;
        application.Run();
	}
}
