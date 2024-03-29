namespace Titan.Platform.Win32.D3D12;

[Flags]
public enum D3D12_TILE_COPY_FLAGS
{
    D3D12_TILE_COPY_FLAG_NONE = 0,
    D3D12_TILE_COPY_FLAG_NO_HAZARD = 0x1,
    D3D12_TILE_COPY_FLAG_LINEAR_BUFFER_TO_SWIZZLED_TILED_RESOURCE = 0x2,
    D3D12_TILE_COPY_FLAG_SWIZZLED_TILED_RESOURCE_TO_LINEAR_BUFFER = 0x4
}
