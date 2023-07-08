namespace Pixl;

public sealed class Shared<T> : Resource
{
    public Shared(T value)
    {
        Value = value;
    }

    public T Value { get; set; }
}
