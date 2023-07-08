namespace Pixl;

public struct Transform : IComponent
{
    [Range<float>(float.MinValue, float.MaxValue, 0.1f)]
    public Vec3 Position;
    [Range<float>(float.MinValue, float.MaxValue, 0.2f)]
    public Vec3 Rotation;
    [Range<float>(float.MinValue, float.MaxValue, 0.05f)]
    public Vec3 Scale;
    public Matrix4x4 LocalToWorld;

    public Transform(Vec3 position, Vec3 rotation, Vec3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Matrix4x4.Transformation(in position, in rotation, in scale, out LocalToWorld);
    }

    public static Transform Default => new(Vec3.Zero, Vec3.Zero, Vec3.One);
}
