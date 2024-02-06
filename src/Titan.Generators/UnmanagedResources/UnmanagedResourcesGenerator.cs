using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Titan.Generators.UnmanagedResources;
[Generator]
public class UnmanagedResourcesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TitanTypes.UnmanagedResourceAttribute,
            static (node, _) => node is StructDeclarationSyntax structDecl && structDecl.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword)),
            static (syntaxContext, _) => new UnmanagedResourceType((INamedTypeSymbol)syntaxContext.TargetSymbol));

        var valueProvider = context
            .CompilationProvider
            .Combine(structDeclarations.Collect());
        
        context
            .RegisterSourceOutput(valueProvider, static (productionContext, source) => Execute(source.Left, source.Right!, productionContext));

        static void Execute(Compilation _, ImmutableArray<UnmanagedResourceType> resources, SourceProductionContext context)
        {
            foreach (var resource in resources)
            {
                var result = UnmanagedResourceBuilder.Build(resource);
                context.AddSource($"{resource.Symbol.Name}.g.cs", result);
            }
        }
    }
}
