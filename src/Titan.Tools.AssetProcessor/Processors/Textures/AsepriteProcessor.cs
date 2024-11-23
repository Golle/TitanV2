using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Textures;

internal class AsepriteProcessor : AssetProcessor<AsepriteMetadata>
{
    protected override Task OnProcess(AsepriteMetadata metadata, IAssetDescriptorContext context)
    {
        Logger.Warning<AsepriteMetadata>($"Processing {metadata.Id}. Aseprite support not yet implemented.");
        return Task.CompletedTask;
    }
}
