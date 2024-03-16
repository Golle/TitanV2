using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Models;
internal class MtlProcessor : AssetProcessor<MtlMetadata>
{
    protected override Task OnProcess(MtlMetadata metadata, IAssetDescriptorContext context)
    {
        Logger.Warning<MtlMetadata>($"Processing {metadata.Id} (NYI)");
        return Task.CompletedTask;
    }
}
