using Veldrid;

namespace Pixl;

public sealed class RenderTexture : GraphicsResource
{
    private Texture? _depthTexture;
    private Texture? _colorTexture;

    public RenderTexture(Int2 size)
    {
        if (size.X <= 0) throw new ArgumentException($"{nameof(RenderTexture)} width cannot be less than or equal to 0", nameof(size));
        if (size.Y <= 0) throw new ArgumentException($"{nameof(RenderTexture)} height cannot be less than or equal to 0", nameof(size));
        Size = size;
    }

    /// <summary>
    /// The size of the texture
    /// </summary>
    public Int2 Size { get; }

    internal Framebuffer? Framebuffer { get; private set; }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var factory = graphics.ResourceFactory;
        var depthTextureDescription = new TextureDescription((uint)Size.X, (uint)Size.Y, 1, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil, TextureType.Texture2D);
        _depthTexture = factory.CreateTexture(depthTextureDescription);

        var colorTextureDescription = new TextureDescription((uint)Size.X, (uint)Size.Y, 1, 1, 1, PixelFormat.R32_G32_B32_A32_Float, TextureUsage.RenderTarget, TextureType.Texture2D);
        _colorTexture = factory.CreateTexture(colorTextureDescription);

        var framebufferDescription = new FramebufferDescription(_depthTexture, _colorTexture);
        Framebuffer = factory.CreateFramebuffer(framebufferDescription);
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        _depthTexture?.Dispose();
        _depthTexture = null;

        _colorTexture?.Dispose();
        _colorTexture = null;

        Framebuffer?.Dispose();
        Framebuffer = null;
    }
}
