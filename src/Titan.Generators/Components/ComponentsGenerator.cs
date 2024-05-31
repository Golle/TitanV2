using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Titan.Generators.Components;

[Generator]
internal class ComponentsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TitanTypes.ComponentAttribute,
            static (node, _) => node.IsPartial() && node.IsStruct(),
            static (syntaxContext, _) => new ComponentType((INamedTypeSymbol)syntaxContext.TargetSymbol));

        var valueProvider = context
            .CompilationProvider
            .Combine(structDeclarations.Collect());

        context
            .RegisterSourceOutput(valueProvider, static (productionContext, source) => Execute(source.Left, source.Right, productionContext));

        static void Execute(Compilation _, ImmutableArray<ComponentType> components, SourceProductionContext context)
        {
            //NOTE(Jens): This is a very naive solution to component ID. We could put all of them in the same struct, and have getters for each type that will read them as references. Making them constants.
            
            foreach (var component in components)
            {
                var source = ComponentBuilder.Build(component);
                context.AddSource($"{component.Type.Name}.g.cs", source);
            }
        }
    }
}

internal readonly struct ComponentType(INamedTypeSymbol type)
{
    public readonly INamedTypeSymbol Type = type;
}
