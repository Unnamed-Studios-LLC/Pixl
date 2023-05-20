using EntitiesDb;

namespace Pixl;

public static class EntitiesTransform
{
    public static void GetWorldMatrix(this EntityDatabase entityDatabase, ref Transform transform, out Matrix4x4 matrix)
    {
        transform.GetTransformationMatrix(out matrix);
        if (transform.ParentId == 0 ||
            !entityDatabase.EntityExists(transform.ParentId)) return;

        ref var parentTransform = ref entityDatabase.TryGetComponent<Transform>(transform.ParentId, out var found);
        if (!found) return;
        entityDatabase.GetWorldMatrix(ref parentTransform, out var parentMatrix);
        matrix = parentMatrix * matrix;
    }

    internal static void UpdateWorldMatrix(this EntityDatabase entityDatabase, ref Transform transform, bool flag)
    {
        if (transform.Flag == flag) return;

        transform.Flag = flag;
        transform.GetTransformationMatrix(out transform.WorldMatrix);
        if (transform.ParentId != 0 &&
            entityDatabase.EntityExists(transform.ParentId))
        {
            ref var parentTransform = ref entityDatabase.TryGetComponent<Transform>(transform.ParentId, out var found);
            if (!found) return;
            entityDatabase.UpdateWorldMatrix(ref parentTransform, flag);
            transform.WorldMatrix = parentTransform.WorldMatrix * transform.WorldMatrix;
        }
    }
}
