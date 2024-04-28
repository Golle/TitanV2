using Titan.Tools.AssetProcessor.Processors;

namespace Titan.Tools.AssetProcessor.Export;

internal class RegistryExporter(string outputPath, string registryName, string binaryFileName) : IExporter
{
    public async Task<bool> Export(IAssetDescriptorContext context)
    {
        var (@namespace, name) = ParseName(registryName);
        var content = CreateContent(context, @namespace, name, binaryFileName);
        var outputFilePath = Path.Combine(outputPath, $"{name}.cs");
        await File.WriteAllTextAsync(outputFilePath, content);
        return true;

        static string CreateContent(IAssetDescriptorContext context, string? @namespace, string name, string bin)
        {
            var assets = context.GetAssets();
            var builder = new RegistryBuilder(@namespace, name, bin);
            foreach (ref readonly var asset in assets.Span)
            {
                builder.AddDescriptor(asset.Descriptor, asset.Metadata);
            }
            return builder.Build();
        }

        static (string? Namespace, string Name) ParseName(string name)
        {
            var lastDot = name.LastIndexOf('.');

            if (lastDot > 0)
            {
                var @namespace = name.Substring(0, lastDot);
                return (@namespace, StringHelper.ToPropertyName(name.Substring(lastDot + 1)));
            }

            return (null, StringHelper.ToPropertyName(name));
        }
    }
}
