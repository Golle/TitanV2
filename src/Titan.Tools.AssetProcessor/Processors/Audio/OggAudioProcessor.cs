using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Parsers.Ogg;

namespace Titan.Tools.AssetProcessor.Processors.Audio;

internal class AudioMetadata : AssetFileMetadata
{

}

internal class AudioProcessor : AssetProcessor<AudioMetadata>
{
    protected override async Task OnProcess(AudioMetadata metadata, IAssetDescriptorContext context)
    {
        var extension = Path.GetExtension(metadata.ContentFileRelativePath);
        if (extension is ".ogg")
        {
            var bytes = await File.ReadAllBytesAsync(metadata.ContentFileFullPath);
            OggReader2.Read(bytes);
        }
        else
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"The audio file extension {extension} is not supported yet.");
        }
    }
}
