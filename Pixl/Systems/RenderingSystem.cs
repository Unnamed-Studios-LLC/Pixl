﻿using EntitiesDb;

namespace Pixl;

public sealed class RenderingSystem : ComponentSystem
{
    public Material? Material;
    public Property? WorldToClipMatrix;

    public RenderingSystem()
    {
        Order = 1000;

        var resources = Game.Current.DefaultResources;
        Material = resources.DefaultMaterial;
        WorldToClipMatrix = resources.WorldToClipMatrix;
    }

    public RenderingSystem(Material? material, Property? worldToClipMatrix) : this()
    {
        Material = material;
        WorldToClipMatrix = worldToClipMatrix;
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        Scene.Entities.ForEach((ref Camera camera, ref Transform cameraTransform) =>
        {
            var windowSize = Window.Size;
            var remainder = new Int2(windowSize.X % 2, windowSize.Y % 2);
            var half = windowSize / 2;

            var projectionMatrix = Matrix4x4.Orthographic(
                -half.X,
                half.X + remainder.X,
                -half.Y,
                half.Y + remainder.Y,
                -1000 * cameraTransform.Scale.Z, 1000 * cameraTransform.Scale.Z);

            cameraTransform.GetViewMatrix(out var worldToClip);
            worldToClip = projectionMatrix * worldToClip;
            WorldToClipMatrix?.Set(worldToClip);

            var game = Game.Current;
            renderer.BeginBatch(Material);
            Scene.Entities.ForEach((ref Sprite sprite, ref Transform transform) =>
            {
                renderer.SetTexture(sprite.TextureId);

                // clockwise quad vertices
                Vec3 min = sprite.Rect.Min;
                Vec3 max = sprite.Rect.Max;

                var texture = renderer.Texture;
                var spriteMin = sprite.Rect.Min * texture.TexelSize;
                var spriteMax = sprite.Rect.Max * texture.TexelSize;

                spriteMin.Y = 1 - spriteMin.Y;
                spriteMax.Y = 1 - spriteMax.Y;
                
                var a = new PositionTexColorVertex((Vec3)(transform.WorldMatrix * min), spriteMin, sprite.Color);
                var b = new PositionTexColorVertex((Vec3)(transform.WorldMatrix * new Vec2(min.X, max.Y)), new Vec2(spriteMin.X, spriteMax.Y), sprite.Color);
                var c = new PositionTexColorVertex((Vec3)(transform.WorldMatrix * max), spriteMax, sprite.Color);
                var d = new PositionTexColorVertex((Vec3)(transform.WorldMatrix * new Vec2(max.X, min.Y)), new Vec2(spriteMax.X, spriteMin.Y), sprite.Color);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
            renderer.EndBatch();
        });
    }
}
