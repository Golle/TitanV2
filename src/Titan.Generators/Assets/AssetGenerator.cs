using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Titan.Generators.Assets;

[Generator]
public class AssetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TitanTypes.AssetAttribute,
            static (node, _) => node.IsPartial() && node.IsStruct(),
            static (syntaxContext, _) =>
            {

                var type = (int)syntaxContext
                    .Attributes[0]
                    .ConstructorArguments[0]
                    .Value!;

                return new AssetType((INamedTypeSymbol)syntaxContext.TargetSymbol, type);
            });

        var valueProvider = context
            .CompilationProvider
            .Combine(structDeclarations.Collect());

        context
            .RegisterSourceOutput(valueProvider, static (productionContext, source) => Execute(source.Left, source.Right, productionContext));

        static void Execute(Compilation _, ImmutableArray<AssetType> assets, SourceProductionContext context)
        {
            foreach (var asset in assets)
            {
                var source = AssetBuilder.Build(asset);
                context.AddSource($"{asset.Asset.Name}.g.cs", source);
            }
        }
    }
}

public readonly struct AssetType(INamedTypeSymbol asset, int type)
{
    public readonly INamedTypeSymbol Asset = asset;
    public readonly int Type = type;
}
