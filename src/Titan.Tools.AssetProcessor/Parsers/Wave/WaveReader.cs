using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32.XAudio2;

namespace Titan.Tools.AssetProcessor.Parsers.Wave;

internal sealed class WaveReader
{
    public static bool TryRead(ReadOnlySpan<byte> data, out WAVEFORMATEX format, out ReadOnlySpan<byte> pcmData)
    {
        format = default;
        pcmData = default;

        var reader = new TitanBinaryReader(data);
        var riff = reader.Read(4);
        var size = reader.Read<uint>();
        if (size > data.Length)
        {
            Logger.Error<WaveReader>($"Not a valid wave file. The riff size is greater than the entire data stream. RIFF = {size} bytes. Data Length = {data.Length}");
            return false;
        }
        var wave = reader.Read(4);
        if (!riff.SequenceEqual("RIFF"u8))
        {
            Logger.Error<WaveReader>("Not a valid Wave file. Could not find RIFF at the start.");
            return false;
        }

        if (!wave.SequenceEqual("WAVE"u8))
        {
            Logger.Error<WaveReader>("Not a valid Wave file. Could not find WAVE.");
            return false;
        }

        while (reader.HasData)
        {
            var chunkId = reader.Read(4);
            var chunkSize = reader.Read<uint>();

            //NOTE(Jens): We can convert data, fmt etc to uint and compare it like that instead if this is to slow.
            if (chunkId.SequenceEqual("data"u8))
            {
                if (!pcmData.IsEmpty)
                {
                    Logger.Warning<WaveReader>("Multiple data fields. Not supported.");
                }
                pcmData = reader.Read(chunkSize);
            }
            else if (chunkId.SequenceEqual("fmt "u8))
            {
                var formatReader = new TitanBinaryReader(reader.Read(chunkSize));
                format.wFormatTag = formatReader.Read<ushort>();
                format.nChannels = formatReader.Read<ushort>();
                format.nSamplesPerSec = formatReader.Read<uint>();
                format.nAvgBytesPerSec = formatReader.Read<uint>();
                format.nBlockAlign = formatReader.Read<ushort>();
                format.wBitsPerSample = formatReader.Read<ushort>();
            }
            else
            {
                //Logger.Trace<WaveReader>($"{Encoding.ASCII.GetString(chunkId)} is not supported. Ignored.");
                //NOTE(Jens): Some chunks are not aligned correctly, make sure they are 2 bytes aligned.
                var alignedChunkSize = MemoryUtils.AlignToUpper(chunkSize, 2);
                reader.Read(alignedChunkSize);
            }
        }

        return true;
    }
}
