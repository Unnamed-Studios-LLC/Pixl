namespace Pixl;

public struct Parent : IComponent
{
    public Entity Entity;

    internal uint ChildOf;
    internal int ChildIndex;
}
