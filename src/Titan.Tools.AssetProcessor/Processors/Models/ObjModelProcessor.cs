using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Models;
internal class ObjModelProcessor : AssetProcessor<ObjModelMetadata>
{
    protected override Task OnProcess(ObjModelMetadata metadata, IAssetDescriptorContext context)
    {
        Logger.Warning<ObjModelMetadata>($"Processing {metadata.Id} (NYI)");
        return Task.CompletedTask;
    }
}
