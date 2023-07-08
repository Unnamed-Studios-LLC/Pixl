namespace Pixl;

public struct CanvasTransform : IComponent
{
    public Vec2 Position;
    public Vec3 Rotation;
    public Vec2 Scale;
    public Vec2 Size;
    public Vec2 Pivot;
    public Vec2 Anchor;
    public uint ParentId;
    internal uint SyncedParentId;

    public CanvasTransform(Vec2 position, Vec3 rotation, Vec2 scale, Vec2 size, Vec2 pivot, Vec2 anchor, uint parentId)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Size = size;
        Pivot = pivot;
        Anchor = anchor;
        ParentId = parentId;
    }

    public static CanvasTransform Default => new(Vec2.Zero, Vec3.Zero, Vec2.One, new Vec2(100, 100), Vec2.Half, Vec2.Half, 0);

    internal void GetTransformationMatrix(out Matrix4x4 matrix, in Vec2 parentSize)
    {
        matrix = Matrix4x4.Translate(Anchor * parentSize + Position - Pivot * Size * Scale);
        if (Rotation.X != 0)
        {
            matrix *= Matrix4x4.RotationX((float)Math.Sin(Rotation.X * Angle.Deg2Rad), (float)Math.Cos(Rotation.X * Angle.Deg2Rad));
        }
        if (Rotation.Y != 0)
        {
            matrix *= Matrix4x4.RotationY((float)Math.Sin(Rotation.Y * Angle.Deg2Rad), (float)Math.Cos(Rotation.Y * Angle.Deg2Rad));
        }
        if (Rotation.Z != 0)
        {
            matrix *= Matrix4x4.RotationZ((float)Math.Sin(Rotation.Z * Angle.Deg2Rad), (float)Math.Cos(Rotation.Z * Angle.Deg2Rad));
        }
        matrix *= Matrix4x4.Scale(new Vec3(Scale.X, Scale.Y, 1));
    }
}
