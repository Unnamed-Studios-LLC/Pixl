namespace Pixl;

internal sealed class InputState
{
    private readonly KeyRecord[] _keys = new KeyRecord[(int)KeyCode.Count];

    public ref KeyRecord GetKeyRecord(KeyCode keyCode) => ref _keys[(int)keyCode - 1];

    public void OnKeyDown(KeyCode keyCode, long time)
    {
        _keys[(int)keyCode - 1] = new KeyRecord(KeyState.Pressed, time);
    }

    public void OnKeyUp(KeyCode keyCode, long time)
    {
        _keys[(int)keyCode - 1] = new KeyRecord(KeyState.Released, time);
    }
}
