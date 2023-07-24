using System;
using System.Threading;
using AppKit;
using CoreGraphics;
using CoreVideo;
using Foundation;
using PixlEditor = Pixl.Editor.Editor;

namespace Pixl.Mac.Editor;

[Register ("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private MacWindow? _window;
    private Resources? _resources;
	private Graphics? _graphics;
    private Files? _files;
    private PixlEditor? _editor;
    private CVDisplayLink? _displayLink;
    private readonly object _renderLock = new();

	public override void DidFinishLaunching (NSNotification notification)
	{
        // Insert code here to initialize your application
        _window = new MacWindow("Pixl Editor", new Int2(1000, 800));
        _files = new(string.Empty, new MacFileBrowser(_window), typeof(Game).Assembly, typeof(PixlEditor).Assembly);
		_graphics = new();
		_resources = new(_graphics, _files);
        var logger = new FileLogger(string.Empty, "editor.log", false);
        var values = new Values(new MacValuesStore("co.unnamedstudios.pixl"));

        _graphics.Start(_resources, _window, GraphicsApi.Metal);
        _editor = new PixlEditor(_window, _graphics, _resources, values, logger, _files);
        _editor.Start();

        var displayNumber = (NSNumber)NSScreen.MainScreen.DeviceDescription["NSScreenNumber"];
        var displayId = CGDisplay.GetDisplayID(int.MaxValue);
        _displayLink = CVDisplayLink.CreateFromDisplayId(displayNumber.UInt32Value, out var result);
        if (_displayLink == null || result != CVReturn.Success)
        {
            throw new Exception($"Failed to create CVDisplayLink: {result}");
        }

        _displayLink.SetOutputCallback(Render);
        _displayLink.Start();
    }

	public override void WillTerminate (NSNotification notification)
	{
        lock (_renderLock)
        {
            _window?.Stop();
            _window = null;

            _editor?.Stop();
            _editor = null;

            if (_graphics != null && _resources != null)
            {
                _graphics.Stop(_resources);
                _graphics = null;
            }
        }

        _displayLink?.Stop();
        _displayLink?.Dispose();
        _displayLink = null;
    }

    private CVReturn Render(CVDisplayLink displayLink, ref CVTimeStamp inNow, ref CVTimeStamp inOutputTime, CVOptionFlags flagsIn, ref CVOptionFlags flagsOut)
    {
        lock (_renderLock)
        {
            if (_editor != null)
            {
                if (!_editor.Run()) return CVReturn.Error;
                _graphics?.SwapBuffers();
            }
            return CVReturn.Success;
        }
    }
}