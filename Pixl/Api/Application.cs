namespace Pixl;

internal static class Application
{
    /// <inheritdoc cref="Api.AssetsPath"/>
    public static string AssetsPath => Game.Current.AssetsPath;

    /// <inheritdoc cref="Api.DataPath"/>
    public static string DataPath => Game.Current.DataPath;

    public static GraphicsApi GraphicsApi
    {
        get => Game.Current.Graphics.Api;
        set => Game.Current.SwitchGraphicsApi(value);
    }
}
