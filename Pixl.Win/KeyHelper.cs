namespace Pixl.Win;

public static class KeyHelper
{
    public static KeyCode GetKeyCode(int winKeyCode)
    {
        return winKeyCode switch
        {
            >= 0x30 and <= 0x39 => KeyCode.Alpha0 + winKeyCode - 0x30,
            >= 0x41 and <= 0x5A => KeyCode.A + winKeyCode - 0x41,
            _ => KeyCode.None
        };
    }
}
