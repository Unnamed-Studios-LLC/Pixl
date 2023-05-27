using System.Reflection;
using System.Runtime.CompilerServices;

namespace Pixl;

internal static class Application
{
    /// <inheritdoc cref="Api.AssetsPath"/>
    public static string AssetsPath => Game.Current.AssetsPath;

    /// <inheritdoc cref="Api.DataPath"/>
    public static string DataPath => Game.Current.DataPath;

    /// <summary>
    /// The graphics api currently being used
    /// </summary>
    public static GraphicsApi GraphicsApi
    {
        get => Game.Current.Graphics.Api;
        set => Game.Current.SwitchGraphicsApi(value);
    }

    /// <inheritdoc cref="Api.MainThreadId"/>
    public static int MainThreadId => Game.Current.MainThreadId;

    /// <summary>
    /// If execution is currently on the main thread
    /// </summary>
    public static bool OnMainThread => Environment.CurrentManagedThreadId == MainThreadId;

    /// <summary>
    /// Must be called from the main thread. See <see cref="Application.OnMainThread"/>.
    /// </summary>
    internal static void RequireMainThread([CallerMemberName] string? callerName = null)
    {
        if (!OnMainThread) throw new Exception($"{callerName} must be called on the main thread");
    }
}
