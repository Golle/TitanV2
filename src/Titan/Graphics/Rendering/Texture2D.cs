using Titan.Graphics.D3D12;

namespace Titan.Graphics.Rendering;

/// <summary>
/// The common interface for a Texture.
/// <remarks>This struct should never be passed by value</remarks>
/// <see cref="D3D12Texture2D"/> for the D3D12 implementation.
/// </summary>
public struct Texture2D
{
    public uint Width;
    public uint Height;
}
