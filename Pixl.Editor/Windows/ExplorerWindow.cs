using ImGuiNET;
using System.Numerics;
using Veldrid;

namespace Pixl.Editor;

internal sealed class ExplorerWindow : EditorWindow
{
    private readonly Scene _scene;
    private readonly Resources _resources;
    private readonly Graphics _graphics;
    private readonly VertexRenderer _renderer;
    private readonly Property _worldToClipMatrix;
    private readonly Material _material;

    private Int2 _size;
    private Transform _cameraTransform = Transform.Default;
    private float _orthographicSize = 100;

    public ExplorerWindow(Scene scene, Resources resources, Graphics graphics)
    {
        _scene = scene;
        _resources = resources;
        _graphics = graphics;
        _worldToClipMatrix = resources.Default.WorldToClipMatrix;
        _material = resources.Default.DefaultMaterial;
        _renderer = new VertexRenderer(resources, ushort.MaxValue, 2_048_000);
        resources.Add(_renderer);
        Borderless = true;

        _cameraTransform.Position = new Vec3(0, 0, -10);
    }

    public RenderTexture? RenderTexture { get; private set; }
    public override string Name => "Explorer";

    protected override void OnUI()
    {
        SubmitExplorerWindow();
        if (RenderTexture?.Framebuffer == null) return;

        // render scene to texture
        var commands = _graphics.Commands;
        var framebuffer = RenderTexture.Framebuffer;

        commands.Begin();
        commands.SetFramebuffer(framebuffer);
        commands.ClearColorTarget(0, RgbaFloat.Grey);
        commands.End();
        _graphics.Submit(commands);

        _renderer.Begin(_graphics, _graphics.Commands, RenderTexture.Framebuffer);
        RenderScene();
        _renderer.End();

    }

    private Matrix4x4 GetWorldToClip()
    {
        var width = _orthographicSize * (_size.X / (float)_size.Y);
        var projectionMatrix = Matrix4x4.Orthographic(
            -width,
            width,
            -_orthographicSize,
            _orthographicSize,
            -0.03f,
            1000
        );

        Matrix4x4.View(in _cameraTransform.Position, in _cameraTransform.Rotation, in _cameraTransform.Scale, out var worldToClip);
        worldToClip = projectionMatrix * worldToClip;
        return worldToClip;
    }

    private void HandleInput()
    {
        var io = ImGui.GetIO();
        var worldToClip = GetWorldToClip();
        var clipToWorld = worldToClip.Inverse;
        if (!ImGui.IsItemHovered()) return;

        _orthographicSize *= 1 + io.MouseWheel * 0.1f;

        if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
        {
            var delta = io.MouseDelta.ToVec2();
            delta.Y *= -1; // ImGui Y coordinate is inverted

            Vec3 clipDelta = delta * (Vec2.One * 2 / _size);
            var worldDelta = clipToWorld.MultiplyVector(in clipDelta);
            _cameraTransform.Position -= worldDelta;
        }
    }

    private void SubmitExplorerWindow()
    {
        var contentMin = ImGui.GetWindowContentRegionMin();
        var contentMax = ImGui.GetWindowContentRegionMax();
        _size = (Int2)(contentMax - contentMin).ToVec2();

        UpdateRenderTexture();
        ImGui.Image((nint)RenderTexture!.Id, new Vector2(_size.X, _size.Y));
        HandleInput();
    }

    private void RenderScene()
    {
        var worldToClip = GetWorldToClip();
        _worldToClipMatrix?.Set(worldToClip);

        _renderer.BeginBatch(_material);
        _scene.Entities.ForEach((ref Sprite sprite, ref Transform transform) =>
        {
            _renderer.SetTexture(sprite.TextureId);

            // clockwise quad vertices
            Vec3 min = sprite.Rect.Min;
            Vec3 max = sprite.Rect.Max;

            // apply pivot
            var size = max - min;
            var pivotOffset = size * sprite.Pivot;
            min -= pivotOffset;
            max -= pivotOffset;

            var texture = _renderer.Texture;
            var spriteMin = sprite.Rect.Min * texture.TexelSize;
            var spriteMax = sprite.Rect.Max * texture.TexelSize;

            spriteMin.Y = 1 - spriteMin.Y;
            spriteMax.Y = 1 - spriteMax.Y;

            var a = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * min), spriteMin, sprite.Color);
            var b = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * new Vec3(min.X, max.Y, 0)), new Vec2(spriteMin.X, spriteMax.Y), sprite.Color);
            var c = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * max), spriteMax, sprite.Color);
            var d = new PositionTexColorVertex((Vec3)(transform.LocalToWorld * new Vec3(max.X, min.Y, 0)), new Vec2(spriteMax.X, spriteMin.Y), sprite.Color);

            _renderer.RenderQuad(in a, in b, in c, in d);
        });
        _renderer.EndBatch();
    }

    private Vec2 TransformWindowToWorld(Vec2 window)
    {
        var clip = (window / _size - 0.5f) * 2;
        var worldToClip = GetWorldToClip();
        throw new NotImplementedException();
    }

    private void UpdateRenderTexture()
    {
        var textureSize = Int2.Max(Int2.One, _size);
        if (RenderTexture == null)
        {
            RenderTexture = new RenderTexture(textureSize, SampleMode.Point, ColorFormat.Rgba32);
            _resources.Add(RenderTexture);
        }
        else
        {
            RenderTexture.Resize(textureSize);
        }
    }
}
