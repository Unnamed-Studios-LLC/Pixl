using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pixl.SourceGenerators.Templates;
using System.Collections.Immutable;

namespace Pixl.SourceGenerators.MetaData;

[Generator]
internal class TypeMetaDataGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var types = context.SyntaxProvider.CreateSyntaxProvider(FindNodes, ProcessNodes)
            .Where(type => type != null)
            .WithComparer(SymbolEqualityComparer.Default);
        context.RegisterSourceOutput(types.Collect(), Generate!);
    }

    private static bool FindNodes(SyntaxNode syntaxNode, CancellationToken cancellationToken) => syntaxNode switch
    {
        ClassDeclarationSyntax => true,
        StructDeclarationSyntax => true,
        _ => false
    };

    private static void Generate(SourceProductionContext context, ImmutableArray<ITypeSymbol> types)
    {
        var uniqueTypes = types.ToImmutableHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var type in uniqueTypes)
        {
            if (context.CancellationToken.IsCancellationRequested) return;
            var source = TypeMetaDataTemplate.Create(type);
            if (context.CancellationToken.IsCancellationRequested) return;

            var displayName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var fileName = $"{displayName.Replace("global::", "")}MetaData.g";
            context.AddSource(fileName, source);
        }
    }

    private static ITypeSymbol? GetTypeSymbol(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken)
    {
        var symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node, cancellationToken);
        if (symbol == null ||
            symbol is not ITypeSymbol typeSymbol) return null;

        var implementedTypes = typeSymbol.GetImplementedTypes();
        if (implementedTypes == ImplmentedTypes.None) return null;
        return typeSymbol;
    }

    private static ITypeSymbol? ProcessNodes(GeneratorSyntaxContext syntaxContext, CancellationToken cancellationToken) => syntaxContext.Node switch
    {
        ClassDeclarationSyntax or StructDeclarationSyntax => GetTypeSymbol(syntaxContext, cancellationToken),
        _ => null
    };
}
