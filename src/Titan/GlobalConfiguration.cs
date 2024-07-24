using System.Runtime.InteropServices;

namespace Titan;

internal enum Platforms
{
    Windows,
    Linux,
    MacOS
}

internal static class GlobalConfiguration
{
    public static readonly Platforms Platform = GetPlatform();
    public static readonly string BasePath = AppContext.BaseDirectory;

#if TRIPLE_BUFFERING
    public const uint MaxRenderFrames = 3;
#else
    public const uint MaxRenderFrames = 2;
#endif
    public const uint CommandBufferCount = 16;


    private static Platforms GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Platforms.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Platforms.MacOS;
        }
        return Platforms.Linux;
    }
}
