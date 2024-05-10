using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;

namespace Titan.Tools.AssetProcessor.Processors;
internal sealed class AssetPipeline
{
    private readonly Dictionary<Type, IAssetProcessor> _processors = new();
    public AssetPipeline With(IAssetProcessor assetProcessor)
    {
        _processors.Add(assetProcessor.Type, assetProcessor);
        return this;
    }

    public AssetPipeline With<T>() where T : IAssetProcessor, new()
    {
        var assetProcessor = new T();
        _processors.Add(assetProcessor.Type, assetProcessor);
        return this;
    }

    public async Task<bool> Run(AssetFileMetadata[] metadataFiles, IAssetDescriptorContext context)
    {
        var success = true;
        await Parallel.ForEachAsync(metadataFiles, async (metadata, _) =>
        {
            var result = await Process(metadata, context);
            if (!result)
            {
                success = false;
            }
        });

        if (!success)
        {
            return false;
        }

        await context.Complete();

        return true;
    }

    public async Task<bool> Process(AssetFileMetadata metadata, IAssetDescriptorContext context)
    {
        if (_processors.TryGetValue(metadata.GetType(), out var processor))
        {
            await processor.Process(metadata, context);
            return true;
        }
        Logger.Error<AssetPipeline>($"Failed to find a {nameof(IAssetProcessor)} for metadata type {metadata.GetType().Name}");
        return false;
    }
}
