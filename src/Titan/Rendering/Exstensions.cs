using Titan.Platform.Win32.DXGI;

namespace Titan.Rendering;

public static class Exstensions
{
    public static DXGI_FORMAT AsDXGIFormat(this RenderTargetFormat format) => format switch
    {
        RenderTargetFormat.RGBA8 => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
        RenderTargetFormat.BackBuffer => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
        _ => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
    };

    public static DXGI_FORMAT AsDXGIFormat(this DepthBufferFormat format) => format switch
    {
        DepthBufferFormat.D32 => DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
        _ => DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT
    };
}
