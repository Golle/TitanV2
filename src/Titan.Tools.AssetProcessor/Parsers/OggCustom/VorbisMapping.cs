using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.OggCustom;

internal struct VorbisMapping
{
    public TitanArray<VorbisMappingChannel> Channels;
    public byte Submaps;
    public int CouplingSteps;
    public TitanArray<byte> SubmapFloors;
    public TitanArray<byte> SubmapResidues;
}