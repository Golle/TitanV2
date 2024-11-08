using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

internal struct VorbisLookupTable
{
    public int Type;
    public float MinValue;
    public float MaxValue;
    public bool SequenceP;
    public TitanArray<uint> LookupValues;
}
