using Veldrid;

namespace Pixl;

internal sealed class Uniform : GraphicsResource
{
    private readonly byte[] _data;

    internal Uniform(int sizeInBytes)
    {
        SizeInBytes = sizeInBytes;
        _data = new byte[sizeInBytes];
    }

    public DeviceBuffer? Buffer { get; private set; }
    public int SizeInBytes { get; }

    public unsafe void Set<T>(T value) where T : unmanaged
    {
        var size = sizeof(T);
        if (size > SizeInBytes) throw new ArgumentException($"Size of {typeof(T)} is too large for the Uniform", nameof(value));
        fixed (byte* ptr = _data)
        {
            *(T*)ptr = value;
        }

        if (Graphics == null || Buffer == null) return;
        Graphics.Device.UpdateBuffer(Buffer, 0, value);
    }

    internal IEnumerable<BindableResource> GetBindableResources()
    {
        if (Buffer == null) throw new Exception($"{nameof(Uniform)} {nameof(Buffer)} is null!");
        yield return Buffer;
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var factory = graphics.ResourceFactory;
        var bufferDescription = new BufferDescription((uint)SizeInBytes, BufferUsage.UniformBuffer);
        Buffer = factory.CreateBuffer(bufferDescription);
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        Buffer?.Dispose();
        Buffer = null;
    }
}
