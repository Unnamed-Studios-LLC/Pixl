using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

namespace Pixl;

public class VertexShader : Shader
{
    internal VertexShader(AssetHandle assetHandle, VertexLayoutDescription vertexLayout) : base(assetHandle)
    {
        VertexLayoutDescription = vertexLayout;
    }

    internal VertexLayoutDescription VertexLayoutDescription { get; }
}

public sealed class VertexShader<T> : VertexShader where T : unmanaged
{
    public VertexShader(string filePath) : this(AssetHandle.CreateAbsolutePath(filePath)) { }

    internal VertexShader(AssetHandle assetHandle) : base(assetHandle, GenerateVertexDescription()) { }

    private static VertexLayoutDescription GenerateVertexDescription()
    {
        var elements = GetElementDescriptions()
            .ToArray();
        return new VertexLayoutDescription((uint)Marshal.SizeOf<T>(), elements);
    }

    private static IEnumerable<VertexElementDescription> GetElementDescriptions()
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return fields.Select(x =>
        {
            var offset = Marshal.OffsetOf<T>(x.Name);
            if (!x.FieldType.TryGetVertexFormat(out var format))
            {
                throw new Exception($"{type} contains an invalid vertex field: {x.Name}. Only VecX, IntX, or Color structs are allowed.");
            }
            return new VertexElementDescription(x.Name, VertexElementSemantic.TextureCoordinate, format, (uint)offset);
        });
    }
}
