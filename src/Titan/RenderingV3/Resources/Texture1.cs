using Titan.Rendering;

namespace Titan.RenderingV3.Resources;
public struct Texture1
{
    public uint Width;
    public uint Height;
    public DescriptorHandle RTV;
    public DescriptorHandle SRV;
    public DescriptorHandle UAV;
    public TextureFormat Format;
    public unsafe void* Resource;

    public bool IsRenderTarget() => RTV.IsValid;
    public bool IsShaderResource() => SRV.IsValid;
    public bool IsUnorderedAccess() => UAV.IsValid;
    public unsafe bool IsValid() => Resource != null;
}

public struct GPUBuffer1
{
    public uint Size;
    public DescriptorHandle SRV;
    public BufferType Type;

    public unsafe void* Resource;
    public bool IsShaderResource() => SRV.IsValid;
    public unsafe bool IsValid() => Resource != null;
}


public enum TextureFormat
{
    Unknown,

    // Render Target Formats
    R8,
    R32,
    RGBA8,
    BGRA8,
    RGBA16F,
    RGBA32F,


    //DXGI_FORMAT_R8_UNORM
    //DXGI_FORMAT_R8G8B8A8_UNORM
    //DXGI_FORMAT_B8G8R8A8_UNORM
    //DXGI_FORMAT_B8G8R8X8_UNORM

    // Depth Buffer Formats
    D32,

    // Other formats
    BC7,

    Count
}
