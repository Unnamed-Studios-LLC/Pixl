using EntitiesDb;

namespace Pixl;

public sealed class CameraSystem : ComponentSystem
{
    public class RenderState
    {
        public Material? Material;
        public Property? WorldToClipMatrix;
        public EntityDatabase? Entities;
        internal VertexRenderer? Renderer;
    }

    public Color32 ClearColor = Color32.Black;
    public Material? Material;
    public Property? WorldToClipMatrix;

    private readonly RenderState _renderState = new();

    public CameraSystem()
    {
        Order = 1000;

        var defaultResources = Game.Shared.Resources.Default;
        Material = defaultResources.DefaultMaterial;
        WorldToClipMatrix = defaultResources.WorldToClipMatrix;
    }

    public CameraSystem(Material? material, Property? worldToClipMatrix) : this()
    {
        Material = material;
        WorldToClipMatrix = worldToClipMatrix;
    }

    internal override void OnRender(VertexRenderer renderer)
    {
        _renderState.Renderer = renderer;
        _renderState.Entities = Entities;

        renderer.Clear(ClearColor);
        Entities.ForEach(_renderState, static (ref Camera camera, ref Transform cameraTransform, ref RenderState state) =>
        {
            var renderer = state.Renderer!;
            var entitites = state.Entities!;
            if (camera.ClearDepth) renderer.ClearDepth();

            var windowSize = Screen.Size;
            var remainder = new Int2(windowSize.X % 2, windowSize.Y % 2);
            var half = windowSize / 2;

            var projectionMatrix = Matrix4x4.Orthographic(
                -half.X,
                half.X + remainder.X,
                -half.Y,
                half.Y + remainder.Y,
                -1000 * cameraTransform.Scale.Z, 1000 * cameraTransform.Scale.Z);

            Matrix4x4.View(in cameraTransform.Position, in cameraTransform.Rotation, in cameraTransform.Scale, out var worldToClip);
            worldToClip = projectionMatrix * worldToClip;
            state.WorldToClipMatrix?.Set(worldToClip);

            renderer.BeginBatch(state.Material);
            entitites.ForEach(state, static (ref Sprite sprite, ref Transform transform, ref RenderState state) =>
            {
                var renderer = state.Renderer!;
                renderer.SetTexture(sprite.Texture.Id);

                // clockwise quad vertices
                Vec3 min = sprite.Rect.Min;
                Vec3 max = sprite.Rect.Max;

                // apply pivot
                var size = max - min;
                var pivotOffset = size * sprite.Pivot;
                min -= pivotOffset;
                max -= pivotOffset;

                var texture = renderer.Texture;
                var spriteMin = sprite.Rect.Min * texture.TexelSize;
                var spriteMax = sprite.Rect.Max * texture.TexelSize;

                spriteMin.Y = 1 - spriteMin.Y;
                spriteMax.Y = 1 - spriteMax.Y;
                
                var a = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * min), spriteMin, sprite.Color);
                var b = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * new Vec2(min.X, max.Y)), new Vec2(spriteMin.X, spriteMax.Y), sprite.Color);
                var c = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * max), spriteMax, sprite.Color);
                var d = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * new Vec2(max.X, min.Y)), new Vec2(spriteMax.X, spriteMin.Y), sprite.Color);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
            renderer.EndBatch();
        });
    }
}
