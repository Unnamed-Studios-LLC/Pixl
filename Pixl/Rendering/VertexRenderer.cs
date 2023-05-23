using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace Pixl;

internal sealed class VertexRenderer : GraphicsResource
{
    private Graphics? _graphics;
    private CommandList? _commandList;
    private Framebuffer? _frameBuffer;
    private Material? _material;
    private Texture2d? _texture;

    private readonly ushort[] _indexBuffer;
    private readonly byte[] _vertexBuffer;
    private DeviceBuffer? _deviceIndexBuffer;
    private DeviceBuffer? _deviceVertexBuffer;
    private ushort _indexCount;
    private ushort _vertexCount;
    private uint _stride;

    public VertexRenderer(int indexBatchSize, int vertexBatchByteSize)
    {
        _indexBuffer = new ushort[indexBatchSize * 3];
        _vertexBuffer = new byte[vertexBatchByteSize];
    }

    public void Begin(Graphics graphics, CommandList commandList, Framebuffer frameBuffer)
    {
        _graphics = graphics;
        _commandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
        _frameBuffer = frameBuffer ?? throw new ArgumentNullException(nameof(frameBuffer));
    }

    public void BeginBatch(Material? material, Texture2d? texture)
    {
        if (_graphics == null) throw BeginNotCalledException();
        if (_material == material &&
            _texture == texture) return;

        EndBatch();

        // material changed
        var game = Game.Current;
        _material = material ?? game.DefaultResources.ErrorMaterial;
        _texture = texture ?? game.Graphics.NullTexture2d;
        _stride = _material.VertexStride;
    }

    public void ClearDepth()
    {
        if (_graphics == null || _commandList == null || _frameBuffer == null) throw BeginNotCalledException();
        EndBatch();

        _commandList.Begin();
        _commandList.SetFramebuffer(_frameBuffer);
        _commandList.ClearDepthStencil(0);
        _commandList.End();

        _graphics.Submit(_commandList);
    }

    public void End()
    {
        EndBatch();
    }

    public void EndBatch()
    {
        if (_indexCount == 0) return;
        if (_graphics == null || _commandList == null || _frameBuffer == null) throw BeginNotCalledException();
        if (_material == null) throw new Exception("Render material is null");
        if (_texture == null) throw new Exception("Render texture is null");

        _material.MainTextureProperty?.Set(_texture);

        var (indexCount, vertexCount) = ConsumeCounts();
        var vertexSize = vertexCount * _stride;

        _commandList.Begin();
        _commandList.SetFramebuffer(_frameBuffer);
        _commandList.SetIndexBuffer(_deviceIndexBuffer, IndexFormat.UInt16);
        _commandList.SetVertexBuffer(0, _deviceVertexBuffer);
        _commandList.SetPipeline(_material.CreatePipeline(_graphics, _frameBuffer));

        uint slot = 0;
        foreach (var resourceSet in _material.CreateResourceSets(_graphics))
        {
            _commandList.SetGraphicsResourceSet(slot++, resourceSet);
        }

        _commandList.DrawIndexed(indexCount);
        _commandList.End();

        var device = _graphics.Device;
        device.WaitForIdle();
        device.UpdateBuffer(_deviceIndexBuffer, 0, _indexBuffer.AsSpan(0, (int)indexCount));
        device.UpdateBuffer(_deviceVertexBuffer, 0, _vertexBuffer.AsSpan(0, (int)vertexSize));
        device.SubmitCommands(_commandList);
    }

    public unsafe void RenderQuad<TVertex>(in TVertex a, in TVertex b, in TVertex c, in TVertex d) where TVertex : unmanaged
    {
        if (_commandList == null) throw BeginNotCalledException();
        if (_material == null) throw BeginBatchNotCalledException();
        if (!MakeSpace(6, 4, sizeof(TVertex))) throw OutOfSpaceException();

        fixed (ushort* indexPtr = &_indexBuffer[_indexCount])
        fixed (byte* vertexPtr = &_vertexBuffer[_vertexCount * _stride])
        {
            var index = indexPtr;
            var vertex = vertexPtr;

            *(TVertex*)(vertex + _stride * 0) = a;
            *(TVertex*)(vertex + _stride * 1) = b;
            *(TVertex*)(vertex + _stride * 2) = c;
            *(TVertex*)(vertex + _stride * 3) = d;

            *index++ = (ushort)(_vertexCount + 0);
            *index++ = (ushort)(_vertexCount + 1);
            *index++ = (ushort)(_vertexCount + 2);
            *index++ = (ushort)(_vertexCount + 0);
            *index++ = (ushort)(_vertexCount + 2);
            *index++ = (ushort)(_vertexCount + 3);
        }

        _vertexCount += 4;
        _indexCount += 6;
    }

    public unsafe void RenderTriangle<TVertex>(in TVertex a, in TVertex b, in TVertex c) where TVertex : unmanaged
    {
        if (_commandList == null) throw BeginNotCalledException();
        if (_material == null) throw BeginBatchNotCalledException();
        if (!MakeSpace(3, 3, sizeof(TVertex))) throw OutOfSpaceException();

        fixed (ushort* indexPtr = &_indexBuffer[_indexCount])
        fixed (byte* vertexPtr = &_vertexBuffer[_vertexCount])
        {
            var index = indexPtr;
            var vertex = vertexPtr;

            *(TVertex*)(vertex + _stride * 0) = a;
            *(TVertex*)(vertex + _stride * 1) = b;
            *(TVertex*)(vertex + _stride * 2) = c;

            *index++ = _vertexCount++;
            *index++ = _vertexCount++;
            *index++ = _vertexCount++;
        }

        _vertexCount += 3;
        _indexCount += 3;
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var indexBufferDescription = new BufferDescription((uint)(_indexBuffer.Length * Marshal.SizeOf<ushort>()), BufferUsage.IndexBuffer | BufferUsage.Dynamic);
        var vertexBufferDescription = new BufferDescription((uint)_vertexBuffer.Length, BufferUsage.VertexBuffer | BufferUsage.Dynamic);

        var factory = graphics.ResourceFactory;
        _deviceIndexBuffer = factory.CreateBuffer(indexBufferDescription);
        _deviceVertexBuffer = factory.CreateBuffer(vertexBufferDescription);
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        _deviceIndexBuffer?.Dispose();
        _deviceIndexBuffer = null;

        _deviceVertexBuffer?.Dispose();
        _deviceVertexBuffer = null;
    }

    private static Exception BeginNotCalledException() => new($"{nameof(VertexRenderer)}.{nameof(Begin)} must be called before Rendering.");
    private static Exception BeginBatchNotCalledException() => new($"{nameof(VertexRenderer)}.{nameof(BeginBatch)} must be called before Rendering.");

    private static Exception OutOfSpaceException() => new("Unable to render, not enough vertex space.");

    private (uint IndexCount, uint VertexCount) ConsumeCounts()
    {
        var counts = (_indexCount, _vertexCount);
        _indexCount = _vertexCount = 0;
        return counts;
    }

    private bool HasSpace(int indexCount, int vertexCount, int vertexSize)
    {
        var vertexOverflow = (uint)Math.Max(0, vertexSize - _stride);
        return _indexCount + indexCount < _indexBuffer.Length &&
            (_vertexCount + vertexCount) * _stride + vertexOverflow < _vertexBuffer.Length;
    }

    private bool MakeSpace(int indexCount, int vertexCount, int vertexSize)
    {
        if (HasSpace(indexCount, vertexCount, vertexSize)) return true;
        EndBatch();
        return HasSpace(indexCount, vertexCount, vertexSize);
    }
}
