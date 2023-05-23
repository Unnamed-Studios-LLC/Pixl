using ImGuiNET;
using System;
using System.Numerics;
using Veldrid;
using Vulkan;

namespace Pixl.Editor;

internal sealed class Gui : GraphicsResource
{
    private readonly Material _material;

    private nint? _context;
    private CommandList? _commandList;
    private DeviceBuffer? _vertexBuffer;
    private DeviceBuffer? _indexBuffer;

    public Gui(AppWindow window, Material material)
    {
        Window = window;
        _material = material;
    }

    public AppWindow Window { get; }

    public void Update(float deltaTime, Span<WindowEvent> events)
    {
        UpdateImGui(deltaTime, events);
    }

    public void Render(Graphics graphics, CommandList commands, Framebuffer frameBuffer)
    {
        commands.Begin();
        commands.SetFramebuffer(frameBuffer);
        commands.ClearColorTarget(0, RgbaFloat.Black);

        commands.SetFramebuffer(frameBuffer);
        commands.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
        commands.SetVertexBuffer(0, _vertexBuffer);
        commands.SetPipeline(_material.CreatePipeline(graphics, frameBuffer));

        uint slot = 0;
        foreach (var resourceSet in _material.CreateResourceSets(graphics))
        {
            commands.SetGraphicsResourceSet(slot++, resourceSet);
        }

        commands.End();
        graphics.Submit(commands);
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        _context = ImGui.CreateContext();

        var factory = graphics.ResourceFactory;
        _commandList = factory.CreateCommandList();
        _commandList.Name = "Pixl Gui Command List";

        _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        _vertexBuffer.Name = "Pixl Gui Vertex Buffer";
        _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        _indexBuffer.Name = "Pixl Gui Index Buffer";
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        if (_context != null) ImGui.DestroyContext(_context.Value);
        _context = null;

        _commandList?.Dispose();
        _commandList = null;

        _vertexBuffer?.Dispose();
        _vertexBuffer = null;
        _indexBuffer?.Dispose();
        _indexBuffer = null;
    }

    private void UpdateImGui(float deltaTime, Span<WindowEvent> events)
    {
        var mousePosition = Window.MousePosition;
        var windowSize = Window.Size;

        // set frame variables
        var io = ImGui.GetIO();
        io.DisplaySize = new Vector2(windowSize.X, windowSize.Y);
        io.DisplayFramebufferScale = Vector2.One;
        io.DeltaTime = deltaTime;
        io.AddMousePosEvent(mousePosition.X, mousePosition.Y);

        // set input events
        foreach (ref var @event in events)
        {

        }
    }
}
