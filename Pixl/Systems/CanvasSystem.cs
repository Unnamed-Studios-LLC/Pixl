namespace Pixl;

public sealed class CanvasSystem : ComponentSystem
{
    public Material? Material;
    public Property? WorldToClipMatrix;

    public CanvasSystem()
    {
        Order = 1001;

        var resources = Game.Current.DefaultResources;
        Material = resources.DefaultMaterial;
        WorldToClipMatrix = resources.WorldToClipMatrix;
    }

    public CanvasSystem(Material? material, Property? worldToClipMatrix) : this()
    {
        Material = material;
        WorldToClipMatrix = worldToClipMatrix;
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        renderer.ClearDepth();
        Scene.Entities.ForEach((ref Canvas canvas) =>
        {
            var windowSize = (Int2)(Window.Size / canvas.Scale);
            var remainder = new Int2(windowSize.X % 2, windowSize.Y % 2);
            var half = windowSize / 2;

            var projectionMatrix = Matrix4x4.Orthographic(
                0,
                windowSize.X,
                0,
                windowSize.Y,
                -1000, 1000);

            var parentSize = (Vec2)windowSize;
            var worldToClip = projectionMatrix * Matrix4x4.Scale(new Vec3(canvas.Scale.X, canvas.Scale.Y, 1)) * Matrix4x4.Translate(new Vec3(0, 0, -1));
            WorldToClipMatrix?.Set(worldToClip);

            var game = Game.Current;
            Texture2d texture = game.Graphics.NullTexture2d;
            renderer.BeginBatch(Material, texture);
            Scene.Entities.ForEach((ref Sprite sprite, ref CanvasTransform transform) =>
            {
                if (sprite.TextureId != texture.Id)
                {
                    if (!game.Resources.TryGet(sprite.TextureId, out var resource) ||
                        resource is not Texture2d spriteTexture)
                    {
                        spriteTexture = game.Graphics.NullTexture2d;
                    }

                    if (spriteTexture != texture)
                    {
                        texture = spriteTexture;
                        renderer.BeginBatch(Material, texture);
                    }
                }

                transform.GetTransformationMatrix(out var modelToWorld, in parentSize);

                // clockwise quad vertices
                Vec3 min = Vec3.Zero;
                Vec3 max = transform.Size;

                var spriteMin = sprite.Rect.Min * texture.TexelSize;
                var spriteMax = sprite.Rect.Max * texture.TexelSize;

                spriteMin.Y = 1 - spriteMin.Y;
                spriteMax.Y = 1 - spriteMax.Y;

                var a = new PositionTexColorVertex((Vec3)(modelToWorld * min), spriteMin, sprite.Color);
                var b = new PositionTexColorVertex((Vec3)(modelToWorld * new Vec2(min.X, max.Y)), new Vec2(spriteMin.X, spriteMax.Y), sprite.Color);
                var c = new PositionTexColorVertex((Vec3)(modelToWorld * max), spriteMax, sprite.Color);
                var d = new PositionTexColorVertex((Vec3)(modelToWorld * new Vec2(max.X, min.Y)), new Vec2(spriteMax.X, spriteMin.Y), sprite.Color);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
            renderer.EndBatch();
        });
    }
}
