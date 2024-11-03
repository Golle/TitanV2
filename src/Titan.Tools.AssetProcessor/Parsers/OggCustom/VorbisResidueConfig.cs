using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

internal struct VorbisResidueConfig
{
    public short Type;
    public int Begin;
    public int End;
    public int PartitionSize;
    public byte Classifications;
    public byte Classbook;
    public TitanArray<Inline8<short>> ResidueBooks;
}
