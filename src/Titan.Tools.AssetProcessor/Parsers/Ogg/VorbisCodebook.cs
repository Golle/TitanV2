using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

internal struct VorbisCodebook
{
    public int MaxLength;
    public TitanArray<int> CodewordsLengths;
    public VorbisLookupTable LookupTable;
}