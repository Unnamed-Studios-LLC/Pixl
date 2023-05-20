namespace Pixl;

public static class Window
{
    /// <summary>
    /// <inheritdoc cref="AppWindow.Size"/>
    /// </summary>
    public static Int2 Size
    {
        get => CurrentWindow.Size;
        set => CurrentWindow.Size = value;
    }

    /// <summary>
    /// <inheritdoc cref="AppWindow.Style"/>
    /// </summary>
    public static WindowStyle Style => CurrentWindow.Style;

    /// <summary>
    /// <inheritdoc cref="AppWindow.Title"/>
    /// </summary>
    public static string Title
    {
        get => CurrentWindow.Title;
        set => CurrentWindow.Title = value;
    }

    /// <summary>
    /// <inheritdoc cref="Api.VSyncCount"/>
    /// </summary>
    public static int VSyncCount { get; set; } = 0;

    internal static AppWindow CurrentWindow => Game.Current.Player.Window;
}
