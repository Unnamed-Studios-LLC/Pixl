using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using Veldrid;

namespace Pixl;

public sealed class Texture2d : GraphicsResource
{
    private readonly byte[]? _data;

    internal Texture2d(Int2 size, SampleMode sampleMode, ColorFormat colorFormat, bool readWrite)
    {
        if (size.X <= 0) throw new ArgumentException($"{nameof(RenderTexture)} width cannot be less than or equal to 0", nameof(size));
        if (size.Y <= 0) throw new ArgumentException($"{nameof(RenderTexture)} height cannot be less than or equal to 0", nameof(size));

        Size = size;
        TexelSize = Vec2.One / size;
        SampleMode = sampleMode;
        ColorFormat = colorFormat;

        var pixelByteSize = GetPixelByteSize(colorFormat);
        _data = readWrite ? new byte[size.X * size.Y * pixelByteSize] : null;
    }

    public Int2 Size { get; }
    public Vec2 TexelSize { get; }
    public SampleMode SampleMode { get; set; }
    public ColorFormat ColorFormat { get; }

    internal bool IsRenderTarget { get; set; }
    internal Texture? Texture { get; private set; }
    internal TextureView? TextureView { get; private set; }

    /// <summary>
    /// Creates a <see cref="Texture2d"/>. <inheritdoc cref="App.RequireMainThread"/>
    /// </summary>
    /// <param name="size"></param>
    /// <param name="sampleMode">The initial sample mode of the texture.</param>
    /// <param name="colorFormat">The pixel color format to use.</param>
    /// <param name="normalized">If pixel components should be 0 -> 1 normalized in shaders.</param>
    public static Texture2d Create(Int2 size, SampleMode sampleMode, ColorFormat colorFormat)
    {
        Game.Shared.RequireMainThread();
        var texture2d = new Texture2d(size, sampleMode, colorFormat, true);
        var resources = Game.Shared.Resources;
        resources.Add(texture2d);
        return texture2d;
    }

    /// <summary>
    /// <inheritdoc cref="Create"/> Supported file types: png, jpeg, bmp, gif, tiff, tga, webp
    /// </summary>
    /// <param name="fileStream">A stream representing a supported file type.</param>
    /// <param name="sampleMode">The initial sample mode of the texture.</param>
    /// <param name="colorFormat">The pixel color format to use.</param>
    /// <param name="normalized">If pixel components should be 0 -> 1 normalized in shaders.</param>
    public static Texture2d CreateFromFile(Stream fileStream, SampleMode sampleMode, ColorFormat colorFormat)
    {
        Game.Shared.RequireMainThread();
        var texture2d = colorFormat switch
        {
            ColorFormat.R8 => LoadFromFile<A8>(fileStream, sampleMode, colorFormat),
            _ => LoadFromFile<Rgba32>(fileStream, sampleMode, colorFormat)
        };

        var resources = Game.Shared.Resources;
        resources.Add(texture2d);
        return texture2d;
    }

    /// <summary>
    /// Commits changes to color data to the GPU texture. <inheritdoc cref="App.RequireMainThread"/>
    /// </summary>
    public void Apply()
    {
        Game.Shared.RequireMainThread();
        if (_data == null) throw ReadWriteDisabledException();
        if (Graphics == null) return;
        UpdateTexture(Graphics);
    }

    /// <summary>
    /// Gets texture data
    /// </summary>
    public Span<byte> GetData()
    {
        if (_data == null) throw ReadWriteDisabledException();
        return _data.AsSpan();
    }

    /// <summary>
    /// Gets texture data in the form of a given struct
    /// </summary>
    public Span<T> GetData<T>() where T : unmanaged
    {
        var data = GetData();
        var span = MemoryMarshal.Cast<byte, T>(data);
        return span;
    }

    internal void ApplyInternal()
    {
        if (_data == null) throw ReadWriteDisabledException();
        if (Graphics == null) return;
        UpdateTexture(Graphics);
    }

    internal IEnumerable<BindableResource> GetBindableResources(Graphics graphics)
    {
        if (TextureView == null) throw new Exception($"{nameof(TextureView)} is null!");
        yield return TextureView;
        yield return graphics.GetSampler(SampleMode);
    }

    internal override void OnCreate(Graphics graphics)
    {
        base.OnCreate(graphics);

        var factory = graphics.ResourceFactory;
        var pixelFormat = GetPixelFormat(ColorFormat);
        var textureUsage = GetTextureUsage();
        var textureDescription = TextureDescription.Texture2D((uint)Size.X, (uint)Size.Y, 1, 1, pixelFormat, textureUsage);
        Texture = factory.CreateTexture(ref textureDescription);

        var textureViewDescription = new TextureViewDescription(Texture);
        TextureView = factory.CreateTextureView(ref textureViewDescription);

        UpdateTexture(graphics);
    }

    internal override void OnDestroy(Graphics graphics)
    {
        base.OnDestroy(graphics);

        Texture?.Dispose();
        Texture = null;

        TextureView?.Dispose();
        TextureView = null;
    }

    private static Texture2d LoadFromFile<T>(Stream fileStream, SampleMode sampleMode, ColorFormat colorFormat) where T : unmanaged, IPixel<T>
    {
        var image = Image.Load<T>(fileStream);
        var texture2d = new Texture2d(new Int2(image.Size.Width, image.Size.Height), sampleMode, colorFormat, true);
        image.CopyPixelDataTo(texture2d.GetData());
        return texture2d;
    }

    private static int GetPixelByteSize(ColorFormat colorFormat)
    {
        return colorFormat switch
        {
            ColorFormat.R8 => 1,
            _ => 4
        };
    }

    private static PixelFormat GetPixelFormat(ColorFormat colorFormat)
    {
        return colorFormat switch
        {
            ColorFormat.R8 => PixelFormat.R8_UNorm,
            _ => PixelFormat.R8_G8_B8_A8_UNorm
        };
    }

    private TextureUsage GetTextureUsage()
    {
        var usage = TextureUsage.Sampled;
        if (IsRenderTarget) usage |= TextureUsage.RenderTarget;
        return usage;
    }

    private static Exception ReadWriteDisabledException() => new("Reading or writing this texture's data is not supported");

    private void UpdateTexture(Graphics graphics)
    {
        if (_data == null) return;
        graphics.Device.UpdateTexture(Texture, _data, 0, 0, 0, (uint)Size.X, (uint)Size.Y, 1, 0, 0);
    }
}
