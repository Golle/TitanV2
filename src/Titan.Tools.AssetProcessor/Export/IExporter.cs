using Titan.Tools.AssetProcessor.Processors;

namespace Titan.Tools.AssetProcessor.Export;

internal interface IExporter
{
    Task<bool> Export(IAssetDescriptorContext context);
}
