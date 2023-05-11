namespace Pixl;

public static class Window
{
    /// <summary>
    /// <inheritdoc cref="Api.WindowSize"/>
    /// </summary>
    public static Int2 Size
    {
        get => Game.Current.WindowSize;
        set => Game.Current.WindowSize = value;
    }

    /// <summary>
    /// <inheritdoc cref="Api.WindowStyle"/>
    /// </summary>
    public static WindowStyle Style => Game.Current.WindowStyle;

    /// <summary>
    /// <inheritdoc cref="Api.WindowTitle"/>
    /// </summary>
    public static string Title
    {
        get => Game.Current.WindowTitle;
        set => Game.Current.WindowTitle = value;
    }

    /// <summary>
    /// <inheritdoc cref="Api.VSyncCount"/>
    /// </summary>
    public static int VSyncCount { get; set; } = 0;
}
