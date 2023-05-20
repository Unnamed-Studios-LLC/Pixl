namespace Pixl;

internal readonly struct WindowEvent
{
    public readonly WindowEventType Type;
    public readonly int ValueA;
    public readonly int ValueB;

    public WindowEvent(WindowEventType type, int valueA, int valueB)
    {
        Type = type;
        ValueA = valueA;
        ValueB = valueB;
    }

    public WindowEvent(WindowEventType type, int valueA)
    {
        Type = type;
        ValueA = valueA;
        ValueB = 0;
    }

    public WindowEvent(WindowEventType type)
    {
        Type = type;
        ValueA = 0;
        ValueB = 0;
    }
}
