using System.Buffers;
using System.Diagnostics;
using NVorbis;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Parsers.Ogg;
using Titan.Tools.AssetProcessor.Parsers.Wave;

namespace Titan.Tools.AssetProcessor.Processors.Audio;

internal class AudioMetadata : AssetFileMetadata;

internal class AudioProcessor : AssetProcessor<AudioMetadata>
{
    //NOTE(Jens): This should be configured in some other way, and must be the same in the runtime of the engine. Global setting?
    private const uint SampleRate = 44100;
    private const uint Channels = 2;

    protected override async Task OnProcess(AudioMetadata metadata, IAssetDescriptorContext context)
    {
        var bytes = await File.ReadAllBytesAsync(metadata.ContentFileFullPath);

        if (metadata.FileExtension is ".ogg")
        {

            var data = OggReader.Read(bytes);
            if (data.IsEmpty)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Data was empty when reading the Ogg data. Check log for details. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                return;
            }

            var audioDescriptor = new AudioDescriptor();
            if (!context.TryAddAudio(audioDescriptor, data, metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the Audio to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            }
        }
        else if (metadata.FileExtension is ".wav")
        {
            if (!WaveReader.TryRead(bytes, out var format, out var data))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to parse the wave file. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                return;
            }
            //NOTE(Jens): Add something about format, we need to verify and convert.

            if (format.nChannels != Channels)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Mismatch in number of channels. Channel change not implemented yet. Expected = {Channels} Audio File = {format.nChannels}. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                return;
            }

            if (format.nSamplesPerSec != SampleRate)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Mismatch in sample rate. Resampling not implemented yet. Expected = {SampleRate} Audio File = {format.nSamplesPerSec}. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                return;
            }

            var audioDescriptor = new AudioDescriptor();
            if (!context.TryAddAudio(audioDescriptor, data, metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the Audio to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
            }
        }
        else
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"The audio file extension {metadata.FileExtension} is not supported yet.");
        }
    }
}
