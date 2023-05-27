using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Pixl.Mac
{
	internal sealed class WindowDelegate : NSWindowDelegate
	{
        private readonly MacWindow _window;

        public WindowDelegate(MacWindow window)
		{
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        public override void WillClose(NSNotification notification)
        {
            NSApplication.SharedApplication.Terminate(null);
            _window.PushEvent(new WindowEvent(WindowEventType.Quit));
        }

        public override CGSize WillResize(NSWindow sender, CGSize toFrameSize)
        {
            _window.WillResize(toFrameSize);
            return toFrameSize;
        }
    }
}

