namespace Pixl;

internal readonly struct PlayerEvent
{
    public readonly PlayerEventType Type;
    public readonly int ValueA;
    public readonly int ValueB;

    public PlayerEvent(PlayerEventType type, int valueA, int valueB)
    {
        Type = type;
        ValueA = valueA;
        ValueB = valueB;
    }

    public PlayerEvent(PlayerEventType type, int valueA)
    {
        Type = type;
        ValueA = valueA;
        ValueB = 0;
    }

    public PlayerEvent(PlayerEventType type)
    {
        Type = type;
        ValueA = 0;
        ValueB = 0;
    }
}
