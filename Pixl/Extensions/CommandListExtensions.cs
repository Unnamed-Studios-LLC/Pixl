using Veldrid;

namespace Pixl;

internal static class CommandListExtensions
{
    public static void SetMaterial(this CommandList commandList, Material material, Framebuffer framebuffer, Graphics graphics)
    {
        commandList.SetPipeline(material.CreatePipeline(graphics, framebuffer));

        uint slot = 0;
        foreach (var resourceSet in material.CreateResourceSets(graphics))
        {
            commandList.SetGraphicsResourceSet(slot++, resourceSet);
        }
    }
}
