using Veldrid;

namespace Pixl;

internal static class CommandListExtensions
{
    public static void SetMaterial(this CommandList commandList, Material material, Framebuffer framebuffer, Graphics graphics)
    {
        var pipeline = material.CreatePipeline(graphics, framebuffer);
        commandList.SetPipeline(pipeline);

        uint slot = 0;
        foreach (var resourceSet in material.CreateResourceSets(graphics))
        {
            commandList.SetGraphicsResourceSet(slot++, resourceSet);
        }
    }
}
