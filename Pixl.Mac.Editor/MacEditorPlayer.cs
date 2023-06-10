using System;
using System.IO;
using CoreFoundation;

namespace Pixl.Mac.Editor;

internal sealed class MacEditorPlayer : IPlayer
{
	public MacEditorPlayer()
    {
        MemoryLogger = new(1100, 100);

        var mainBundle = CFBundle.GetMain();
        var resourcesPath = mainBundle?.ResourcesDirectoryUrl?.AbsoluteUrl?.ToString();
        if (resourcesPath == null) throw new Exception("Unable to determin resource path");
        resourcesPath = resourcesPath.Substring(7);

        AssetsPath = Path.Combine(resourcesPath, "Assets");
    }

    public int ExitCode { get; set; }

    public string AssetsPath { get; }
    public string DataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Pixl Editor", "Company Name", "Product Name");

    public AppWindow Window => throw new NotImplementedException();
    public ILogger Logger => MemoryLogger;

    public MemoryLogger MemoryLogger { get; }

}

