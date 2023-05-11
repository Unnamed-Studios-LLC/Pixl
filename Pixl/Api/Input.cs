namespace Pixl;

public static class Input
{
    public static bool GetKey(KeyCode keyCode) => Game.Current.GetKey(keyCode);

    public static bool GetKeyDown(KeyCode keyCode) => Game.Current.GetKeyDown(keyCode);

    public static bool GetKeyUp(KeyCode keyCode) => Game.Current.GetKeyUp(keyCode);

    public static bool GetMouseButton() => Game.Current.GetMouseButton();

    public static bool GetMouseButtonDown() => Game.Current.GetMouseButtonDown();

    public static bool GetMouseButtonUp() => Game.Current.GetMouseButtonUp();
}
