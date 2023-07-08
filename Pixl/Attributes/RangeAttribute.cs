namespace Pixl;

[AttributeUsage(AttributeTargets.Field)]
public sealed class RangeAttribute<T> : Attribute
{
    public RangeAttribute(T min, T max, T speed)
    {
        Min = min;
        Max = max;
        Speed = speed;
    }

    public T Min { get; set; }
    public T Max { get; }
    public T Speed { get; }
}
