using Veldrid;

namespace Pixl;

internal readonly struct PropertyResource : IDisposable
{
    public readonly PropertyScope Scope;
    public readonly BindableResource BindableResource;

    public PropertyResource(PropertyScope scope, BindableResource bindableResource)
    {
        Scope = scope;
        BindableResource = bindableResource;
    }

    public void Dispose()
    {
        if (BindableResource is IDisposable disposable) disposable.Dispose();
    }
}
