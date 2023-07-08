namespace Pixl;

internal sealed class TextureImporter : AssetImporter
{
    public SampleMode SampleMode { get; set; }
    public ColorFormat ColorFormat { get; set; }

    protected override void Import(string fileName, Stream stream, Action<Resource> outputFunc)
    {
        var texture = Texture2d.CreateFromFile(stream, SampleMode, ColorFormat);
        outputFunc(texture);
    }
}
