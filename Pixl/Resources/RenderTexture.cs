using System.Runtime.CompilerServices;
using Veldrid;

namespace Pixl;

public sealed class RenderTexture : GraphicsResource
{
    private Texture? _depthTexture;

    public RenderTexture(Int2 size, SampleMode sampleMode, ColorFormat colorFormat)
    {
        if (size.X <= 0) throw new ArgumentException($"{nameof(RenderTexture)} width cannot be less than or equal to 0", nameof(size));
        if (size.Y <= 0) throw new ArgumentException($"{nameof(RenderTexture)} height cannot be less than or equal to 0", nameof(size));
        Size = size;
        SampleMode = sampleMode;
        ColorFormat = colorFormat;
        ColorTexture = new Texture2d(size, sampleMode, colorFormat, false)
        {
            IsRenderTarget = true
        };
    }

    /// <summary>
    /// The size of the texture
    /// </summary>
    public Int2 Size { get; private set; }
    public SampleMode SampleMode { get; }
    public ColorFormat ColorFormat { get; }
    internal Texture2d ColorTexture { get; private set; }
    internal Framebuffer? Framebuffer { get; private set; }

    public void Resize(Int2 targetSize)
    {
        if (targetSize == Size) return; // no change occurred
        if (targetSize.X <= 0) throw new ArgumentException($"{nameof(RenderTexture)} width cannot be less than or equal to 0", nameof(targetSize));
        if (targetSize.Y <= 0) throw new ArgumentException($"{nameof(RenderTexture)} height cannot be less than or equal to 0", nameof(targetSize));
        Size = targetSize;

        var graphics = Graphics;
        if (graphics != null) Destroy();
        ColorTexture.Dispose();
        ColorTexture = new Texture2d(targetSize, SampleMode, ColorFormat, false)
        {
            IsRenderTarget = true
        };
        if (graphics != null) Create(graphics);
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var factory = graphics.ResourceFactory;
        var depthTextureDescription = new TextureDescription((uint)Size.X, (uint)Size.Y, 1, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil, TextureType.Texture2D);
        _depthTexture = factory.CreateTexture(depthTextureDescription);

        ColorTexture.Create(graphics);

        var framebufferDescription = new FramebufferDescription(_depthTexture, ColorTexture.Texture);
        Framebuffer = factory.CreateFramebuffer(framebufferDescription);
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        _depthTexture?.Dispose();
        _depthTexture = null;

        ColorTexture.Destroy();

        Framebuffer?.Dispose();
        Framebuffer = null;
    }
}
