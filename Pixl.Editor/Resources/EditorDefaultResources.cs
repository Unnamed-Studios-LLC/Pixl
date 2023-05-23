namespace Pixl.Editor;

internal sealed class EditorDefaultResources
{
    public EditorDefaultResources(Property worldToClipMatrix, Material guiMaterial)
    {
        WorldToClipMatrix = worldToClipMatrix;
        GuiMaterial = guiMaterial;
    }

    public Property WorldToClipMatrix { get; }
    public Material GuiMaterial { get; }

    public void Add(Resources resources)
    {
        resources.Add(WorldToClipMatrix);
        resources.Add(GuiMaterial);
    }
}
