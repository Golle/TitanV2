namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

internal struct VorbisHeader
{
    public uint Version;
    public byte Channels;
    public uint SampleRate;
    public uint BitRateMax;
    public uint BitRateNominal;
    public uint BitRateMin;
    public byte BlockSize0;
    public byte BlockSize1;
    public byte Framing;
}