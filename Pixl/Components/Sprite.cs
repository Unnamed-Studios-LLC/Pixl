namespace Pixl;

public struct Sprite : IComponent
{
    [ResourceType<Texture2d>]
    public ResourceView Texture;
    public RectInt Rect;
    public RectInt? CenterRect;
    public Color32 Color;
    public Vec2 Pivot;

    public Sprite(ResourceView texture, RectInt rect, Color32 color)
    {
        Texture = texture;
        Rect = rect;
        Color = color;
    }

    public static readonly Sprite Default = new Sprite(0, new RectInt(0, 0, 100, 100), Color32.White);
}