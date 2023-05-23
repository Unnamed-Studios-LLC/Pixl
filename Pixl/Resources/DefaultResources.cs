namespace Pixl;

internal sealed class DefaultResources
{
    public DefaultResources(Property worldToClipMatrix, Material defaultMaterial, Material errorMaterial)
    {
        WorldToClipMatrix = worldToClipMatrix;
        DefaultMaterial = defaultMaterial;
        ErrorMaterial = errorMaterial;
    }

    public Property WorldToClipMatrix { get; }
    public Material DefaultMaterial { get; }
    public Material ErrorMaterial { get; }

    public void Add(Resources resources)
    {
        resources.Add(WorldToClipMatrix);
        resources.Add(DefaultMaterial);
        resources.Add(ErrorMaterial);
    }
}
