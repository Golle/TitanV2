using Titan.Core;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

internal struct VorbisSetup
{
    public TitanArray<VorbisCodebook> Codebooks;
    public TitanArray<VorbisFloorConfig> FloorConfig;
    public TitanArray<VorbisResidueConfig> ResidueConfig;
    public TitanArray<VorbisMapping> Mappings;
    public TitanArray<VorbisModeConfig> ModeConfig;
}