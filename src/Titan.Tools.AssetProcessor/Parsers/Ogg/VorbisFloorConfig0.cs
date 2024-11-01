using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

internal struct VorbisFloorConfig0
{
    public int Order;
    public int Rate;
    public int BarkMapSize;
    public int AmplitudeBits;
    public int AmplitudeOffset;
    public int NumberOfBooks;
    public TitanArray<int> CodebookMappings;
}
