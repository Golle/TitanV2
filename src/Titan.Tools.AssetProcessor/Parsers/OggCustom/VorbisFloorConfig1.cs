using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

internal struct VorbisFloorConfig1
{
    public TitanArray<byte> PartitionClassList;
    public TitanArray<byte> ClassDimensions;
    public TitanArray<byte> Subclasses;
    public TitanArray<byte> Masterbooks;
    public TitanArray<TitanArray<short>> SubclassBooks;

    public byte Multiplier;
    public byte RangeBits;
    public TitanArray<int> XList;
}
