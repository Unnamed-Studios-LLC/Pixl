namespace Pixl;

public struct Sprite
{
    public int TextureId;
    public RectInt Rect;

    public Sprite(int textureId, RectInt rect)
    {
        TextureId = textureId;
        Rect = rect;
    }
}