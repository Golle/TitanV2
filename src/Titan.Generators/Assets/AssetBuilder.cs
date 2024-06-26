using System.Text;

namespace Titan.Generators.Assets;
internal static class AssetBuilder
{
    public static string Build(in AssetType assetType)
    {
        var asset = assetType.Asset;
        var type = assetType.Type;
        var builder = new FormattedBuilder(new StringBuilder());

        builder
            .AppendAutoGenerated()
            .AppendLine()
            .AppendLine($"namespace {asset.ContainingNamespace.ToDisplayString()};")
            .AppendLine()
            .AppendLine($"{asset.DeclaredAccessibility.AsString()} unsafe partial struct {asset.Name} : {TitanTypes.IAsset}")
            .AppendOpenBracer()
            .AppendLine($"public static {TitanTypes.AssetType} Type => ({TitanTypes.AssetType}){type};")
            .AppendCloseBracer();

        return builder.ToString();

    }
}
