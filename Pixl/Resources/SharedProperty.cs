using Veldrid;

namespace Pixl;

public sealed class SharedProperty : GraphicsResource
{
    private readonly PropertyDescriptor _property;
    private readonly byte[] _data;

    internal SharedProperty(PropertyDescriptor uniform)
    {
        _property = uniform;
        _data = new byte[uniform.SizeInBytes];
    }

    internal BindableResource? BindableResource { get; private set; }

    public static SharedProperty Create(PropertyDescriptor uniform) => new(uniform);

    public unsafe void Set<T>(T value) where T : unmanaged
    {
        var size = sizeof(T);
        if (size > _property.SizeInBytes) throw new ArgumentException($"Size of {typeof(T)} too large for the Uniform", nameof(value));
        fixed (byte* ptr = _data)
        {
            *(T*)ptr = value;
        }

        if (Graphics == null) return;
        if (BindableResource is DeviceBuffer deviceBuffer)
        {
            Graphics.Device.UpdateBuffer(deviceBuffer, 0, value);
        }
    }

    internal override void OnCreate(Graphics graphics)
    {
        var factory = graphics.ResourceFactory;
        BindableResource = _property.CreateBindableResource(factory);
        if (BindableResource is DeviceBuffer deviceBuffer)
        {
            graphics.Device.UpdateBuffer(deviceBuffer, 0, _data);
        }
    }

    internal override void OnDestroy(Graphics graphics)
    {
        if (BindableResource is IDisposable disposable)
        {
            disposable.Dispose();
        }
        BindableResource = null;
    }
}
