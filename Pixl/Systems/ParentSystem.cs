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
        RegisterComponentEvent<Parent>(Event.OnRemove, OnRemoveParent);
    }

    public override void OnUpdate()
    {
        Entities.ForEach(Entities, static (uint entityId, ref Parent parent, EntityDatabase entities) =>
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
            ref var newChildren = ref entities.TryGetComponent<Children>(parent.Entity.Id, out var found);
            if (!found)
            {
                return;
                entities.AddComponent<Children>(parent.Entity.Id);
                newChildren = ref entities.GetComponent<Children>(parent.Entity.Id);
            }

            parent.ChildOf = parent.Entity.Id;
            parent.ChildIndex = newChildren.EntityIds.Length;
            newChildren.EntityIds.Add(entityId);
        });
    }

    private static void RemoveFromChildren(EntityDatabase entities, uint entityId, ref Parent parent)
    {
        if (parent.ChildOf == 0) return;

        ref var children = ref entities.GetComponent<Children>(parent.ChildOf);
        children.EntityIds.RemoveAtSwapBack(parent.ChildIndex);
        if (parent.ChildIndex != children.EntityIds.Length)
        {
            // remap changed child
            var remappedEntityId = children.EntityIds[parent.ChildIndex];
            ref var remappedParent = ref entities.GetComponent<Parent>(remappedEntityId);
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
