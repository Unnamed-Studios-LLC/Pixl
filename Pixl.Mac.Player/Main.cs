using System.Threading;
using System.Xml.Linq;
using AppKit;
using Pixl;
using Pixl.Demo;
using ScriptingBridge;
using Vortice.Mathematics;

namespace Pixl.Mac.Player;

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
