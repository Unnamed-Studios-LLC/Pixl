using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Pixl.SourceGenerators;

internal static class Extensions
{
    public static string GenerateAttributesSourceConstructors(this ImmutableArray<AttributeData> attributes)
    {
        var attributeConstructors = attributes.Where(x => x.AttributeConstructor != null && x.AttributeClass != null)
            .Select(o =>
            {
                var parameters = o.AttributeConstructor!.Parameters
                    .Select((parameter, i) => new KeyValuePair<string, TypedConstant>(parameter.Name, o.ConstructorArguments[i]))
                    .Select(ConvertAttributeParameters);

                var attributeGlobalName = o.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return $"new {attributeGlobalName}({string.Join(", ", parameters)}),";
            });

        return $"new global::System.Attribute[] {{ {string.Join(Environment.NewLine, attributeConstructors)} }}";
    }

    public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
    {
        if (symbol.BaseType != null)
        {
            foreach (var member in symbol.BaseType.GetAllMembers())
            {
                yield return member;
            }
        }

        foreach (var member in symbol.GetMembers())
        {
            yield return member;
        }
    }

    public static IEnumerable<TSymbol> GetPublicMembers<TSymbol>(this IEnumerable<ISymbol> members)
        where TSymbol : ISymbol
    {
        return members
            .OfType<TSymbol>()
            .Where(o => o.DeclaredAccessibility.HasFlag(Accessibility.Public));
    }

    public static ImplementedTypes GetImplementedTypes(this ITypeSymbol typeSymbol)
    {
        if (!typeSymbol.DeclaredAccessibility.HasFlag(Accessibility.Public) ||
            typeSymbol is not INamedTypeSymbol namedTypeSymbol) return ImplementedTypes.None;

        var types = ImplementedTypes.None;
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (attribute.AttributeClass != null &&
                attribute.AttributeClass.Name.Equals("SerializableAttribute", StringComparison.Ordinal))
            {
                types |= ImplementedTypes.Serializable;
            }
        }

        if (typeSymbol.IsReferenceType)
        {
            var emptyContructor = namedTypeSymbol.Constructors.FirstOrDefault(x => x.Parameters.Length == 0);
            if (emptyContructor == null ||
                !emptyContructor.DeclaredAccessibility.HasFlag(Accessibility.Public))
            {
                return ImplementedTypes.None;
            }

            var baseType = typeSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name.Equals("ComponentSystem", StringComparison.Ordinal))
                {
                    types |= ImplementedTypes.ComponentSystem;
                    break;
                }
                baseType = baseType.BaseType;
            }
        }
        else if (typeSymbol.IsValueType)
        {
            foreach (var @interface in typeSymbol.AllInterfaces)
            {
                if (@interface.Name.Equals("IComponent", StringComparison.Ordinal)) types |= ImplementedTypes.Component;
                if (@interface.Name.Equals("IVertex", StringComparison.Ordinal)) types |= ImplementedTypes.Vertex;
            }
        }
        return types;
    }

    private static string ConvertAttributeParameters(KeyValuePair<string, TypedConstant> pair)
    {
        if (pair.Value.Kind == TypedConstantKind.Array && !pair.Value.IsNull)
        {
            return $@"new[] {pair.Value.ToCSharpString()}";
        }

        if (pair.Value.Type.IsFloat())
        {
            return $@"{pair.Value.ToCSharpString()}F";
        }

        return $@"{pair.Value.ToCSharpString()}";
    }

    private static bool IsFloat(this ITypeSymbol? typeSymbol)
    {
        if (typeSymbol == null) return false;
        return typeSymbol.IsValueType && typeSymbol.Name.Equals("Single", StringComparison.Ordinal);
    }
}
