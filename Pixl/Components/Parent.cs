namespace Pixl;

public struct Parent : IComponent
{
    public Entity Entity;
    public RectInt Test;

    internal uint ChildOf;
    internal int ChildIndex;
}
