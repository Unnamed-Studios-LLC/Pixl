namespace Pixl;

public unsafe struct Name : IComponent, ISerializable
{
    public const int MaxLength = 24;

    private readonly byte _length;
    private fixed char _buffer[MaxLength];

    public Name(int length)
    {
        if (length < 0 || length >= MaxLength) throw new ArgumentOutOfRangeException(nameof(length));
        _length = (byte)length;
    }

    public Name(ReadOnlySpan<char> name)
    {
        _length = (byte)Math.Min(name.Length, MaxLength);
        name = name[.._length];
        fixed (char* bufferPointer = _buffer)
        {
            var destination = new Span<char>(bufferPointer, MaxLength);
            name.CopyTo(destination);
        }
    }

    public char this[int index]
    {
        get
        {
            if (index < 0 || index >= _length) throw new ArgumentOutOfRangeException(nameof(index));
            return _buffer[index];
        }
    }

    public static implicit operator Name(ReadOnlySpan<char> span) => new(span);
    public static implicit operator Name(Span<char> span) => new(span);

    public int Length => _length;

    public static Name CreateNameWithId(ReadOnlySpan<char> prefix, uint entityId)
    {
        var idLength = (int)Math.Floor(Math.Log10(entityId) + 1);
        var name = new Name(prefix.Length + idLength);
        var nameSpan = name.AsSpan();

        // copy prefix
        prefix.CopyTo(nameSpan);

        // copy id digits
        var idValue = entityId;
        for (int i = idLength - 1; i >= 0; i--)
        {
            var digitValue = idValue % 10;
            idValue /= 10;
            var digit = (char)('0' + digitValue);
            nameSpan[prefix.Length + i] = digit;
        }

        return name;
    }

    public Span<char> AsSpan()
    {
        fixed (char* bufferPointer = _buffer)
        {
            var destination = new Span<char>(bufferPointer, _length);
            return destination;
        }
    }

    public void Serialize(ref Node node)
    {
        var stringValue = AsSpan().ToString();
        node.Value("Value", stringValue);
    }
}
