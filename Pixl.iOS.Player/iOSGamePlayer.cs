using System;

namespace Pixl.iOS.Player;

internal sealed class iOSGamePlayer : IPlayer
{
	public iOSGamePlayer(Window window)
	{
        Window = window;

        DataPath = string.Empty;
        AssetsPath = string.Empty;
	}

    public int ExitCode { get; set; }

    public string DataPath { get; }

    public string AssetsPath { get; }

    public Window Window { get; }

    public void Log(object @object)
    {
        System.Diagnostics.Debug.WriteLine(@object);
    }
}

