using EntitiesDb;
using System.Collections.Concurrent;

namespace Pixl;

public sealed class TransformSystem : ComponentSystem
{
    private bool _matrixFlag;
    private readonly ConcurrentDictionary<uint, List<uint>> _hierarchy = new();
    private readonly Stack<List<uint>> _hierarchyCache = new();

    public TransformSystem()
    {
        Order = 900;
    }

    public IReadOnlyList<uint> GetChildren(uint entityId)
    {
        return _hierarchy.TryGetValue(entityId, out var children) ? children : Array.Empty<uint>();
    }

    public override void OnRegisterEvents()
    {
        RegisterEvent<Transform>(Event.OnAdd, OnAdd);
        RegisterEvent<Transform>(Event.OnRemove, OnRemove);
        RegisterEvent<CanvasTransform>(Event.OnAdd, OnAdd);
        RegisterEvent<CanvasTransform>(Event.OnRemove, OnRemove);
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        // parallel matrix multiplcation and hierarchy calculation
        _matrixFlag = !_matrixFlag;
        Scene.Entities.ParallelForEach((uint entityId, ref Transform transform) =>
        {
            ResolveHierarchy<Transform>(entityId, ref transform.ParentId, ref transform.SyncedParentId);
            Scene.Entities.UpdateWorldMatrix(ref transform, _matrixFlag);
        });

        Scene.Entities.ParallelForEach((uint entityId, ref CanvasTransform transform) =>
        {
            ResolveHierarchy<CanvasTransform>(entityId, ref transform.ParentId, ref transform.SyncedParentId);
        });
    }

    private void OnAdd(uint entityId, ref Transform transform)
    {
        transform.Flag = _matrixFlag;
        ResolveHierarchy<Transform>(entityId, ref transform.ParentId, ref transform.SyncedParentId);
    }

    private void OnAdd(uint entityId, ref CanvasTransform transform)
    {
        ResolveHierarchy<Transform>(entityId, ref transform.ParentId, ref transform.SyncedParentId);
    }

    private void OnRemove(uint entityId, ref Transform transform)
    {
        if (transform.SyncedParentId != 0 &&
            _hierarchy.TryGetValue(transform.SyncedParentId, out var parentsChildren))
        {
            parentsChildren.Remove(entityId);
        }

        if (!_hierarchy.TryRemove(entityId, out var children)) return;
        foreach (var child in children)
        {
            ref var childTransform = ref Scene.Entities.GetComponent<Transform>(child);
            if (childTransform.ParentId == childTransform.SyncedParentId) childTransform.ParentId = 0;
            childTransform.SyncedParentId = 0;
        }
        children.Clear();
        _hierarchyCache.Push(children);
    }

    private void OnRemove(uint entityId, ref CanvasTransform transform)
    {
        if (transform.SyncedParentId != 0 &&
            _hierarchy.TryGetValue(transform.SyncedParentId, out var parentsChildren))
        {
            parentsChildren.Remove(entityId);
        }

        if (!_hierarchy.TryRemove(entityId, out var children)) return;
        foreach (var child in children)
        {
            ref var childTransform = ref Scene.Entities.GetComponent<Transform>(child);
            if (childTransform.ParentId == childTransform.SyncedParentId) childTransform.ParentId = 0;
            childTransform.SyncedParentId = 0;
        }
        children.Clear();
        _hierarchyCache.Push(children);
    }

    private List<uint> RentList()
    {
        lock (_hierarchyCache)
        {
            return _hierarchyCache.TryPop(out var list) ? list : new();
        }
    }

    private void ReturnList(List<uint> list)
    {
        lock (_hierarchyCache)
        {
            list.Clear();
            _hierarchyCache.Push(list);
        }
    }

    private void ResolveHierarchy<T>(uint entityId, ref uint parentId, ref uint syncedParentId) where T : unmanaged
    {
        if (syncedParentId == parentId) return;
        if (!Scene.Entities.EntityExists(parentId))
        {
            parentId = 0;
            return;
        }

        ref var parentTransform = ref Scene.Entities.TryGetComponent<T>(parentId, out var found); // TODO change to HasComponent
        if (!found)
        {
            parentId = 0;
            return;
        }

        List<uint>? children;
        while (!_hierarchy.TryGetValue(entityId, out children))
        {
            children = RentList();
            if (!_hierarchy.TryAdd(entityId, children))
            {
                ReturnList(children);
            }
            else break;
        }

        lock (children)
        {
            children.Add(entityId);
        }

        syncedParentId = parentId;
    }
}
