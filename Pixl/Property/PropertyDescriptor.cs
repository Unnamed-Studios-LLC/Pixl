using Veldrid;

namespace Pixl;

public readonly struct PropertyDescriptor
{
    public readonly PropertyType Type;
    public readonly uint SizeInBytes;

    public PropertyDescriptor(PropertyType type, uint sizeInBytes)
    {
        Type = type;
        SizeInBytes = sizeInBytes;
    }

    public static PropertyDescriptor CreateCustom(uint sizeInBytes) => new(PropertyType.Custom, sizeInBytes);

    public static PropertyDescriptor CreateStandard(PropertyType type)
    {
        return type switch
        {
            PropertyType.Mat4 => new PropertyDescriptor(type, 64),
            PropertyType.Custom => throw new ArgumentException($"Use {nameof(CreateCustom)} method for Custom Properties"),
            _ => throw new ArgumentException($"{type} is not a supported {nameof(PropertyType)}", nameof(type)),
        };
    }

    internal BindableResource CreateBindableResource(ResourceFactory factory)
    {
        var bufferDescription = new BufferDescription(SizeInBytes, BufferUsage.UniformBuffer);
        return factory.CreateBuffer(bufferDescription);
    }
}
