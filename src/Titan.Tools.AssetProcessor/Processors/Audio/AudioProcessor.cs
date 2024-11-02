using System.Buffers;
using System.Diagnostics;
using NVorbis;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;

namespace Titan.Tools.AssetProcessor.Processors.Audio;

internal class AudioMetadata : AssetFileMetadata;

internal class AudioProcessor : AssetProcessor<AudioMetadata>
{
    //NOTE(Jens): This should be configured in some other way, and must be the same in the runtime of the engine. Global setting?
    private const uint SampleRate = 44100;
    private const uint Channels = 2;
    protected override async Task OnProcess(AudioMetadata metadata, IAssetDescriptorContext context)
    {
        var extension = Path.GetExtension(metadata.ContentFileRelativePath);
        if (extension is ".ogg")
        {
            var timer = Stopwatch.StartNew();
            //NOTE(Jens): Due to the complexity of ogg - vorbis I've decided to take a shortcut and use a library to save some time. I will eventually implement my own decoder..
            var bytes = await File.ReadAllBytesAsync(metadata.ContentFileFullPath);
            using var reader = new VorbisReader(new MemoryStream(bytes));

            timer.Stop();
            Logger.Trace<AudioProcessor>($"Finished reading Audio file. Elapsed = {timer.Elapsed.TotalMilliseconds} ms. File = {metadata.ContentFileRelativePath}");

            var totalSamples = (int)(reader.TotalTime.TotalSeconds * reader.SampleRate * reader.Channels + 0.5);
            var sampleBuffer = ArrayPool<byte>.Shared.Rent(totalSamples * sizeof(short));
            var readBuffer = ArrayPool<float>.Shared.Rent(4096 * reader.Channels);
            try
            {
                timer.Restart();
                var writer = new TitanBinaryWriter(sampleBuffer.AsSpan());
                if (reader.SampleRate != SampleRate)
                {
                    Logger.Trace<AudioProcessor>($"Mismatch in sample rate. Resampling the Audio. Expected = {SampleRate} Audio File Sample Rate = {reader.SampleRate}");
                    context.AddDiagnostics(DiagnosticsLevel.Error, "Resampling is not supported yet.");
                }

                int samplesRead;
                // sampling is very slow, not sure why, but it's probably a really bad implementation :| buffer size affects its a bit, but not huge change.
                while ((samplesRead = reader.ReadSamples(readBuffer.AsSpan())) > 0)
                {
                    //NOTE(Jens): this is slow, can be done with SIMD
                    for (var i = 0; i < samplesRead; ++i)
                    {
                        var clampedValue = Math.Clamp(readBuffer[i], -1f, 1f);
                        var value = (short)(clampedValue * short.MaxValue);
                        writer.WriteShort(value);
                    }
                }

                timer.Stop();
                Logger.Trace<AudioProcessor>($"Finished sampling Audio data. Elapsed = {timer.Elapsed.TotalMilliseconds} ms. File = {metadata.ContentFileRelativePath}");
                var audioDescriptor = new AudioDescriptor();
                if (!context.TryAddAudio(audioDescriptor, writer.GetData(), metadata))
                {
                    context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the Audio to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(readBuffer);
                ArrayPool<byte>.Shared.Return(sampleBuffer);
            }
        }
        else
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"The audio file extension {extension} is not supported yet.");
        }
    }
}
