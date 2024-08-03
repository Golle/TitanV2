using Titan.Platform.Win32.DXGI;

namespace Titan.Rendering;

public static class Exstensions
{
    public static DXGI_FORMAT AsDXGIFormat(this RenderTargetFormat format) => format switch
    {
        RenderTargetFormat.RGBA8 => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
        RenderTargetFormat.BackBuffer => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN, // not sure how to handle this. 
        _ => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
    };
}
