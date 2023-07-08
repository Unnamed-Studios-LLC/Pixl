namespace Pixl;

internal struct StoredValue
{
    public ValueType Type;
    public object Value;

    public StoredValue(ValueType type, object value)
    {
        Type = type;
        Value = value;
    }

    public bool TryGetFloat(out float value)
    {
        if (Type == ValueType.String)
        {
            value = default;
            return false;
        }

        value = (float)Value;
        return true;
    }

    public bool TryGetInt(out int value)
    {
        if (Type == ValueType.String)
        {
            value = default;
            return false;
        }

        if (Type == ValueType.Float)
        {
            value = (int)MathF.Floor((float)Value);
            return true;
        }

        value = (int)Value;
        return true;
    }

    public bool TryGetString(out string value)
    {
        value = Type != ValueType.String ? Value.ToString()! : (string)Value;
        return true;
    }
}
