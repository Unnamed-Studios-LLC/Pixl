using System.Reflection;

namespace Pixl
{
    internal static class TypeExtensions
    {
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
    }
}
