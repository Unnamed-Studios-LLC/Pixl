using ImGuiNET;

namespace Pixl.Editor;

internal sealed class EditorDefaultResources
{
    public EditorDefaultResources()
    {
        WorldToClipMatrix = new Property("WorldToClipMatrix", PropertyScope.Shared, PropertyDescriptor.CreateStandard(PropertyType.Mat4));
        GuiMaterial = CreateGui(WorldToClipMatrix);
    }

    public Property WorldToClipMatrix { get; }
    public Material GuiMaterial { get; }

    public void Add(Resources resources)
    {
        resources.Add(WorldToClipMatrix);
        resources.Add(GuiMaterial);
    }

    private static Material CreateGui(Property worldToClipMatrix)
    {
        var vertexHandle = AssetHandle.CreateInternal("gui.vert");
        var fragmentHandle = AssetHandle.CreateInternal("gui.frag");

        var vertexShader = new VertexShader<GuiVertex>(vertexHandle);
        var fragmentShader = new FragmentShader(fragmentHandle);

        var vertexProperties = new Property[]
        {
            worldToClipMatrix
        };

        var fragmentProperties = new Property[]
        {
            new Property("Main", PropertyScope.Local, PropertyDescriptor.CreateTexture2d())
        };

        var material = new Material(vertexShader, fragmentShader, vertexProperties, fragmentProperties)
        {
            DepthTestEnabled = false,
            ClipRectEnabled = true
        };
        return material;
    }
}
