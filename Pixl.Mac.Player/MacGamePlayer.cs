using System;
using System.IO;
using CoreFoundation;

namespace Pixl.Mac.Player;

internal sealed class MacGamePlayer : IPlayer
{
	public MacGamePlayer(AppWindow window)
	{
		Window = window;

        var mainBundle = CFBundle.GetMain();
        var resourcesPath = mainBundle?.ResourcesDirectoryUrl?.AbsoluteUrl?.ToString();
        if (resourcesPath == null) throw new Exception("Unable to determin resource path");
        resourcesPath = resourcesPath.Substring(7);

        AssetsPath = Path.Combine(resourcesPath, "Assets");
    }

    public int ExitCode { get; set; }

    public AppWindow Window { get; }

    public string DataPath => throw new NotImplementedException();
    public string AssetsPath { get; }

    public void Log(object @object)
    {
        System.Diagnostics.Debug.WriteLine(@object);
    }
}

