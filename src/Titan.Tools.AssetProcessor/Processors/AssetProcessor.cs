using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;

namespace Titan.Tools.AssetProcessor.Processors;

internal abstract class AssetProcessor<TMetadata> : IAssetProcessor where TMetadata : AssetFileMetadata
{
    public Type Type => typeof(TMetadata);
    public async Task Process(AssetFileMetadata metadata, IAssetDescriptorContext context)
    {
        if (metadata is TMetadata meta)
        {
            if (metadata.Skip)
            {
                Logger.Info<IAssetProcessor>($"Skipping Asset. Skip = {metadata.Skip}. Id = {meta.Id} File = {meta.ContentFileRelativePath}");
            }
            else
            {
                var timer = Stopwatch.StartNew();
                Logger.Info($"Processing {metadata.Id}", GetType());
                await OnProcess(meta, context);
                Logger.Info($"Processing {metadata.Id} finished in {timer.Elapsed.TotalMilliseconds}", GetType());
            }
        }
    }

    protected abstract Task OnProcess(TMetadata metadata, IAssetDescriptorContext context);
}
