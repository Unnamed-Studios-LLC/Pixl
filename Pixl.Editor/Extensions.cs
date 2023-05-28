using System.Numerics;

namespace Pixl.Editor;

internal static class Extensions
{
    public static Vec2 ToVec2(this Vector2 vector2) => new(vector2.X, vector2.Y);
}
