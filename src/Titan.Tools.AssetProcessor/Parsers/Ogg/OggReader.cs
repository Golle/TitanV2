using NVorbis;
using System.Buffers;
using Titan.Core.Logging;
using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;
internal class OggReader
{
    private const uint SampleRate = 44100;
    private const uint Channels = 2;
    public static ReadOnlySpan<byte> Read(byte[] data)
    {
        //NOTE(Jens): Due to the complexity of ogg - vorbis I've decided to take a shortcut and use a library to save some time. I will eventually implement my own decoder..
        using var reader = new VorbisReader(new MemoryStream(data));

        if (reader.SampleRate != SampleRate)
        {
            Logger.Error<OggReader>($"Mismatch in sample rate. Resampling the Audio is not supported yet. Expected = {SampleRate} Audio File Sample Rate = {reader.SampleRate}");
            return ReadOnlySpan<byte>.Empty;
        }

        if (reader.Channels != Channels)
        {
            Logger.Error<OggReader>($"Mismatch in number of channels. Convertion is not supported yet. Expected = {Channels}. Audio File Channels = {reader.Channels}");
            return ReadOnlySpan<byte>.Empty;
        }
        

        var totalSamples = (int)(reader.TotalTime.TotalSeconds * reader.SampleRate * reader.Channels + 0.5);
        var sampleBuffer = new byte[totalSamples * sizeof(short)];
        var readBuffer = ArrayPool<float>.Shared.Rent(4096 * reader.Channels);
        try
        {
            var writer = new TitanBinaryWriter(sampleBuffer.AsSpan());
            

            int samplesRead;
            // sampling is very slow, not sure why, but it's probably a really bad implementation :| buffer size affects it a bit, but not huge change.
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

            return writer.GetData();
        }
        finally
        {
            ArrayPool<float>.Shared.Return(readBuffer);
        }
    }
}
