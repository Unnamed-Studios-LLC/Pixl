namespace Pixl;

public sealed class RenderingSystem : ComponentSystem
{
    public RenderingSystem()
    {
        Order = 1000;
    }

    internal override void OnRender(VertexRenderer<Vertex> renderer)
    {
        renderer.BeginBatch(Game.Current.Resources.DefaultMaterial.Id);

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

            cameraTransform.GetViewMatrix(out var viewToClip);
            viewToClip = projectionMatrix * viewToClip;

            Scene.Entities.ForEach((ref Sprite sprite, ref Transform transform) =>
            {
                transform.GetTransformationMatrix(out var modelToClip);
                modelToClip = viewToClip * modelToClip;

                // clockwise quad vertices
                Vec3 min = sprite.Rect.Min;
                Vec3 max = sprite.Rect.Max;

                var a = new Vertex((Vec3)(modelToClip * min), Color.Blue);
                var b = new Vertex((Vec3)(modelToClip * new Vec2(min.X, max.Y)), Color.Red);
                var c = new Vertex((Vec3)(modelToClip * max), Color.Green);
                var d = new Vertex((Vec3)(modelToClip * new Vec2(max.X, min.Y)), Color.Yellow);

                renderer.RenderQuad(in a, in b, in c, in d);
            });
        });
    }
}
