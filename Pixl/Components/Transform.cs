namespace Pixl;

public struct Transform
{
    public Vec3 Position;
    public Vec3 Rotation;
    public Vec3 Scale;

    public Transform(Vec3 position, Vec3 rotation, Vec3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    public static Transform Default => new(Vec3.Zero, Vec3.Zero, Vec3.One);

    internal void GetTransformationMatrix(out Matrix4x4 matrix)
    {
        matrix = Matrix4x4.Translate(in Position);
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
        matrix *= Matrix4x4.Scale(in Scale);
    }

    internal void GetViewMatrix(out Matrix4x4 matrix)
    {
        matrix = Matrix4x4.Scale(in Scale) * Matrix4x4.Translate(Position * -1);
    }
}
