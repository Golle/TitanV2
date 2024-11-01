namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

internal ref struct OggPage
{
    public byte Version;
    public OggHeaderFlags HeaderType;
    public ulong GranulePosition;
    public uint BitstreamSerialNumber;
    public uint PageSequenceNumber;
    public uint Checksum;
    public byte PageSegments;
    public ReadOnlySpan<byte> SegmentTable;
    public ReadOnlySpan<byte> Payload;
}
