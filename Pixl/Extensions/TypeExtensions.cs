using System.Collections.Concurrent;
using System.Reflection;
using Veldrid;

namespace Pixl
{
    internal static class TypeExtensions
    {
        private readonly static ConcurrentDictionary<Type, bool> s_unmanagedCache = new();
        private readonly static Dictionary<Type, VertexElementFormat> s_vertexFormatMappings = new()
        {
            [typeof(Vec2)] = VertexElementFormat.Float2,
            [typeof(Vec3)] = VertexElementFormat.Float3,
            [typeof(Vec4)] = VertexElementFormat.Float4,
            [typeof(Int2)] = VertexElementFormat.Int2,
            [typeof(Int3)] = VertexElementFormat.Int3,
            [typeof(Int4)] = VertexElementFormat.Int4,
            [typeof(Color32)] = VertexElementFormat.Byte4_Norm,
        };

        public static bool HasEmptyConstructor(this Type type)
        {
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (constructor.GetParameters().Length == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool DoesOverride(this Type type, Type baseType, string methodName) => type.DoesOverride(baseType, methodName, Array.Empty<Type>());
        public static bool DoesOverride(this Type type, Type baseType, string methodName, params Type[] parameterTypes)
        {
            var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, parameterTypes);
            if (method == null) return false;
            return method.DeclaringType != baseType;
        }

        public static bool TryGetVertexFormat(this Type type, out VertexElementFormat format)
        {
            return s_vertexFormatMappings.TryGetValue(type, out format);
        }

        public static bool IsUnmanaged(this Type type)
        {
            if (s_unmanagedCache.TryGetValue(type, out var unmanaged)) return unmanaged;

            if (type.IsPrimitive || type.IsPointer || type.IsEnum) unmanaged = true;
            else if (type.IsGenericType || !type.IsValueType) unmanaged = false;
            else unmanaged = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .All(x => x.FieldType.IsUnmanaged());

            s_unmanagedCache[type] = unmanaged;
            return unmanaged;
        }
    }
}
