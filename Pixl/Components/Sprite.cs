namespace Pixl;

public struct Sprite
{
    public uint TextureId;
    public RectInt Rect;
    public RectInt? CenterRect;
    public Color32 Color;

    public Sprite(uint textureId, RectInt rect, Color32 color)
    {
        TextureId = textureId;
        Rect = rect;
        Color = color;
    }
}