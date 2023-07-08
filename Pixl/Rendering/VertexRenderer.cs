using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace Pixl;

internal sealed class VertexRenderer : GraphicsResource
{
    private readonly Resources _resources;
    private readonly Texture2d _nullTexture;
    private readonly Material _errorMaterial;
    private readonly ushort[] _indexBuffer;
    private readonly byte[] _vertexBuffer;

    private Graphics? _graphics;
    private CommandList? _commandList;
    private Framebuffer? _frameBuffer;
    private Material? _material;

    private DeviceBuffer? _deviceIndexBuffer;
    private DeviceBuffer? _deviceVertexBuffer;
    private ushort _indexCount;
    private ushort _vertexCount;
    private uint _stride;

    public VertexRenderer(Resources resources, ushort indexBatchSize, int vertexBatchByteSize)
    {
        _resources = resources;
        Texture = _nullTexture = resources.Default.NullTexture;
        _errorMaterial = resources.Default.ErrorMaterial;
        _indexBuffer = new ushort[indexBatchSize];
        _vertexBuffer = new byte[vertexBatchByteSize];
    }

    public int BatchCount { get; private set; }
    public Texture2d Texture { get; private set; }
    public RectInt? ClipRect { get; private set; }

    public void Begin(Graphics graphics, CommandList commandList, Framebuffer frameBuffer)
    {
        BatchCount = 0;
        _graphics = graphics;
        _commandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
        _frameBuffer = frameBuffer ?? throw new ArgumentNullException(nameof(frameBuffer));
    }

    public void BeginBatch(Material? material)
    {
        if (_graphics == null) throw BeginNotCalledException();
        if (_material == material) return;

        EndBatch();

        // material changed
        _material = material ?? _errorMaterial;
        _stride = _material.VertexStride;
        Texture = _nullTexture;
        ClipRect = null;
    }

    public void Clear(Color32 color)
    {
        if (_graphics == null || _commandList == null || _frameBuffer == null) throw BeginNotCalledException();
        EndBatch();

        _commandList.Begin();
        _commandList.SetFramebuffer(_frameBuffer);
        _commandList.ClearColorTarget(0, new RgbaFloat(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));
        _commandList.ClearDepthStencil(0);
        _commandList.End();

        _graphics.Submit(_commandList);
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
        if (Texture == null) throw new Exception("Render texture is null");

        _material.MainTextureProperty?.Set(Texture);

        var (indexCount, vertexCount) = ConsumeCounts();
        var vertexSize = vertexCount * _stride;

        _commandList.Begin();
        _commandList.SetFramebuffer(_frameBuffer);
        _commandList.UpdateBuffer(_deviceIndexBuffer, 0, _indexBuffer.AsSpan(0, (int)indexCount));
        _commandList.UpdateBuffer(_deviceVertexBuffer, 0, _vertexBuffer.AsSpan(0, (int)vertexSize));
        _commandList.SetIndexBuffer(_deviceIndexBuffer, IndexFormat.UInt16);
        _commandList.SetVertexBuffer(0, _deviceVertexBuffer);
        _commandList.SetMaterial(_material, _frameBuffer, _graphics);

        if (ClipRect != null)
        {
            var clipRect = ClipRect.Value;
            _commandList.SetScissorRect(0, (uint)clipRect.X, (uint)clipRect.Y, (uint)clipRect.Width, (uint)clipRect.Height);
        }
        else
        {
            _commandList.SetFullScissorRect(0);
        }

        _commandList.DrawIndexed(indexCount);
        _commandList.End();

        var device = _graphics.Device;
        device.SubmitCommands(_commandList);
        device.WaitForIdle();
        BatchCount++;
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

    public void SetClipRect(RectInt? clipRect)
    {
        if (ClipRect == clipRect) return;
        EndBatch();
        ClipRect = clipRect;
    }

    public void SetTexture(uint textureId)
    {
        if (Texture.Id == textureId) return;
        Texture2d? texture = null;
        if (_resources.TryGet(textureId, out var resource) &&
            resource is Texture2d resourceTexture)
        {
            texture = resourceTexture;
        }
        SetTexture(texture);
    }

    public void SetTexture(Texture2d? texture)
    {
        texture ??= _nullTexture;
        if (texture == Texture) return;
        if (_material == null) throw BeginBatchNotCalledException();
        if (_material.MainTextureProperty != null)
        {
            // If the material supports a texture, end batch
            EndBatch();
        }
        Texture = texture;
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
