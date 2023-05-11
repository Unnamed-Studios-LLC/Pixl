namespace Pixl;

public static class Debug
{
    /// <inheritdoc cref="Api.Log"/>
    public static void Log(object @object) => Game.Current.Log(@object);
}
