using Titan.Platform.Win32.DXGI;

namespace Titan.Rendering;

public static class Exstensions
{
    public static DXGI_FORMAT AsDXGIFormat(this RenderTargetFormat format) => format switch
    {
        RenderTargetFormat.R32 => DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT,
        RenderTargetFormat.R8 => DXGI_FORMAT.DXGI_FORMAT_R8_UNORM,
        RenderTargetFormat.RGBA8 => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
        RenderTargetFormat.RGBA16F => DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT,
        RenderTargetFormat.RGBA32F => DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,
        RenderTargetFormat.BackBuffer => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,

        _ => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
    };

    public static DXGI_FORMAT AsDXGIFormat(this DepthBufferFormat format) => format switch
    {
        DepthBufferFormat.D32 => DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
        _ => DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT
    };
}
