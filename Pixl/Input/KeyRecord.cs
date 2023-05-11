namespace Pixl;

internal struct KeyRecord
{
    public KeyState State;
    public long Time;

    public KeyRecord(KeyState state, long time)
    {
        State = state;
        Time = time;
    }
}
