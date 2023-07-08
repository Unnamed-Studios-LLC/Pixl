using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Pixl.Editor;

internal sealed partial class Gui : GraphicsResource
{
    private readonly Material _material;
    private readonly Property _worldToClipMatrix;
    private readonly Resources _resources;
    private readonly Texture2d _nullTexture;

    private DeviceBuffer? _indexBuffer;
    private DeviceBuffer? _vertexBuffer;

    private nint? _context;
    private Texture2d? _fontTexture;
    private Texture2d _mainTexture;

    public Gui(Window window, Resources resources)
    {
        Window = window;
        _resources = resources;
        _material = resources.Default.GuiMaterial;
        _worldToClipMatrix = resources.Default.WorldToClipMatrix;
        _nullTexture = _mainTexture = resources.Default.NullTexture;
    }

    public Vec2 RenderScale { get; set; } = Vec2.One;
    public Window Window { get; }

    public void Render(Graphics graphics, CommandList commands, Framebuffer frameBuffer)
    {
        commands.Begin();
        commands.SetFramebuffer(frameBuffer);
        commands.ClearColorTarget(0, RgbaFloat.Black);
        RenderImGui(graphics, commands, frameBuffer);
        commands.End();
        graphics.Submit(commands);
        UpdateCursor();
    }

    public void Start(Resources resources)
    {
        _context ??= ImGui.CreateContext();

        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard |
            ImGuiConfigFlags.DockingEnable;
        io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

        CreateFontTexture(resources);
    }

    public void Stop(Resources resources)
    {
        if (_context != null) ImGui.DestroyContext(_context.Value);
        _context = null;

        if (_fontTexture != null)
        {
            resources.Remove(_fontTexture);
            _fontTexture = null;
        }
    }

    public void Update(float deltaTime, Span<WindowEvent> events)
    {
        UpdateImGui(deltaTime, events);
    }

    private unsafe void CreateFontTexture(Resources resources)
    {
        var io = ImGui.GetIO();

        Int2 size;
        io.Fonts.GetTexDataAsRGBA32(out nint pixels, out size.X, out size.Y, out var bytesPerPixel);

        var texture = new Texture2d(size, SampleMode.Point, ColorFormat.Rgba32, true);

        var pixelSpan = new Span<byte>(pixels.ToPointer(), bytesPerPixel * size.X * size.Y);
        var textureData = texture.GetData();
        pixelSpan.CopyTo(textureData);
        texture.ApplyInternal();
        resources.Add(texture);

        _fontTexture = texture;
        io.Fonts.SetTexID((nint)texture.Id);
        io.Fonts.ClearTexData();
    }

    private void RenderImGui(Graphics graphics, CommandList commandList, Framebuffer framebuffer)
    {
        ImGui.Render();

        var drawData = ImGui.GetDrawData();
        if (drawData.CmdListsCount == 0) return;

        ResizeBuffers(graphics, drawData);
        UpdateBuffers(commandList, drawData);

        commandList.SetFramebuffer(framebuffer);
        commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        commandList.SetVertexBuffer(0, _vertexBuffer);

        _mainTexture = _fontTexture ?? _nullTexture;
        _material.MainTextureProperty?.Set(_mainTexture);
        commandList.SetMaterial(_material, framebuffer, graphics);

        var io = ImGui.GetIO();
        var projectionMatrix = Matrix4x4.Orthographic(
            0,
            io.DisplaySize.X,
            io.DisplaySize.Y,
            0,
            -1, 1);

        _worldToClipMatrix.Set(projectionMatrix);
        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        var indexOffset = 0;
        var vertexOffset = 0;
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdListsRange[i];
            RenderImGuiDrawList(cmdList, commandList, graphics, framebuffer, indexOffset, vertexOffset);
            indexOffset += cmdList.IdxBuffer.Size;
            vertexOffset += cmdList.VtxBuffer.Size;
        }
    }

    private unsafe void RenderImGuiDrawList(ImDrawListPtr drawList, CommandList commandList, Graphics graphics, Framebuffer frameBuffer, int indexOffset, int vertexOffset)
    {
        for (int i = 0; i < drawList.CmdBuffer.Size; i++)
        {
            var drawCommand = drawList.CmdBuffer[i];
            if (drawCommand.UserCallback != IntPtr.Zero) continue; // not implemented, also i don't know what to implement :D

            // sync texture
            var textureId = drawCommand.TextureId.ToInt64();
            if (textureId != _mainTexture.Id)
            {
                if (_resources.TryGet(textureId, out var resource))
                {
                    // TODO better standardize texture targets in a ITexture interface with a TargetTexture property
                    if (resource is Texture2d resourceTexture)
                    {
                        _mainTexture = resourceTexture;
                    }
                    else if (resource is RenderTexture renderTexture)
                    {
                        _mainTexture = renderTexture.ColorTexture;
                    }
                }
                else
                {
                    _mainTexture = _nullTexture;
                }

                _material.MainTextureProperty?.Set(_mainTexture);
                commandList.SetMaterial(_material, frameBuffer, graphics);
            }

            commandList.SetScissorRect(0,
                (uint)drawCommand.ClipRect.X,
                (uint)drawCommand.ClipRect.Y,
                (uint)(drawCommand.ClipRect.Z - drawCommand.ClipRect.X),
                (uint)(drawCommand.ClipRect.W - drawCommand.ClipRect.Y)
            );

            commandList.DrawIndexed(
                drawCommand.ElemCount,
                1,
                drawCommand.IdxOffset + (uint)indexOffset,
                (int)drawCommand.VtxOffset + vertexOffset,
                0
            );
        }
    }

    private void ResizeBuffers(Graphics graphics, ImDrawDataPtr drawData)
    {
        var totalIndexSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
        var totalVertexSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
        var device = graphics.Device;

        if (_indexBuffer == null ||
            totalIndexSize > _indexBuffer.SizeInBytes)
        {
            _indexBuffer?.Dispose();
            var bufferDescription = new BufferDescription((uint)(totalIndexSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic);
            _indexBuffer = device.ResourceFactory.CreateBuffer(bufferDescription);
        }

        if (_vertexBuffer == null ||
            totalVertexSize > _vertexBuffer.SizeInBytes)
        {
            _vertexBuffer?.Dispose();
            var bufferDescription = new BufferDescription((uint)(totalVertexSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic);
            _vertexBuffer = device.ResourceFactory.CreateBuffer(bufferDescription);
        }
    }

    private unsafe void UpdateBuffers(CommandList commandList, ImDrawDataPtr drawData)
    {
        var indexOffset = 0;
        var vertexOffset = 0;

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var drawList = drawData.CmdListsRange[i];

            // update index buffer
            commandList.UpdateBuffer(
                _indexBuffer,
                (uint)indexOffset * sizeof(ushort),
                drawList.IdxBuffer.Data,
                (uint)(drawList.IdxBuffer.Size * sizeof(ushort))
            );

            // update vertex buffer
            commandList.UpdateBuffer(
                _vertexBuffer,
                (uint)(vertexOffset * Unsafe.SizeOf<ImDrawVert>()),
                drawList.VtxBuffer.Data,
                (uint)(drawList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>())
            );

            indexOffset += drawList.IdxBuffer.Size;
            vertexOffset += drawList.VtxBuffer.Size;
        }
    }

    private void UpdateCursor()
    {
        var imGuiCursor = ImGui.GetMouseCursor();
        var cursorState = imGuiCursor switch
        {
            ImGuiMouseCursor.Hand => CursorState.Hand,
            ImGuiMouseCursor.TextInput => CursorState.TextInput,
            ImGuiMouseCursor.ResizeAll or ImGuiMouseCursor.ResizeNESW or ImGuiMouseCursor.ResizeNWSE => CursorState.Resize,
            ImGuiMouseCursor.ResizeEW => CursorState.ResizeHorizontal,
            ImGuiMouseCursor.ResizeNS => CursorState.ResizeVertical,
            _ => CursorState.None
        };
        Window.CursorState = cursorState;
    }

    private void UpdateImGui(float deltaTime, Span<WindowEvent> events)
    {
        var windowSize = Window.Size;
        var mousePosition = Window.MousePosition;
        mousePosition.Y = windowSize.Y - mousePosition.Y - 1;

        // set frame variables
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(windowSize.X / RenderScale.X, windowSize.Y / RenderScale.Y);
        io.DisplayFramebufferScale = new Vector2(RenderScale.X, RenderScale.Y);
        io.DeltaTime = deltaTime;
        io.AddMousePosEvent(mousePosition.X, mousePosition.Y);

        // set input events
        ProcessInputEvents(events);

        ImGui.NewFrame();
    }
}
