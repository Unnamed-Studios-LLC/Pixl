using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid;

namespace Pixl;

public class VertexShader : Shader
{
    public VertexShader(string filePath, Type vertexType) : base(filePath)
    {
        VertexLayoutDescription = GenerateVertexDescription(vertexType);
    }

    internal VertexLayoutDescription VertexLayoutDescription { get; }

    private static VertexLayoutDescription GenerateVertexDescription(Type type)
    {
        var elements = GetElementDescriptions(type)
            .ToArray();
        return new VertexLayoutDescription((uint)Marshal.SizeOf(type), elements);
    }

    private static IEnumerable<VertexElementDescription> GetElementDescriptions(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return fields.Select(x =>
        {
            var offset = Marshal.OffsetOf(type, x.Name);
            if (!x.FieldType.TryGetVertexFormat(out var format))
            {
                throw new Exception($"{type} contains an invalid vertex field: {x.Name}. Only VecX, IntX, or Color structs are allowed.");
            }
            return new VertexElementDescription(x.Name, VertexElementSemantic.TextureCoordinate, format, (uint)offset);
        });
    }
}

public sealed class VertexShader<T> : VertexShader where T : unmanaged
{
    public VertexShader(string filePath) : base(filePath, typeof(T)) { }
}
