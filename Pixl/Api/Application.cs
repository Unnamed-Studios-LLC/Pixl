namespace Pixl;

internal static class Application
{
    /// <inheritdoc cref="Api.AssetsPath"/>
    public static string AssetsPath => Game.Current.AssetsPath;

    /// <inheritdoc cref="Api.DataPath"/>
    public static string DataPath => Game.Current.DataPath;
}
