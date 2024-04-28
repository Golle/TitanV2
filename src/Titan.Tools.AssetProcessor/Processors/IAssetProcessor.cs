using Titan.Tools.AssetProcessor.Metadata;

namespace Titan.Tools.AssetProcessor.Processors;
internal interface IAssetProcessor
{
    Type Type { get; }
    Task Process(AssetFileMetadata metadata, IAssetDescriptorContext context);
}
