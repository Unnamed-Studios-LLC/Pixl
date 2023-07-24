using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AppKit;
using CoreFoundation;
using Foundation;
using UniformTypeIdentifiers;

namespace Pixl.Mac
{
	internal sealed class MacFileBrowser : FileBrowser
	{
        private readonly MacWindow _window;

        public MacFileBrowser(MacWindow window)
        {
            _window = window;
        }

        public override string? Open(FileBrowserRequest request)
        {
            string? filePath = null;
            var waitEvent = new ManualResetEvent(false);
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var nsWindow = _window.NSWindow;
                if (nsWindow == null)
                {
                    waitEvent.Set();
                    return;
                }

                var panel = NSOpenPanel.OpenPanel;
                if (!string.IsNullOrWhiteSpace(request.Directory))
                {
                    panel.DirectoryUrl = NSUrl.FromFilename(request.Directory);
                }
                panel.AllowsMultipleSelection = false;
                panel.CanChooseDirectories = false;
                panel.CanChooseFiles = true;
                panel.AllowedContentTypes = request.Extensions.Select(x => UTType.CreateFromExtension(x.Extension)).Where(x => x != null).ToArray()!;
                panel.BeginSheet(nsWindow, result =>
                {
                    var response = (NSModalResponse)result;
                    if (response == NSModalResponse.OK)
                    {
                        filePath = panel.Url.RelativePath;
                    }
                    waitEvent.Set();
                });
            });
            waitEvent.WaitOne();
            return filePath;
        }

        public override string? OpenFolder(FileBrowserRequest request)
        {
            string? filePath = null;
            var waitEvent = new ManualResetEvent(false);
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var nsWindow = _window.NSWindow;
                if (nsWindow == null)
                {
                    waitEvent.Set();
                    return;
                }

                var panel = NSOpenPanel.OpenPanel;
                if (!string.IsNullOrWhiteSpace(request.Directory))
                {
                    panel.DirectoryUrl = NSUrl.FromFilename(request.Directory);
                }
                panel.AllowsMultipleSelection = false;
                panel.CanChooseDirectories = true;
                panel.CanChooseFiles = false;
                panel.BeginSheet(nsWindow, result =>
                {
                    var response = (NSModalResponse)result;
                    if (response == NSModalResponse.OK)
                    {
                        filePath = panel.Url.RelativePath;
                    }
                    waitEvent.Set();
                });
            });
            waitEvent.WaitOne();
            return filePath;
        }

        public override IEnumerable<string>? OpenMultiple(FileBrowserRequest request)
        {
            string[]? filePaths = null;
            var waitEvent = new ManualResetEvent(false);
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var nsWindow = _window.NSWindow;
                if (nsWindow == null)
                {
                    waitEvent.Set();
                    return;
                }

                var panel = NSOpenPanel.OpenPanel;
                if (!string.IsNullOrWhiteSpace(request.Directory))
                {
                    panel.DirectoryUrl = NSUrl.FromFilename(request.Directory);
                }
                panel.AllowsMultipleSelection = true;
                panel.CanChooseDirectories = false;
                panel.CanChooseFiles = true;
                panel.AllowedContentTypes = request.Extensions.Select(x => UTType.CreateFromExtension(x.Extension)).Where(x => x != null).ToArray()!;
                panel.BeginSheet(nsWindow, result =>
                {
                    var response = (NSModalResponse)result;
                    if (response == NSModalResponse.OK)
                    {
                        filePaths = panel.Urls.Select(x => x.RelativePath).Where(x => x != null).ToArray()!;
                    }
                    waitEvent.Set();
                });
            });
            waitEvent.WaitOne();
            return filePaths;
        }

        public override string? Save(FileBrowserRequest request)
        {
            string? filePath = null;
            var waitEvent = new ManualResetEvent(false);
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var nsWindow = _window.NSWindow;
                if (nsWindow == null)
                {
                    waitEvent.Set();
                    return;
                }

                var panel = NSSavePanel.SavePanel;
                if (!string.IsNullOrWhiteSpace(request.Directory))
                {
                    panel.DirectoryUrl = NSUrl.FromFilename(request.Directory);
                }
                panel.NameFieldStringValue = request.DefaultName;
                panel.AllowedContentTypes = request.Extensions.Select(x => UTType.CreateFromExtension(x.Extension)).Where(x => x != null).ToArray()!;
                panel.BeginSheet(nsWindow, result =>
                {
                    var response = (NSModalResponse)result;
                    if (response == NSModalResponse.OK)
                    {
                        filePath = panel.Url.RelativePath;
                    }
                    waitEvent.Set();
                });
            });
            waitEvent.WaitOne();
            return filePath;
        }
    }
}

