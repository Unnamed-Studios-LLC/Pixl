namespace Pixl;

public struct Sprite : IComponent
{
    [ResourceId<Texture2d>]
    public uint TextureId;
    public RectInt Rect;
    public RectInt? CenterRect;
    public Color32 Color;
    public Vec2 Pivot;

    public Sprite(uint textureId, RectInt rect, Color32 color)
    {
        TextureId = textureId;
        Rect = rect;
        Color = color;
    }

    public static readonly Sprite Default = new Sprite(0, new RectInt(0, 0, 100, 100), Color32.White);
}