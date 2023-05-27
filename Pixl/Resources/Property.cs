using Veldrid;

namespace Pixl;

public sealed class Property : GraphicsResource
{
    internal Property(string name, PropertyScope scope, PropertyDescriptor descriptor)
    {
        Name = name;
        Scope = scope;
        Descriptor = descriptor;
        (BackingType, BackingResource) = GetBacking(descriptor);
    }

    public string Name { get; }
    public PropertyScope Scope { get; }
    public PropertyDescriptor Descriptor { get; }

    internal GraphicsResource? BackingResource { get; private set; }
    internal PropertyBackingType BackingType { get; }

    private Texture2d? Texture2d => BackingResource as Texture2d;
    private Uniform? Uniform => BackingResource as Uniform;

    public unsafe void Set<T>(T value) where T : unmanaged => Set(ref value);
    public unsafe void Set<T>(ref T value) where T : unmanaged
    {
        AssertUniform();
        Uniform?.Set(value);
    }

    public void Set(Texture2d texture2d)
    {
        AssertTexture2d();
        BackingResource = texture2d ?? throw new ArgumentNullException(nameof(texture2d));
    }

    internal IEnumerable<BindableResource> GetBindableResources(Graphics graphics)
    {
        if (BackingResource == null) return Array.Empty<BindableResource>();
        return BackingResource switch
        {
            Texture2d texture2d => texture2d.GetBindableResources(graphics),
            Uniform uniform => uniform.GetBindableResources(),
            _ => throw new Exception($"{BackingResource.GetType()} is missing a {nameof(BindableResource)} switch case in {nameof(GetBindableResources)}"),
        };
    }

    internal ResourceLayoutElementDescription[] GetResourceLayoutElementDescriptions(ShaderStages stage)
    {
        return BackingType switch
        {
            PropertyBackingType.Texture2d => new[]
            {
                new ResourceLayoutElementDescription($"{Name}Texture", ResourceKind.TextureReadOnly, stage),
                new ResourceLayoutElementDescription($"{Name}Sampler", ResourceKind.Sampler, stage)
            },
            PropertyBackingType.Uniform => new[]
            {
                new ResourceLayoutElementDescription(Name, ResourceKind.UniformBuffer, stage)
            },
            _ => throw new Exception($"{BackingType} is missing a {nameof(ResourceLayoutElementDescription)} switch case in {nameof(GetResourceLayoutElementDescriptions)}"),
        };
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        switch (Descriptor.Type)
        {
            case PropertyType.Custom:
            case PropertyType.Mat4:
                Uniform?.Create(graphics);
                break;
        }
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        switch (Descriptor.Type)
        {
            case PropertyType.Custom:
            case PropertyType.Mat4:
                Uniform?.Destroy();
                break;
        }
    }

    private static (PropertyBackingType, GraphicsResource?) GetBacking(PropertyDescriptor descriptor)
    {
        return descriptor.Type switch
        {
            PropertyType.Texture2d => (PropertyBackingType.Texture2d, null),
            PropertyType.Custom or
            PropertyType.Mat4 => (PropertyBackingType.Uniform, new Uniform(descriptor.SizeInBytes)),
            _ => (PropertyBackingType.None, null)
        };
    }

    private void AssertTexture2d()
    {
        if (BackingType != PropertyBackingType.Texture2d) throw new Exception($"Property type of {Descriptor.Type} is not a {nameof(Pixl.Texture2d)} type");
    }

    private void AssertUniform()
    {
        if (BackingType != PropertyBackingType.Uniform) throw new Exception($"Property type of {Descriptor.Type} is not a {nameof(Pixl.Uniform)} type");
    }
}
