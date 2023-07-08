namespace Pixl;

public static class Debug
{
    public static void Log(object @object) => Game.Shared.Logger.Log(@object);
}
