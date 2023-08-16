using Microsoft.CodeAnalysis;

namespace Pixl.SourceGenerators.Templates;

internal static class TypeMetaDataTemplate
{
    public static string Create(ITypeSymbol type)
    {
        var globalName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var safeGlobalName = globalName.Replace(".", "").Replace("global::", "");
        var implementedTypes = type.GetImplementedTypes();
        var isUnmanaged = type.IsUnmanagedType;
        var isComponent = isUnmanaged && (implementedTypes & ImplementedTypes.Component) == ImplementedTypes.Component;
        var isVertex = isUnmanaged && (implementedTypes & ImplementedTypes.Vertex) == ImplementedTypes.Vertex;
        var className = isUnmanaged ? "UnmanagedTypeMetaData" : "TypeMetaData";
        var hasDefault = false;

        if (isUnmanaged &&
            type is INamedTypeSymbol namedType)
        {
            hasDefault = namedType.GetMembers().Any(x => x is IFieldSymbol field && field.IsStatic && field.DeclaredAccessibility == Accessibility.Public && field.Name.Equals("Default", StringComparison.Ordinal));
        }

        var source = $@"
namespace Pixl
{{
    public static class {safeGlobalName}Extensions
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Initialize()
        {{
            var type = typeof({globalName});
            var metaData = CreateMetaData();
            MetaData.Register(type, metaData);
        }}
        
        private static {className}<{globalName}> CreateMetaData()
        {{
            return new {className}<{globalName}>(
                ""{type.Name}"",
                {(isComponent ? "true" : "false")},
                {(isVertex ? "true" : "false")},
                {type.GetAttributes().GenerateAttributesSourceConstructors()},
                {CreateFieldConstructors(type)},
                () => {(hasDefault ? $"{globalName}.Default" : $"new {globalName}()")}
            );
        }}
    }}
}}
";
        return source;
    }

    private static string CreateGetterAndSetter(IFieldSymbol field, string instanceType, string fieldType)
    {
        return field.IsReadOnly ?
            $"static instance => instance.{field.Name}, null" :
            $"static instance => instance.{field.Name}, static (ref {instanceType} instance, in {fieldType} value) => instance.{field.Name} = value";
    }

    private static string CreateFieldConstructors(ITypeSymbol type)
    {
        var globalName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var fields = type.GetAllMembers()
            .GetPublicMembers<IFieldSymbol>()
            .Where(x => !x.IsStatic)
            .GroupBy(o => o.Name)
            .Select(o => o.Last());

        var fieldObjects = fields.Select(field =>
        {
            var fieldGlobalName = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var fieldAttributes = field.GetAttributes().GenerateAttributesSourceConstructors();
            return $"new FieldMetaData<{globalName}, {fieldGlobalName}>(\"{field.Name}\", {fieldAttributes}, {CreateGetterAndSetter(field, globalName, fieldGlobalName)})";
        });

        return $"new FieldMetaData[] {{ {string.Join(", ", fieldObjects)} }}";
    }
}
