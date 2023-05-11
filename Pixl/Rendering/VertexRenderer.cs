using System.Runtime.InteropServices;
using Veldrid;

namespace Pixl;

internal sealed class VertexRenderer<TVertex> : GraphicsResource where TVertex : unmanaged
{
    private CommandList? _commandList;
    private Framebuffer? _frameBuffer;
    private Material? _material;

    private readonly uint[] _indexBuffer;
    private readonly TVertex[] _vertexBuffer;
    private DeviceBuffer? _deviceIndexBuffer;
    private DeviceBuffer? _deviceVertexBuffer;
    private uint _indexCount;
    private uint _vertexCount;

    public VertexRenderer(int batchSize)
    {
        _indexBuffer = new uint[batchSize * 3];
        _vertexBuffer = new TVertex[batchSize * 3];
    }

    public void Begin(CommandList commandList, Framebuffer frameBuffer)
    {
        _commandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
        _frameBuffer = frameBuffer ?? throw new ArgumentNullException(nameof(frameBuffer));
    }

    public void BeginBatch(uint materialId)
    {
        if (_commandList == null) throw BeginNotCalledException();

        if (_material?.Id == materialId) return;

        // material changed
        var material = FetchMaterial(materialId);
        if (material != _material)
        {
            EndBatch();
            _material = material;
        }
    }

    public void EndBatch()
    {
        if (_indexCount == 0) return;
        if (_commandList == null || _frameBuffer == null) throw BeginNotCalledException();
        if (_material == null) throw new Exception("Render material is null");

        var (indexCount, vertexCount) = ConsumeCounts();

        var device = Game.Current.Graphics.Device;
        device.UpdateBuffer(_deviceIndexBuffer, 0, _indexBuffer.AsSpan(0, (int)indexCount));
        device.UpdateBuffer(_deviceVertexBuffer, 0, _vertexBuffer.AsSpan(0, (int)vertexCount));

        _commandList.Begin();
        _commandList.SetFramebuffer(_frameBuffer);
        _commandList.SetIndexBuffer(_deviceIndexBuffer, IndexFormat.UInt32);
        _commandList.SetVertexBuffer(0, _deviceVertexBuffer);
        _commandList.SetPipeline(_material.GetPipeline(_frameBuffer));
        _commandList.DrawIndexed(indexCount);
        _commandList.End();

        device.WaitForIdle();
        device.SubmitCommands(_commandList);
    }

    public unsafe void RenderQuad(in TVertex a, in TVertex b, in TVertex c, in TVertex d)
    {
        if (_commandList == null) throw BeginNotCalledException();
        if (!MakeSpace(6)) throw OutOfSpaceException();

        fixed (uint* indexPtr = &_indexBuffer[_indexCount])
        fixed (TVertex* vertexPtr = &_vertexBuffer[_vertexCount])
        {
            var index = indexPtr;
            var vertex = vertexPtr;

            *vertex++ = a;
            *vertex++ = b;
            *vertex++ = c;
            *vertex++ = d;

            *index++ = _vertexCount + 0;
            *index++ = _vertexCount + 1;
            *index++ = _vertexCount + 2;
            *index++ = _vertexCount + 0;
            *index++ = _vertexCount + 2;
            *index++ = _vertexCount + 3;
        }

        _vertexCount += 4;
        _indexCount += 6;
    }

    public unsafe void RenderTriangle(in TVertex a, in TVertex b, in TVertex c)
    {
        if (_commandList == null) throw BeginNotCalledException();
        if (!MakeSpace(3)) throw OutOfSpaceException();

        fixed (uint* indexPtr = &_indexBuffer[_indexCount])
        fixed (TVertex* vertexPtr = &_vertexBuffer[_vertexCount])
        {
            var index = indexPtr;
            var vertex = vertexPtr;

            *vertex++ = a;
            *index++ = _vertexCount++;
            *vertex++ = b;
            *index++ = _vertexCount++;
            *vertex++ = c;
            *index++ = _vertexCount++;
        }

        _vertexCount += 3;
        _indexCount += 3;
    }

    public void ClearDepth()
    {
        if (_commandList == null || _frameBuffer == null) throw BeginNotCalledException();
        EndBatch();

        _commandList.Begin();
        _commandList.SetFramebuffer(_frameBuffer);
        _commandList.ClearDepthStencil(0);
        _commandList.End();

        var device = Game.Current.Graphics.Device;
        device.SubmitCommands(_commandList);
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        var indexBufferDescription = new BufferDescription((uint)(_indexBuffer.Length * Marshal.SizeOf<uint>()), BufferUsage.IndexBuffer);
        var vertexBufferDescription = new BufferDescription((uint)(_vertexBuffer.Length * Marshal.SizeOf<TVertex>()), BufferUsage.VertexBuffer);

        var factory = Game.Current.Graphics.ResourceFactory;
        _deviceIndexBuffer = factory.CreateBuffer(indexBufferDescription);
        _deviceVertexBuffer = factory.CreateBuffer(vertexBufferDescription);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _deviceIndexBuffer?.Dispose();
        _deviceIndexBuffer = null;

        _deviceVertexBuffer?.Dispose();
        _deviceVertexBuffer = null;
    }

    private static Exception BeginNotCalledException() => new($"{nameof(VertexRenderer<TVertex>)}.{nameof(Begin)} must be called before Rendering.");

    private static Material FetchMaterial(uint id)
    {
        if (Game.Current.Resources.TryGet(id, out var resource) &&
            resource is Material material)
        {
            return material;
        }
        return Game.Current.Resources.ErrorMaterial;
    }

    private static Exception OutOfSpaceException() => new("Unable to render, not enough vertex space.");

    private (uint IndexCount, uint VertexCount) ConsumeCounts()
    {
        var counts = (_indexCount, _vertexCount);
        _indexCount = _vertexCount = 0;
        return counts;
    }

    private bool MakeSpace(int indexCount)
    {
        if (_indexCount + indexCount < _indexBuffer.Length) return true;
        EndBatch();
        return _indexCount + indexCount < _indexBuffer.Length;
    }
}
