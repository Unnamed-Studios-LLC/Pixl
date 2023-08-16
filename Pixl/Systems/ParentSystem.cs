using EntitiesDb;

namespace Pixl;

public sealed class ParentSystem : ComponentSystem
{
    public ParentSystem()
    {
        Order = 800;
    }

    public override void OnRegisterEvents()
    {
        RegisterEvent<Parent>(Event.OnRemove, OnRemoveParent);
    }

    public override void OnUpdate()
    {
        Entities.ForEach(Entities, static (uint entityId, ref Parent parent, ref EntityDatabase entities) =>
        {
            if (parent.Entity.Id == parent.ChildOf) return;

            // remove from linked children
            RemoveFromChildren(entities, entityId, ref parent);

            if (parent.Entity.Id == 0) return;

            if (parent.Entity.Id == entityId ||
                !entities.EntityExists(parent.Entity.Id))
            {
                parent.Entity.Id = 0;
                return;
            }

            // add to new children
            var children = entities.TryGetBuffer<Child>(parent.Entity.Id, out var found);
            if (!found)
            {
                return;
            }

            parent.ChildOf = parent.Entity.Id;
            parent.ChildIndex = children.Length;
            children.Add(new Child(entityId));
        });
    }

    private static void RemoveFromChildren(EntityDatabase entities, uint entityId, ref Parent parent)
    {
        if (parent.ChildOf == 0) return;

        var children = entities.GetBuffer<Child>(parent.ChildOf);
        children.RemoveAtSwapBack(parent.ChildIndex);
        if (parent.ChildIndex != children.Length)
        {
            // remap changed child
            var remappedChild = children[parent.ChildIndex];
            ref var remappedParent = ref entities.GetComponent<Parent>(remappedChild.Entity.Id);
            remappedParent.ChildIndex = parent.ChildIndex;
        }

        parent.ChildOf = 0;
        parent.ChildIndex = 0;
    }

    private void OnRemoveParent(uint entityId, ref Parent parent)
    {
        RemoveFromChildren(Entities, entityId, ref parent);
    }
}
