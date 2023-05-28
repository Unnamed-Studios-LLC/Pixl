namespace Pixl;

internal sealed class DefaultResources
{
    public DefaultResources()
    {
        WorldToClipMatrix = new Property("WorldToClipMatrix", PropertyScope.Shared, PropertyDescriptor.CreateStandard(PropertyType.Mat4));
        DefaultMaterial = Material.CreateDefault(WorldToClipMatrix);
        ErrorMaterial = Material.CreateError(WorldToClipMatrix);
        NullTexture = CreateNullTexture();
    }

    public Property WorldToClipMatrix { get; }
    public Material DefaultMaterial { get; }
    public Material ErrorMaterial { get; }
    public Texture2d NullTexture { get; }

    public void Add(Resources resources)
    {
        resources.Add(WorldToClipMatrix);
        resources.Add(DefaultMaterial);
        resources.Add(ErrorMaterial);
        resources.Add(NullTexture);
    }

    private static Texture2d CreateNullTexture()
    {
        var nullTexture = new Texture2d(new Int2(1, 1), SampleMode.Point, ColorFormat.Rgba32, true);
        nullTexture.GetData<Color32>()[0] = Color32.White;
        return nullTexture;
    }
}
