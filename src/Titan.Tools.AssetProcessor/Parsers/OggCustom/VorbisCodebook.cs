using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

internal struct VorbisCodebook
{
    public int MaxLength;
    public TitanArray<int> CodewordsLengths;
    public VorbisLookupTable LookupTable;
}