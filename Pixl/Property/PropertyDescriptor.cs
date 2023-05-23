using Veldrid;

namespace Pixl;

public readonly struct PropertyDescriptor
{
    public readonly PropertyType Type;
    public readonly int SizeInBytes;

    public PropertyDescriptor(PropertyType type, int sizeInBytes)
    {
        Type = type;
        SizeInBytes = sizeInBytes;
    }

    public static unsafe PropertyDescriptor CreateCustom<T>() where T : unmanaged => new(PropertyType.Custom, sizeof(T));
    public static PropertyDescriptor CreateCustom(int sizeInBytes) => new(PropertyType.Custom, sizeInBytes);

    public static PropertyDescriptor CreateStandard(PropertyType type)
    {
        return type switch
        {
            PropertyType.Mat4 => new PropertyDescriptor(type, 64),
            PropertyType.Custom => throw new ArgumentException($"Use {nameof(CreateCustom)} method for Custom Properties"),
            _ => throw new ArgumentException($"{type} is not a supported {nameof(PropertyType)}", nameof(type)),
        };
    }

    public static PropertyDescriptor CreateTexture2d() => new(PropertyType.Texture2d, 0);
}
