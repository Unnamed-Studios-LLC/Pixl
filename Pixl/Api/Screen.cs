namespace Pixl;

public static class Screen
{
    /// <summary>
    /// <inheritdoc cref="Window.Size"/>
    /// </summary>
    public static Int2 Size
    {
        get => Game.Shared.Window.Size;
        set => Game.Shared.Window.Size = value;
    }

    /// <summary>
    /// <inheritdoc cref="Window.Title"/>
    /// </summary>
    public static string Title
    {
        get => Game.Shared.Window.Title;
        set => Game.Shared.Window.Title = value;
    }
}
