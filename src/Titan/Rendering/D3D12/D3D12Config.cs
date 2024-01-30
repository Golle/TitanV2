using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Platform.Win32.D3D;

namespace Titan.Rendering.D3D12;

public record D3D12Config(D3D_FEATURE_LEVEL FeatureLevel, bool AllowTearing, bool VSync, Color ClearColor) : IConfiguration, IDefault<D3D12Config>
{
    public const uint DefaultSRVCount = 1024;
    public const uint DefaultRTVCount = 1024;
    public const uint DefaultDSVCount = 1024;
    public const uint DefaultUAVCount = 1024;
    public const uint DefaultTempSRVCount = 256;
    public static readonly uint DefaultTempConstantBufferSize = MemoryUtils.MegaBytes(2);

    public static readonly Color DefaultClearColor = Color.FromRGB(0x001f54);

    public const uint DefaultMaxTextures = 1024;
    public const uint DefaultMaxMaterials = 1024;
    public const uint DefaultMaxBuffers = 1024;
    public const uint DefaultMaxPipelineStates = 256;
    public const uint DefaultMaxRootSignatures = 256;

    public GPUMemoryConfig MemoryConfig { get; init; }
    public ResourceConfig ResourceConfig { get; init; }

    public static D3D12Config Default => new(D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1, false, true, DefaultClearColor)
    {
        MemoryConfig = new(DefaultSRVCount, DefaultRTVCount, DefaultDSVCount, DefaultUAVCount, DefaultTempConstantBufferSize, DefaultTempSRVCount),
        ResourceConfig = new(DefaultMaxTextures, DefaultMaxMaterials, DefaultMaxBuffers, DefaultMaxPipelineStates, DefaultMaxRootSignatures)
    };
}
