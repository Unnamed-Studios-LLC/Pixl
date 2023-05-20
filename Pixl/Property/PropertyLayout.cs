using Veldrid;

namespace Pixl;

public sealed class PropertyLayout
{
    public static readonly PropertyLayout Empty = new(Array.Empty<PropertySlot>(), Array.Empty<PropertyDescriptor>(), Array.Empty<SharedProperty>());

    public readonly PropertySlot[] Slots;
    public readonly PropertyDescriptor[] LocalProperties;
    public readonly SharedProperty[] SharedProperties;

    internal PropertyLayout(PropertySlot[] slots, PropertyDescriptor[] localProperties, SharedProperty[] sharedProperties)
    {
        Slots = slots;
        LocalProperties = localProperties;
        SharedProperties = sharedProperties;

        int local = 0, shared = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            ref var slot = ref slots[i];
            if (slot.Scope == PropertyScope.Shared) shared++;
            else if (slot.Scope == PropertyScope.Local) local++;
        }

        if (shared != SharedProperties.Length) throw new Exception("Shared properties do not match property slot array members");
        if (local != LocalProperties.Length) throw new Exception("Local properties do not match property slot array members");
    }

    internal PropertyResource[] CreateResources(ResourceFactory factory)
    {
        var shared = 0;
        var local = 0;
        var resources = new PropertyResource[Slots.Length];
        for (int i = 0; i < Slots.Length; i++)
        {
            ref var slot = ref Slots[i];
            var bindableResource = slot.Scope switch
            {
                PropertyScope.Shared => SharedProperties[shared++].BindableResource ?? throw new Exception($"BindableResource on SharedProperty ({SharedProperties[i - 1]}) is null"),
                _ => LocalProperties[local++].CreateBindableResource(factory)
            };
            resources[i] = new PropertyResource(slot.Scope, bindableResource);
        }
        return resources;
    }

    internal ResourceLayoutDescription CreateResourceLayoutDescription(ShaderStages shaderStage)
    {
        var elementDescriptions = new ResourceLayoutElementDescription[Slots.Length];
        for (int i = 0; i < Slots.Length; i++)
        {
            ref var slot = ref Slots[i];
            elementDescriptions[i] = new ResourceLayoutElementDescription(slot.Name, ResourceKind.UniformBuffer, shaderStage);
        }
        return new ResourceLayoutDescription(elementDescriptions);
    }
}
