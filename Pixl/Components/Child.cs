using EntitiesDb;

namespace Pixl;

[Bufferable(8)]
public struct Child : IComponent
{
    public Entity Entity;

    public Child(Entity entity)
    {
        Entity = entity;
    }
}
