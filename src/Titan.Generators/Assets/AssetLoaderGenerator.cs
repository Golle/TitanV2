using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Titan.Generators.Assets;

internal struct AssetLoaderType(INamedTypeSymbol loaderSymbol, INamedTypeSymbol assetSymbol)
{
    public INamedTypeSymbol LoaderSymbol = loaderSymbol;
    public INamedTypeSymbol AssetSymbol = assetSymbol;
}

[Generator]
public class AssetLoaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            TitanTypes.AssetLoaderAttributeMetadataName,
            static (node, _) => node.IsPartial() && node.IsStruct(),
            static (syntaxContext, _) =>
            {
                var loaderType = (INamedTypeSymbol)syntaxContext.TargetSymbol;
                var assetType = loaderType
                    .GetAttributes()
                    .First()
                    .AttributeClass!
                    .TypeArguments
                    .OfType<INamedTypeSymbol>()
                    .Single();

                return new AssetLoaderType(loaderType, assetType);
            })
            ;

        var assetLoaderProvider = structDeclarations
            .Collect()
            ;

        context
            .RegisterImplementationSourceOutput(assetLoaderProvider, static (productionContext, source) => Execute(source, productionContext));

        static void Execute(ImmutableArray<AssetLoaderType> loaders, SourceProductionContext context)
        {
            foreach (var loader in loaders)
            {
                var source = AssetLoaderBuilder.Build(loader);
                context.AddSource($"{loader.LoaderSymbol.Name}.g.cs", source);
            }
        }
    }
}
