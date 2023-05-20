namespace Pixl;

public readonly struct PropertySlot
{
    public readonly string Name;
    public readonly PropertyScope Scope;

    public PropertySlot(string name, PropertyScope scope)
    {
        Name = name;
        Scope = scope;
    }
}
