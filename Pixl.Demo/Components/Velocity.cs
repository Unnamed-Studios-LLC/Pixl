namespace Pixl.Demo.Components;

internal struct Velocity
{
    public Vec2 Position;
    public float Time;
    public Vec2 Vector;

    public Velocity(Vec2 vector) : this()
    {
        Vector = vector;
    }
}
