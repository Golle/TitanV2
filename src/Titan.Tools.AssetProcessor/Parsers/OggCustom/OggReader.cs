using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;

namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct VorbisMetadataHeader1
{
    public byte PacketType;
    public fixed byte Vorbis[6];
    public uint VendorLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct VorbisHeader1
{
    public byte PacketType;
    public fixed byte Vorbis[6];
    public uint Version;
    public byte Channels;
    public uint SampleRate;
    public uint BitRateMax;
    public uint BitRateNominal;
    public uint BitRateMinimum;
    public byte BlocksizeInfo;
    public byte FramingFlag;

}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 27)]
internal struct OggHeader
{
    public uint CapturePattern;
    public byte Version;
    public OggHeaderFlags HeaderType;
    public ulong GranulePosition;
    public uint BitstreamSerialNumber;
    public uint PageSequenceNumber;
    public uint Checksum;
    public byte PageSegments;
    //public byte SegmentTable;
}

internal static unsafe class OggReader
{
    public static void Read(ReadOnlySpan<byte> file)
    {
        var reader = new TitanBinaryReader(file);
        var packetCount = 0;
        var page = 0;
Start:

        ref readonly var header = ref reader.Read<OggHeader>();

        //var segmentTable = (byte*)Unsafe.AsPointer(ref header.SegmentTable);

        var packetSize = 0;
        for (var i = 0; i < header.PageSegments; ++i)
        {
            var segmentSize = reader.Read<byte>();
            packetSize += segmentSize;

            // this is the end of this segment (could also be done after the loop)?
            if (segmentSize < 255)
            {
                if (page == 0)
                {
                    var s = sizeof(VorbisHeader1);

                    // Vorbis header
                    ref readonly var vorbisHeader = ref reader.Read<VorbisHeader1>();


                }
                else if (page == 1)
                {

                    ref readonly var metadata = ref reader.Read<VorbisMetadataHeader1>();
                    var vendorString = reader.Read(metadata.VendorLength);
                    var userCommentCount = reader.Read<uint>();
                    for (var j = 0; j < userCommentCount; ++j)
                    {
                        var length = reader.Read<uint>();
                        var comment = reader.Read(length);
                    }

                    var framingFlag = reader.Read<byte>();
                    // Vorbis Comment
                    //var temp = reader.Read(packetSize);
                }
                else if (page == 2)
                {
                    // Setup header
                    var temp = reader.Read(packetSize);

                }
                else
                {
                    var packet = reader.Read(packetSize);
                }

                packetSize = 0; // reset size of current packet
                packetCount++;
            }
        }

        if (header.HeaderType != OggHeaderFlags.EndOfStream)
        {
            page++;
            goto Start;
        }
        Logger.Error($"Woho! Packets = {packetCount}");

    }
}
