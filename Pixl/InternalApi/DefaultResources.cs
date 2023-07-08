namespace Pixl;

internal sealed class DefaultResources
{
    public DefaultResources(Files files)
    {
        WorldToClipMatrix = new Property("WorldToClipMatrix", PropertyScope.Shared, PropertyDescriptor.CreateStandard(PropertyType.Mat4));
        DefaultMaterial = Material.CreateDefault(files, WorldToClipMatrix);
        ErrorMaterial = Material.CreateError(files, WorldToClipMatrix);
        GuiMaterial = Material.CreateGui(files, WorldToClipMatrix);
        NullTexture = CreateNullTexture();
    }

    public Property WorldToClipMatrix { get; }
    public Material DefaultMaterial { get; }
    public Material ErrorMaterial { get; }
    public Material GuiMaterial { get; }
    public Texture2d NullTexture { get; }

    public void Add(Resources resources)
    {
        resources.Add(WorldToClipMatrix, 1, true);
        resources.Add(DefaultMaterial, 2, true);
        resources.Add(ErrorMaterial, 3, true);
        resources.Add(GuiMaterial, 4, true);
        resources.Add(NullTexture, 5, true);
    }

    public void Remove(Resources resources)
    {
        resources.Remove(WorldToClipMatrix);
        resources.Remove(DefaultMaterial);
        resources.Remove(ErrorMaterial);
        resources.Remove(GuiMaterial);
        resources.Remove(NullTexture);
    }

    private static Texture2d CreateNullTexture()
    {
        var nullTexture = new Texture2d(new Int2(1, 1), SampleMode.Point, ColorFormat.Rgba32, true);
        nullTexture.GetData<Color32>()[0] = Color32.White;
        return nullTexture;
    }
}
