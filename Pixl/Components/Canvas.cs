namespace Pixl;

public struct Canvas
{
    public Vec2 Scale;

    public Canvas(Vec2 scale)
    {
        Scale = scale;
    }

    public static Canvas Default => new(Vec2.One);
}
