namespace Pixl;

public static class Input
{
    public static bool GetKey(KeyCode keyCode) => Game.Shared.GetKey(keyCode);
    public static bool GetKeyDown(KeyCode keyCode) => Game.Shared.GetKeyDown(keyCode);
    public static bool GetKeyUp(KeyCode keyCode) => Game.Shared.GetKeyUp(keyCode);

    public static bool GetMouseButton(int index = 0) => GetKey(KeyCode.Mouse0 + index);
    public static bool GetMouseButtonDown(int index = 0) => GetKeyDown(KeyCode.Mouse0 + index);
    public static bool GetMouseButtonUp(int index = 0) => GetKeyUp(KeyCode.Mouse0 + index);
}